using Healthcare.Api.DTOs.Appointments;
using Healthcare.Api.DTOs.Reports;
using Healthcare.Api.Infrastructure;
using Healthcare.Api.Models;
using Healthcare.Api.Security;
using Microsoft.EntityFrameworkCore;

namespace Healthcare.Api.Services.Reports;

public class ReportService(ResourceAccess access)
{
    public async Task<ReportDto> GetAsync(DateTime from, DateTime to, int? doctorId, int? locationId, string? status, string? paymentMethod, string? paidTo)
    {
        if (from.Kind != DateTimeKind.Utc || to.Kind != DateTimeKind.Utc || to <= from || to - from > TimeSpan.FromDays(366))
            throw new ArgumentException("Use a UTC date range of at most 366 days (end exclusive).");
        if (doctorId.HasValue && !await access.CanRead("Doctors", doctorId.Value)) throw new ApiException(403, "Doctor is outside your scope.");
        if (locationId.HasValue && !await access.CanRead("Locations", locationId.Value)) throw new ApiException(403, "Location is outside your scope.");
        var appointments = access.Query<Appointment>().Where(a => (!doctorId.HasValue || a.DoctorId == doctorId) && (!locationId.HasValue || a.LocationId == locationId));
        var periodAppointments = appointments.Where(a => a.StartDatetime >= from && a.StartDatetime < to && (status == null || a.Status == status));
        var visits = access.Query<Visit>().Where(v => (!doctorId.HasValue || v.DoctorId == doctorId) && (!locationId.HasValue || v.LocationId == locationId));
        var periodVisits = visits.Where(v => (v.CheckInTime ?? v.CreatedAt) >= from && (v.CheckInTime ?? v.CreatedAt) < to);
        var consultations = access.Query<Consultation>().Where(c => c.CreatedAt >= from && c.CreatedAt < to && c.Visit.Status == "completed" && visits.Select(v => v.Id).Contains(c.VisitId));
        var doctors = access.Query<Doctor>().Where(d => !doctorId.HasValue || d.Id == doctorId);
        var activity = await doctors.OrderBy(d => d.Id).Select(d => new DoctorActivityDto(d.Id, d.FirstName + " " + d.LastName,
            periodAppointments.Count(a => a.DoctorId == d.Id), periodVisits.Count(v => v.DoctorId == d.Id), consultations.Count(c => c.Visit.DoctorId == d.Id))).ToListAsync();
        var counts = await periodAppointments.GroupBy(a => a.Status).Select(g => new StatusCountDto(g.Key, g.Count())).ToListAsync();
        var payments = access.Query<PaymentRecord>().Where(p => p.CreatedAt >= from && p.CreatedAt < to && visits.Select(v => v.Id).Contains(p.VisitId) &&
            (paymentMethod == null || p.PaymentMethod == paymentMethod) && (paidTo == null || p.PaidTo == paidTo));
        var collections = await payments.GroupBy(p => new { p.PaymentMethod, p.PaidTo }).Select(g => new CollectionTotalDto(g.Key.PaymentMethod, g.Key.PaidTo, g.Sum(p => p.Amount), g.Count())).ToListAsync();
        var now = DateTime.UtcNow;
        var upcoming = await periodAppointments.Where(a => a.StartDatetime >= now && a.Status != "cancelled" && a.Status != "completed")
            .OrderBy(a => a.StartDatetime).Take(50).Select(a => new AppointmentDto { Id = a.Id, PatientName = a.Patient.FirstName + " " + a.Patient.LastName, DoctorName = a.Doctor.FirstName + " " + a.Doctor.LastName, LocationName = a.Location.Name, VisitTypeName = a.VisitType.Name, PatientId = a.PatientId, DoctorId = a.DoctorId, LocationId = a.LocationId, VisitTypeId = a.VisitTypeId,
                StartDatetime = a.StartDatetime, EndDatetime = a.EndDatetime, Status = a.Status, CreatedBy = a.CreatedBy, CreatedAt = a.CreatedAt, UpdatedAt = a.UpdatedAt, HasVisits = a.Visits.Any() }).ToListAsync();
        return new(counts, activity, collections, upcoming);
    }
}
