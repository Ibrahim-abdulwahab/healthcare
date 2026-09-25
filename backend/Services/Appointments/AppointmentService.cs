using Healthcare.Api.Data;
using Healthcare.Api.DTOs.Appointments;
using Healthcare.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Healthcare.Api.Services.Appointments;

public class AppointmentService : IAppointmentService
{
    private readonly HealthcareDbContext _context;

    public AppointmentService(HealthcareDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<AppointmentDto>> GetAllAsync()
    {
        return await _context.Appointments
            .AsNoTracking()
            .Select(a => new AppointmentDto
            {
                Id = a.Id,
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
                UpdatedAt = a.UpdatedAt
            })
            .ToListAsync();
    }

    public async Task<AppointmentDto?> GetByIdAsync(int id)
    {
        return await _context.Appointments
            .AsNoTracking()
            .Where(a => a.Id == id)
            .Select(a => new AppointmentDto
            {
                Id = a.Id,
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
                UpdatedAt = a.UpdatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<AppointmentDto> CreateAsync(CreateAppointmentDto dto)
    {
        await ValidateAppointmentAsync(
            dto.PatientId,
            dto.DoctorId,
            dto.LocationId,
            dto.VisitTypeId,
            dto.CreatedBy,
            dto.StartDatetime,
            dto.EndDatetime
        );

        var now = DateTime.UtcNow;

        var appointment = new Appointment
        {
            PatientId = dto.PatientId,
            DoctorId = dto.DoctorId,
            LocationId = dto.LocationId,
            VisitTypeId = dto.VisitTypeId,
            StartDatetime = dto.StartDatetime,
            EndDatetime = dto.EndDatetime,
            Status = dto.Status,
            Notes = dto.Notes,
            CreatedBy = dto.CreatedBy,
            CreatedAt = now,
            UpdatedAt = now
        };

        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();

        return ToDto(appointment);
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

        await ValidateAppointmentAsync(
            dto.PatientId,
            dto.DoctorId,
            dto.LocationId,
            dto.VisitTypeId,
            dto.CreatedBy,
            dto.StartDatetime,
            dto.EndDatetime,
            id
        );

        appointment.PatientId = dto.PatientId;
        appointment.DoctorId = dto.DoctorId;
        appointment.LocationId = dto.LocationId;
        appointment.VisitTypeId = dto.VisitTypeId;
        appointment.StartDatetime = dto.StartDatetime;
        appointment.EndDatetime = dto.EndDatetime;
        appointment.Status = dto.Status;
        appointment.Notes = dto.Notes;
        appointment.CreatedBy = dto.CreatedBy;
        appointment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return ToDto(appointment);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var appointment = await _context.Appointments.FindAsync(id);

        if (appointment is null)
        {
            return false;
        }

        _context.Appointments.Remove(appointment);
        await _context.SaveChangesAsync();

        return true;
    }

    private async Task ValidateAppointmentAsync(
        int patientId,
        int doctorId,
        int locationId,
        int visitTypeId,
        int createdBy,
        DateTime startDatetime,
        DateTime endDatetime,
    int? excludeAppointmentId = null)
    {
        if (!await _context.Patients.AnyAsync(p => p.Id == patientId))
            throw new ArgumentException(
                $"Patient with ID {patientId} does not exist."
            );

        if (!await _context.Doctors.AnyAsync(d => d.Id == doctorId))
            throw new ArgumentException(
                $"Doctor with ID {doctorId} does not exist."
            );

        if (!await _context.Locations.AnyAsync(l => l.Id == locationId))
            throw new ArgumentException(
                $"Location with ID {locationId} does not exist."
            );

        var doctorLocation = await _context.DoctorLocations
            .AsNoTracking()
            .FirstOrDefaultAsync(dl =>
                dl.DoctorId == doctorId &&
                dl.LocationId == locationId &&
                dl.IsActive
            );

        if (doctorLocation is null)
        {
            throw new ArgumentException(
                $"Doctor with ID {doctorId} is not assigned to active Location with ID {locationId}."
            );
        }

        if (!await _context.VisitTypes.AnyAsync(v => v.Id == visitTypeId))
            throw new ArgumentException(
                $"Visit Type with ID {visitTypeId} does not exist."
            );

        if (!await _context.Users.AnyAsync(u => u.Id == createdBy))
            throw new ArgumentException(
                $"User with ID {createdBy} does not exist."
            );

        if (endDatetime <= startDatetime)
            throw new ArgumentException(
                "Appointment end time must be after its start time."
            );

        if (startDatetime.Date != endDatetime.Date)
        {
            throw new ArgumentException(
                "An appointment must start and end on the same date."
            );
        }

        var dayOfWeek = (int)startDatetime.DayOfWeek;
        var startTime = TimeOnly.FromDateTime(startDatetime);
        var endTime = TimeOnly.FromDateTime(endDatetime);

        var hasValidSchedule = await _context.Schedules
            .AsNoTracking()
            .AnyAsync(s =>
                s.DoctorLocationId == doctorLocation.Id &&
                s.DayOfWeek == dayOfWeek &&
                s.IsActive &&
                s.StartTime <= startTime &&
                s.EndTime >= endTime
            );

        if (!hasValidSchedule)
        {
            throw new ArgumentException(
                "The requested appointment time is outside the doctor's active schedule for this location."
            );
        }

        var appointmentDate = DateOnly.FromDateTime(startDatetime);

        var blockingExceptionExists = await _context.ScheduleExceptions
            .AsNoTracking()
            .AnyAsync(se =>
                se.DoctorLocationId == doctorLocation.Id &&
                se.Date == appointmentDate &&
                !se.IsAvailable &&
                startTime < se.EndTime &&
                endTime > se.StartTime
            );

        if (blockingExceptionExists)
        {
            throw new ArgumentException(
                "The requested appointment time overlaps with a period when the doctor is unavailable."
            );
        }

        var overlappingAppointmentExists = await _context.Appointments
            .AsNoTracking()
            .AnyAsync(a =>
                a.DoctorId == doctorId &&
                a.Id != excludeAppointmentId &&
                a.Status != "cancelled" &&
                startDatetime < a.EndDatetime &&
                endDatetime > a.StartDatetime
            );

        if (overlappingAppointmentExists)
        {
            throw new ArgumentException(
                "The doctor already has another appointment during the requested time."
            );
        }
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