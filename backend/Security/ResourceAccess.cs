using Healthcare.Api.Data;
using Healthcare.Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Healthcare.Api.Security;

// Shared SQL scopes for lists, individual resources, target parents, files and reports.
public sealed class ResourceAccess(HealthcareDbContext db, RequestUser user, IHttpContextAccessor http)
{
    public IQueryable<Doctor> Doctors => db.Doctors.Where(d => user.IsAdmin || d.Id == user.DoctorId || d.OrganizationId == user.OrganizationId);
    public IQueryable<T> Query<T>() where T : class
    {
        var all = db.Set<T>().AsNoTracking();
        if (user.Id == 0) return all.Where(_ => false);
        if (user.IsAdmin && typeof(T) != typeof(HealthcareFile)) return all;
        var doctors = Doctors.Select(d => d.Id);
        var appointments = db.Appointments.Where(a => doctors.Contains(a.DoctorId));
        var visits = db.Visits.Where(v => doctors.Contains(v.DoctorId));
        var consultations = db.Consultations.Where(c => visits.Select(v => v.Id).Contains(c.VisitId));
        var prescriptions = db.Prescriptions.Where(p => doctors.Contains(p.DoctorId) && consultations.Select(c => c.Id).Contains(p.ConsultationId));
        var patients = db.Patients.Where(p => appointments.Any(a => a.PatientId == p.Id && a.Status != "cancelled") || visits.Any(v => v.PatientId == p.Id));
        object scoped = typeof(T).Name switch
        {
            nameof(Doctor) => Doctors,
            nameof(Location) => db.Locations.Where(l => user.OrganizationId.HasValue ? l.OrganizationId == user.OrganizationId : l.DoctorLocations.Any(dl => doctors.Contains(dl.DoctorId))),
            nameof(DoctorLocation) => db.DoctorLocations.Where(dl => doctors.Contains(dl.DoctorId)),
            nameof(Schedule) => db.Schedules.Where(s => doctors.Contains(s.DoctorLocation.DoctorId)),
            nameof(ScheduleException) => db.ScheduleExceptions.Where(s => doctors.Contains(s.DoctorLocation.DoctorId)),
            nameof(Patient) => patients,
            nameof(Appointment) => appointments,
            nameof(Visit) => visits,
            nameof(Consultation) => consultations,
            nameof(FollowUp) => db.FollowUps.Where(f => doctors.Contains(f.DoctorId)),
            nameof(Prescription) => prescriptions,
            nameof(PrescriptionItem) => db.PrescriptionItems.Where(p => prescriptions.Select(x => x.Id).Contains(p.PrescriptionId)),
            nameof(LabRequest) => db.LabRequests.Where(l => visits.Select(v => v.Id).Contains(l.VisitId)),
            nameof(ImagingRequest) => db.ImagingRequests.Where(l => visits.Select(v => v.Id).Contains(l.VisitId)),
            nameof(PaymentRecord) => db.PaymentRecords.Where(p => visits.Select(v => v.Id).Contains(p.VisitId)),
            nameof(Notification) => db.Notifications.Where(n => n.UserId == user.Id),
            nameof(HealthcareFile) => FileQuery(),
            nameof(CodeList) or nameof(Item) or nameof(ItemCategory) or nameof(VisitType) => all,
            _ => all.Where(_ => false)
        };
        return ((IQueryable<T>)scoped).AsNoTracking();
    }

    private IQueryable<HealthcareFile> FileQuery()
    {
        var patients = Query<Patient>().Select(p => p.Id);
        var visits = Query<Visit>().Select(p => p.Id);
        var consultations = Query<Consultation>().Select(p => p.Id);
        var prescriptions = Query<Prescription>().Select(p => p.Id);
        var followUps = Query<FollowUp>().Select(p => p.Id);
        var labs = Query<LabRequest>().Select(p => p.Id);
        var imaging = Query<ImagingRequest>().Select(p => p.Id);
        return db.Files.Where(f =>
            (f.TableName == "patients" && patients.Contains(f.RecordId)) ||
            (f.TableName == "visits" && visits.Contains(f.RecordId)) ||
            (f.TableName == "consultations" && consultations.Contains(f.RecordId)) ||
            (f.TableName == "prescriptions" && prescriptions.Contains(f.RecordId)) ||
            (f.TableName == "follow_ups" && followUps.Contains(f.RecordId)) ||
            (f.TableName == "lab_requests" && labs.Contains(f.RecordId)) ||
            (f.TableName == "imaging_requests" && imaging.Contains(f.RecordId)));
    }

