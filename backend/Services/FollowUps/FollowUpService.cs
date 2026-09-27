using Healthcare.Api.Security;
using Healthcare.Api.Data;
using Healthcare.Api.DTOs.FollowUps;
using Healthcare.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Healthcare.Api.Services.FollowUps;

public class FollowUpService : IFollowUpService
{
    private readonly HealthcareDbContext _context;
    private readonly ResourceAccess _access;

    public FollowUpService(HealthcareDbContext context, ResourceAccess access)
    {
        _context = context;
        _access = access;
    }

    public async Task<IEnumerable<FollowUpDto>> GetAllAsync()
    {
        return await _access.List<FollowUp>()
            .Select(f => new FollowUpDto
            {
                Id = f.Id,
                PatientId = f.PatientId,
                DoctorId = f.DoctorId,
                AppointmentId = f.AppointmentId,
                Notes = f.Notes,
                ReminderDate = f.ReminderDate,
                Status = f.Status,
                CreatedAt = f.CreatedAt,
                UpdatedAt = f.UpdatedAt
            })
            .ToListAsync();
    }

    public async Task<FollowUpDto?> GetByIdAsync(int id)
    {
        return await _access.Query<FollowUp>()
            .Where(f => f.Id == id)
            .Select(f => new FollowUpDto
            {
                Id = f.Id,
                PatientId = f.PatientId,
                DoctorId = f.DoctorId,
                AppointmentId = f.AppointmentId,
                Notes = f.Notes,
                ReminderDate = f.ReminderDate,
                Status = f.Status,
                CreatedAt = f.CreatedAt,
                UpdatedAt = f.UpdatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<FollowUpDto> CreateAsync(CreateFollowUpDto dto)
    {
        await ValidateFollowUpAsync(
            dto.PatientId,
            dto.DoctorId,
            dto.AppointmentId
        );

        var now = DateTime.UtcNow;

        var followUp = new FollowUp
        {
            PatientId = dto.PatientId,
            DoctorId = dto.DoctorId,
            AppointmentId = dto.AppointmentId,
            Notes = dto.Notes,
            ReminderDate = dto.ReminderDate,
            Status = dto.Status,
            CreatedAt = now,
            UpdatedAt = now
        };

        _context.FollowUps.Add(followUp);
        await _context.SaveChangesAsync();

        return ToDto(followUp);
    }

    public async Task<FollowUpDto?> UpdateAsync(
        int id,
        UpdateFollowUpDto dto)
    {
        var followUp = await _context.FollowUps.FindAsync(id);

        if (followUp is null)
            return null;

        await ValidateFollowUpAsync(
            dto.PatientId,
            dto.DoctorId,
            dto.AppointmentId
        );

        followUp.PatientId = dto.PatientId;
        followUp.DoctorId = dto.DoctorId;
        followUp.AppointmentId = dto.AppointmentId;
        followUp.Notes = dto.Notes;
        followUp.ReminderDate = dto.ReminderDate;
        followUp.Status = dto.Status;
        followUp.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return ToDto(followUp);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var followUp = await _context.FollowUps.FindAsync(id);

        if (followUp is null)
            return false;

        _context.FollowUps.Remove(followUp);
        await _context.SaveChangesAsync();

        return true;
    }

    private async Task ValidateFollowUpAsync(
        int patientId,
        int doctorId,
        int? appointmentId)
    {
        var patientExists = await _context.Patients
            .AsNoTracking()
            .AnyAsync(p => p.Id == patientId);

        if (!patientExists)
        {
            throw new ArgumentException(
                $"Patient with ID {patientId} does not exist."
            );
        }

        var doctorExists = await _context.Doctors
            .AsNoTracking()
            .AnyAsync(d => d.Id == doctorId);

        if (!doctorExists)
        {
            throw new ArgumentException(
                $"Doctor with ID {doctorId} does not exist."
            );
        }

        if (appointmentId.HasValue)
        {
            var appointment = await _context.Appointments
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == appointmentId.Value);

            if (appointment is null)
            {
                throw new ArgumentException(
                    $"Appointment with ID {appointmentId.Value} does not exist."
                );
            }

            if (appointment.PatientId != patientId)
            {
                throw new ArgumentException(
                    "The FollowUp patient must match the Appointment patient."
                );
            }

            if (appointment.DoctorId != doctorId)
            {
                throw new ArgumentException(
                    "The FollowUp doctor must match the Appointment doctor."
                );
            }
        }
    }

    private static FollowUpDto ToDto(FollowUp followUp)
    {
        return new FollowUpDto
        {
            Id = followUp.Id,
            PatientId = followUp.PatientId,
            DoctorId = followUp.DoctorId,
            AppointmentId = followUp.AppointmentId,
            Notes = followUp.Notes,
            ReminderDate = followUp.ReminderDate,
            Status = followUp.Status,
            CreatedAt = followUp.CreatedAt,
            UpdatedAt = followUp.UpdatedAt
        };
    }
}