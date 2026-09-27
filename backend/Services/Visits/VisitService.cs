using Healthcare.Api.Infrastructure;
using Healthcare.Api.Security;
using Healthcare.Api.Data;
using Healthcare.Api.DTOs.Visits;
using Healthcare.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Healthcare.Api.Services.Visits;

public class VisitService : IVisitService
{
    private readonly HealthcareDbContext _context;
    private readonly ResourceAccess _access;

    public VisitService(HealthcareDbContext context, ResourceAccess access)
    {
        _context = context;
        _access = access;
    }

    public async Task<IEnumerable<VisitDto>> GetAllAsync()
    {
        return await _access.List<Visit>()
            .Select(v => new VisitDto
            {
                Id = v.Id,
                PatientName = v.Patient.FirstName + " " + v.Patient.LastName,
                DoctorName = v.Doctor.FirstName + " " + v.Doctor.LastName,
                LocationName = v.Location.Name,

                AppointmentId = v.AppointmentId,
                PatientId = v.PatientId,
                DoctorId = v.DoctorId,
                LocationId = v.LocationId,
                Status = v.Status,
                CheckInTime = v.CheckInTime,
                CheckOutTime = v.CheckOutTime,
                CreatedAt = v.CreatedAt,
                UpdatedAt = v.UpdatedAt
            })
            .ToListAsync();
    }

    public async Task<VisitDto?> GetByIdAsync(int id)
    {
        return await _access.Query<Visit>()
            .Where(v => v.Id == id)
            .Select(v => new VisitDto
            {
                Id = v.Id,
                PatientName = v.Patient.FirstName + " " + v.Patient.LastName,
                DoctorName = v.Doctor.FirstName + " " + v.Doctor.LastName,
                LocationName = v.Location.Name,

                AppointmentId = v.AppointmentId,
                PatientId = v.PatientId,
                DoctorId = v.DoctorId,
                LocationId = v.LocationId,
                Status = v.Status,
                CheckInTime = v.CheckInTime,
                CheckOutTime = v.CheckOutTime,
                CreatedAt = v.CreatedAt,
                UpdatedAt = v.UpdatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<VisitDto> CreateAsync(CreateVisitDto dto)
    {
        await ValidateVisitAsync(
            dto.AppointmentId,
            dto.PatientId,
            dto.DoctorId,
            dto.LocationId,
            dto.CheckInTime,
            dto.CheckOutTime
        );

        var now = DateTime.UtcNow;

        var visit = new Visit
        {
            AppointmentId = dto.AppointmentId,
            PatientId = dto.PatientId,
            DoctorId = dto.DoctorId,
            LocationId = dto.LocationId,
            Status = dto.Status,
            CheckInTime = dto.CheckInTime,
            CheckOutTime = dto.CheckOutTime,
            CreatedAt = now,
            UpdatedAt = now
        };

        _context.Visits.Add(visit);
        await _context.SaveChangesAsync();

        return ToDto(visit);
    }

    public async Task<VisitDto?> UpdateAsync(int id, UpdateVisitDto dto)
    {
        var visit = await _context.Visits.FindAsync(id);

        if (visit is null)
            return null;

        if ((visit.PatientId != dto.PatientId || visit.DoctorId != dto.DoctorId || visit.LocationId != dto.LocationId || visit.AppointmentId != dto.AppointmentId) &&
            (await _context.Consultations.AnyAsync(c => c.VisitId == id) || await _context.PaymentRecords.AnyAsync(p => p.VisitId == id) ||
             await _context.LabRequests.AnyAsync(l => l.VisitId == id) || await _context.ImagingRequests.AnyAsync(i => i.VisitId == id)))
            throw new ApiException(409, "A visit with clinical or payment records cannot be reassigned.");
        await ValidateVisitAsync(
            dto.AppointmentId,
            dto.PatientId,
            dto.DoctorId,
            dto.LocationId,
            dto.CheckInTime,
            dto.CheckOutTime
        );

        visit.AppointmentId = dto.AppointmentId;
        visit.PatientId = dto.PatientId;
        visit.DoctorId = dto.DoctorId;
        visit.LocationId = dto.LocationId;
        visit.Status = dto.Status;
        visit.CheckInTime = dto.CheckInTime;
        visit.CheckOutTime = dto.CheckOutTime;
        visit.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return ToDto(visit);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var visit = await _context.Visits.FindAsync(id);

        if (visit is null)
            return false;

        _context.Visits.Remove(visit);
        await _context.SaveChangesAsync();

        return true;
    }

    private async Task ValidateVisitAsync(
        int appointmentId,
        int patientId,
        int doctorId,
        int locationId,
        DateTime? checkInTime,
        DateTime? checkOutTime)
    {
        var appointment = await _context.Appointments
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == appointmentId);

        if (appointment is null)
            throw new ArgumentException(
                $"Appointment with ID {appointmentId} does not exist."
            );

        if (appointment.PatientId != patientId)
            throw new ArgumentException(
                "The Visit patient must match the Appointment patient."
            );

        if (appointment.DoctorId != doctorId)
            throw new ArgumentException(
                "The Visit doctor must match the Appointment doctor."
            );

        if (appointment.LocationId != locationId)
            throw new ArgumentException(
                "The Visit location must match the Appointment location."
            );

        if ((checkInTime.HasValue && checkInTime.Value.Kind != DateTimeKind.Utc) ||
            (checkOutTime.HasValue && checkOutTime.Value.Kind != DateTimeKind.Utc))
            throw new ArgumentException("Visit timestamps must be UTC (Z).");
        if (checkInTime.HasValue &&
            checkOutTime.HasValue &&
            checkOutTime.Value < checkInTime.Value)
        {
            throw new ArgumentException(
                "Check-out time cannot be earlier than check-in time."
            );
        }
    }

    private static VisitDto ToDto(Visit visit)
    {
        return new VisitDto
        {
            Id = visit.Id,
            AppointmentId = visit.AppointmentId,
            PatientId = visit.PatientId,
            DoctorId = visit.DoctorId,
            LocationId = visit.LocationId,
            Status = visit.Status,
            CheckInTime = visit.CheckInTime,
            CheckOutTime = visit.CheckOutTime,
            CreatedAt = visit.CreatedAt,
            UpdatedAt = visit.UpdatedAt
        };
    }
}