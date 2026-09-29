using Healthcare.Api.Infrastructure;
using Healthcare.Api.Services.Availability;
using Healthcare.Api.Security;
using Healthcare.Api.Configuration;
using Healthcare.Api.Data;
using Healthcare.Api.DTOs.Appointments;
using Healthcare.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Healthcare.Api.Services.Appointments;

public class AppointmentService : IAppointmentService
{
    private readonly HealthcareDbContext _context;
    private readonly ResourceAccess _access;
    private readonly HealthcareSettings _settings;
    private readonly IAvailabilityService _availability;

    public AppointmentService(
        HealthcareDbContext context,
        ResourceAccess access,
        IOptions<HealthcareSettings> options, IAvailabilityService availability)
    {
        _context = context;
        _access = access;
        _settings = options.Value;
        _availability = availability;
    }

    public async Task<IEnumerable<AppointmentDto>> GetAllAsync()
    {
        return await _access.List<Appointment>()
            .Select(a => new AppointmentDto
            {
                Id = a.Id,
                PatientName = a.Patient.FirstName + " " + a.Patient.LastName,
                DoctorName = a.Doctor.FirstName + " " + a.Doctor.LastName,
                LocationName = a.Location.Name,
                VisitTypeName = a.VisitType.Name,

                PatientId = a.PatientId,
                DoctorId = a.DoctorId,
                LocationId = a.LocationId,
                VisitTypeId = a.VisitTypeId,
                StartDatetime = a.StartDatetime,
                EndDatetime = a.EndDatetime,
                Status = a.Status,
                Notes = a.Notes,
                CreatedBy = a.CreatedBy,
                CreatedAt = a.CreatedAt,
                HasCompletedVisits = a.Visits.Any(v => v.Status == "completed"), HasVisits = a.Visits.Any(),
                UpdatedAt = a.UpdatedAt
            })
            .ToListAsync();
    }