    public IQueryable<T> List<T>() where T : class
    {
        var query = Query<T>();
        var args = http.HttpContext?.Request.Query;
        if (args is null) return query.OrderBy(x => EF.Property<int>(x, "Id")).Take(100);
        foreach (var field in new[] { "DoctorId", "PatientId", "LocationId", "VisitId", "ConsultationId", "PrescriptionId", "DoctorLocationId", "RecordId", "UserId" })
        {
            if (typeof(T).GetProperty(field)?.PropertyType == typeof(int) && args.TryGetValue(field, out var value))
            {
                if (!int.TryParse(value, out var id) || id <= 0) throw new ArgumentException($"Invalid {field}.");
                query = query.Where(x => EF.Property<int>(x, field) == id);
            }
        }
        var search = args["search"].ToString().Trim();
        if (args.TryGetValue("patientId", out var patientValue) && !typeof(T).GetProperties().Any(p => p.Name == "PatientId"))
        {
            if (!int.TryParse(patientValue, out var patientId) || patientId <= 0) throw new ArgumentException("Invalid patientId.");
            object filtered = typeof(T).Name switch
            {
                nameof(Consultation) => ((IQueryable<Consultation>)(object)query).Where(c => c.Visit.PatientId == patientId),
                nameof(PrescriptionItem) => ((IQueryable<PrescriptionItem>)(object)query).Where(p => p.Prescription.PatientId == patientId),
                nameof(LabRequest) => ((IQueryable<LabRequest>)(object)query).Where(p => p.Visit.PatientId == patientId),
                nameof(ImagingRequest) => ((IQueryable<ImagingRequest>)(object)query).Where(p => p.Visit.PatientId == patientId),
                nameof(PaymentRecord) => ((IQueryable<PaymentRecord>)(object)query).Where(p => p.Visit.PatientId == patientId),
                _ => query
            };
            query = (IQueryable<T>)filtered;
        }
        if (search.Length > 0)
        {
            if (typeof(T) == typeof(Patient)) query = (IQueryable<T>)(object)((IQueryable<Patient>)(object)query).Where(p => (p.FirstName + " " + p.LastName).ToLower().Contains(search.ToLower()) || p.Mrn.ToLower().Contains(search.ToLower()) || (p.Phone != null && p.Phone.Contains(search)));
            else
            {
                var parameter = Expression.Parameter(typeof(T), "record");
                Expression match = Expression.Constant(false);
                foreach (var name in new[] { "Name", "FirstName", "LastName", "Email", "Code", "Category", "Key", "ValueEn", "ValueAr", "ReasonForVisit", "Diagnosis", "TestName", "ServiceName", "Notes", "Status", "PaymentMethod", "Action", "TableName" })
                {
                    if (typeof(T).GetProperty(name)?.PropertyType != typeof(string)) continue;
                    var property = Expression.Property(parameter, name);
                    var contains = Expression.Call(Expression.Call(property, nameof(string.ToLower), Type.EmptyTypes), nameof(string.Contains), Type.EmptyTypes, Expression.Constant(search.ToLowerInvariant()));
                    match = Expression.OrElse(match, Expression.AndAlso(Expression.NotEqual(property, Expression.Constant(null, typeof(string))), contains));
                }
                query = query.Where(Expression.Lambda<Func<T, bool>>(match, parameter));
            }
        }
        if (typeof(T) == typeof(Appointment))
        {
            var a = (IQueryable<Appointment>)(object)query;
            if (args.TryGetValue("status", out var status)) a = a.Where(x => x.Status == status.ToString());
            if (args.TryGetValue("from", out var from)) { var instant = ParseUtc(from.ToString()); a = a.Where(x => x.EndDatetime > instant); }
            if (args.TryGetValue("to", out var to)) { var instant = ParseUtc(to.ToString()); a = a.Where(x => x.StartDatetime < instant); }
            query = (IQueryable<T>)(object)a;
        }
        if (typeof(T) == typeof(HealthcareFile) && args.TryGetValue("tableName", out var table))
            query = query.Where(x => EF.Property<string>(x, "TableName") == table.ToString());
        var page = Number("page", 1, int.MaxValue / 200);
        var size = Number("pageSize", 100, 200);
        return query.OrderBy(x => EF.Property<int>(x, "Id")).Skip((page - 1) * size).Take(size);

        int Number(string key, int fallback, int max) => !args.ContainsKey(key) ? fallback : int.TryParse(args[key], out var n) && n > 0 && n <= max ? n : throw new ArgumentException($"Invalid {key}.");
    }

    public static DateTime ParseUtc(string text) => DateTimeOffset.TryParse(text, out var date) && (text.EndsWith('Z') || text.LastIndexOf('+') > 9 || text.LastIndexOf('-') > 9)
        ? date.UtcDateTime : throw new ArgumentException("Timestamp must include Z or a UTC offset.");

    public Task<bool> CanRead(string resource, int id) => resource switch
    {
        "Doctors" => Exists<Doctor>(id), "Locations" => Exists<Location>(id), "DoctorLocations" => Exists<DoctorLocation>(id),
        "Schedules" => Exists<Schedule>(id), "ScheduleExceptions" => Exists<ScheduleException>(id), "Patients" => Exists<Patient>(id),
        "Appointments" => Exists<Appointment>(id), "Visits" => Exists<Visit>(id), "Consultations" => Exists<Consultation>(id),
        "FollowUps" => Exists<FollowUp>(id), "Prescriptions" => Exists<Prescription>(id), "PrescriptionItems" => Exists<PrescriptionItem>(id),
        "LabRequests" => Exists<LabRequest>(id), "ImagingRequests" => Exists<ImagingRequest>(id), "PaymentRecords" => Exists<PaymentRecord>(id),
        "HealthcareFiles" => Exists<HealthcareFile>(id), "Notifications" => Exists<Notification>(id),
        "CodeLists" => Exists<CodeList>(id), "Items" => Exists<Item>(id), "ItemCategories" => Exists<ItemCategory>(id), "VisitTypes" => Exists<VisitType>(id),
        _ => Task.FromResult(user.IsAdmin)
    };
    private Task<bool> Exists<T>(int id) where T : class => Query<T>().AnyAsync(x => EF.Property<int>(x, "Id") == id);

    public static string FileParent(string table) => table switch
    {
        "patients" => "Patients", "visits" => "Visits", "consultations" => "Consultations", "prescriptions" => "Prescriptions",
        "follow_ups" => "FollowUps", "lab_requests" => "LabRequests", "imaging_requests" => "ImagingRequests",
        _ => throw new ArgumentException("Unsupported file parent type.")
    };
}