    public async Task<AppointmentDto?> GetByIdAsync(int id)
    {
        return await _access.Query<Appointment>()
            .Where(a => a.Id == id)
            .Select(a => new AppointmentDto
            {
                Id = a.Id,
                PatientName = a.Patient.FirstName + " " + a.Patient.LastName,
                DoctorName = a.Doctor.FirstName + " " + a.Doctor.LastName,
                LocationName = a.Location.Name,
                VisitTypeName = a.VisitType.Name,

                PatientId = a.PatientId,
                DoctorId = a.DoctorId,
                LocationId = a.LocationId,
                VisitTypeId = a.VisitTypeId,
                StartDatetime = a.StartDatetime,
                EndDatetime = a.EndDatetime,
                Status = a.Status,
                Notes = a.Notes,
                CreatedBy = a.CreatedBy,
                CreatedAt = a.CreatedAt,
                HasCompletedVisits = a.Visits.Any(v => v.Status == "completed"), HasVisits = a.Visits.Any(),
                UpdatedAt = a.UpdatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<AppointmentDto> CreateAsync(
        CreateAppointmentDto dto,
        int createdBy)
    {
        if (!await _access.CanRead("Doctors", dto.DoctorId)) throw new ApiException(403, "Doctor is outside your assignment.");
        if (dto.Status != "booked") throw new ArgumentException("New appointments must start as booked.");
        var end = await ValidateAppointmentAsync(dto.PatientId, dto.DoctorId, dto.LocationId, dto.VisitTypeId, dto.StartDatetime);

        var now = DateTime.UtcNow;

        var appointment = new Appointment
        {
            PatientId = dto.PatientId,
            DoctorId = dto.DoctorId,
            LocationId = dto.LocationId,
            VisitTypeId = dto.VisitTypeId,
            StartDatetime = dto.StartDatetime,
            EndDatetime = end,
            Status = dto.Status,
            Notes = dto.Notes,
            CreatedBy = createdBy,
            CreatedAt = now,
            UpdatedAt = now
        };

        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();

        return (await GetByIdAsync(appointment.Id))!;
    }

    public async Task<AppointmentDto?> UpdateAsync(
        int id,
        UpdateAppointmentDto dto)
    {
        var appointment = await _context.Appointments.FindAsync(id);

        if (appointment is null)
        {
            return null;
        }

        if (dto.ExpectedUpdatedAt.HasValue && dto.ExpectedUpdatedAt.Value != appointment.UpdatedAt)
            throw new ApiException(409, "The appointment changed. Refresh before editing.");
        var hasCompletedVisit = await _context.Visits.AnyAsync(v => v.AppointmentId == id && v.Status == "completed");
        AppointmentLifecycle.Validate(appointment.Status, dto.Status, hasCompletedVisit);
        var moves = appointment.DoctorId != dto.DoctorId || appointment.LocationId != dto.LocationId ||
            appointment.VisitTypeId != dto.VisitTypeId || appointment.StartDatetime != dto.StartDatetime;
        var changesPatient = appointment.PatientId != dto.PatientId;
        var hasVisits = await _context.Visits.AnyAsync(v => v.AppointmentId == id);
        if ((moves || changesPatient) && hasVisits) throw new ApiException(409, "Appointments with visits cannot be reassigned or rescheduled.");
        if ((moves || changesPatient) && appointment.Status is "completed" or "cancelled") throw new ApiException(409, "Terminal appointments cannot be rescheduled or reassigned.");
        if ((changesPatient || appointment.DoctorId != dto.DoctorId) && await _context.FollowUps.AnyAsync(f => f.AppointmentId == id))
            throw new ApiException(409, "The appointment is linked to follow-ups and cannot be reassigned.");
        if (changesPatient && await _context.Consultations.AnyAsync(c => c.NextAppointmentId == id))
            throw new ApiException(409, "The appointment is referenced by a consultation and cannot change patient.");
        var end = appointment.EndDatetime;
        if (moves) end = await ValidateAppointmentAsync(dto.PatientId, dto.DoctorId, dto.LocationId, dto.VisitTypeId, dto.StartDatetime, id);
        else if (changesPatient && !await _context.Patients.AnyAsync(p => p.Id == dto.PatientId)) throw new ArgumentException("Patient does not exist.");

        appointment.PatientId = dto.PatientId;
        appointment.DoctorId = dto.DoctorId;
        appointment.LocationId = dto.LocationId;
        appointment.VisitTypeId = dto.VisitTypeId;
        appointment.StartDatetime = dto.StartDatetime;
        appointment.EndDatetime = end;
        appointment.Status = dto.Status;
        appointment.Notes = dto.Notes;
        appointment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return (await GetByIdAsync(appointment.Id))!;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var appointment = await _context.Appointments.FindAsync(id);

        if (appointment is null)
        {
            return false;
        }

        if (await _context.Visits.AnyAsync(v => v.AppointmentId == id))
            throw new ApiException(409, "An appointment with visits cannot be deleted.");
        _context.Appointments.Remove(appointment);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> StaffCanManageDoctorAsync(
    int staffUserId,
    int doctorId)
    {
        var staff = await _context.Users
            .AsNoTracking()
            .Where(u =>
                u.Id == staffUserId &&
                u.Role == "staff" &&
                u.IsActive)
            .Select(u => new
            {
                u.StaffDoctorId,
                u.StaffOrganizationId
            })
            .FirstOrDefaultAsync();

        if (staff is null)
        {
            return false;
        }

        // Secretary assigned directly to this doctor
        if (staff.StaffDoctorId.HasValue)
        {
            return staff.StaffDoctorId.Value == doctorId;
        }

        // Secretary assigned to an organization:
        // doctor must belong to that organization
        if (staff.StaffOrganizationId.HasValue)
        {
            return await _context.Doctors
                .AsNoTracking()
                .AnyAsync(d =>
                    d.Id == doctorId &&
                    d.OrganizationId ==
                        staff.StaffOrganizationId.Value);
        }

        return false;
    }

    public async Task<List<AppointmentDto>> GetByStaffUserIdAsync(
    int staffUserId)
    {
        var staff = await _context.Users
            .AsNoTracking()
            .Where(u =>
                u.Id == staffUserId &&
                u.Role == "staff" &&
                u.IsActive)
            .Select(u => new
            {
                u.StaffDoctorId,
                u.StaffOrganizationId
            })
            .FirstOrDefaultAsync();

        if (staff is null)
        {
            return [];
        }

        var query = _context.Appointments
            .AsNoTracking()
            .AsQueryable();

        if (staff.StaffDoctorId.HasValue)
        {
            query = query.Where(a =>
                a.DoctorId == staff.StaffDoctorId.Value);
        }
        else if (staff.StaffOrganizationId.HasValue)
        {
            query = query.Where(a =>
                a.Doctor.OrganizationId ==
                staff.StaffOrganizationId.Value);
        }
        else
        {
            return [];
        }

        return await query
            .Select(a => new AppointmentDto
            {
                Id = a.Id,
                PatientName = a.Patient.FirstName + " " + a.Patient.LastName,
                DoctorName = a.Doctor.FirstName + " " + a.Doctor.LastName,
                LocationName = a.Location.Name,
                VisitTypeName = a.VisitType.Name,

                PatientId = a.PatientId,
                DoctorId = a.DoctorId,
                LocationId = a.LocationId,
                VisitTypeId = a.VisitTypeId,
                StartDatetime = a.StartDatetime,
                EndDatetime = a.EndDatetime,
                Status = a.Status,
                Notes = a.Notes,
                CreatedBy = a.CreatedBy,
                CreatedAt = a.CreatedAt,
                HasCompletedVisits = a.Visits.Any(v => v.Status == "completed"), HasVisits = a.Visits.Any(),
                UpdatedAt = a.UpdatedAt
            })
            .ToListAsync();
    }

    public async Task<List<AppointmentDto>> GetByDoctorIdAsync(int doctorId)
    {
        return await _context.Appointments
            .AsNoTracking()
            .Where(a => a.DoctorId == doctorId)
            .Select(a => new AppointmentDto
            {
                Id = a.Id,
                PatientName = a.Patient.FirstName + " " + a.Patient.LastName,
                DoctorName = a.Doctor.FirstName + " " + a.Doctor.LastName,
                LocationName = a.Location.Name,
                VisitTypeName = a.VisitType.Name,

                PatientId = a.PatientId,
                DoctorId = a.DoctorId,
                LocationId = a.LocationId,
                VisitTypeId = a.VisitTypeId,
                StartDatetime = a.StartDatetime,
                EndDatetime = a.EndDatetime,
                Status = a.Status,
                Notes = a.Notes,
                CreatedBy = a.CreatedBy,
                CreatedAt = a.CreatedAt,
                HasCompletedVisits = a.Visits.Any(v => v.Status == "completed"), HasVisits = a.Visits.Any(),
                UpdatedAt = a.UpdatedAt
            })
            .ToListAsync();
    }

    private async Task<DateTime> ValidateAppointmentAsync(int patientId, int doctorId, int locationId, int visitTypeId, DateTime start, int? excludeId = null)
    {
        if (start.Kind != DateTimeKind.Utc) throw new ArgumentException("Appointment start must be a UTC timestamp ending in Z.");
        if (!await _context.Patients.AnyAsync(p => p.Id == patientId)) throw new ArgumentException("Patient does not exist.");
        var date = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(start, TimeZoneInfo.FindSystemTimeZoneById(_settings.TimeZone)));
        var slots = await _availability.GetAvailableSlotsAsync(doctorId, locationId, date, visitTypeId, excludeId);
        var slot = slots.FirstOrDefault(s => s.StartDatetime == start);
        if (slot is null) throw new ApiException(409, "This start time is unavailable. Check active resources, working hours, slot alignment and existing bookings.");
        return slot.EndDatetime;
    }

    public async Task<AppointmentDto?> ChangeStatusAsync(int id, ChangeAppointmentStatusDto dto)
    {
        var current = await GetByIdAsync(id);
        if (current is null) return null;
        return await UpdateAsync(id, new UpdateAppointmentDto { PatientId = current.PatientId, DoctorId = current.DoctorId,
            LocationId = current.LocationId, VisitTypeId = current.VisitTypeId, StartDatetime = current.StartDatetime,
            Notes = current.Notes, Status = dto.Status, ExpectedUpdatedAt = dto.ExpectedUpdatedAt });
    }

    public async Task<AppointmentDto?> RescheduleAsync(int id, RescheduleAppointmentDto dto)
    {
        var current = await GetByIdAsync(id);
        if (current is null) return null;
        return await UpdateAsync(id, new UpdateAppointmentDto { PatientId = current.PatientId, DoctorId = dto.DoctorId,
            LocationId = dto.LocationId, VisitTypeId = dto.VisitTypeId, StartDatetime = dto.StartDatetime,
            Notes = current.Notes, Status = current.Status, ExpectedUpdatedAt = dto.ExpectedUpdatedAt });
    }

    private static AppointmentDto ToDto(Appointment appointment)
    {
        return new AppointmentDto
        {
            Id = appointment.Id,
            PatientId = appointment.PatientId,
            DoctorId = appointment.DoctorId,
            LocationId = appointment.LocationId,
            VisitTypeId = appointment.VisitTypeId,
            StartDatetime = appointment.StartDatetime,
            EndDatetime = appointment.EndDatetime,
            Status = appointment.Status,
            Notes = appointment.Notes,
            CreatedBy = appointment.CreatedBy,
            CreatedAt = appointment.CreatedAt,
            UpdatedAt = appointment.UpdatedAt
        };
    }
}
