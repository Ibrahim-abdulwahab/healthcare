using Healthcare.Api.Data;
using Healthcare.Api.DTOs.Consultations;
using Healthcare.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Healthcare.Api.Services.Consultations;

public class ConsultationService : IConsultationService
{
    private readonly HealthcareDbContext _context;

    public ConsultationService(HealthcareDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ConsultationDto>> GetAllAsync()
    {
        return await _context.Consultations
            .AsNoTracking()
            .Select(c => new ConsultationDto
            {
                Id = c.Id,
                VisitId = c.VisitId,
                ReasonForVisit = c.ReasonForVisit,
                Diagnosis = c.Diagnosis,
                Notes = c.Notes,
                FollowUpPlan = c.FollowUpPlan,
                NextAppointmentId = c.NextAppointmentId,
                CreatedBy = c.CreatedBy,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            })
            .ToListAsync();
    }

    public async Task<ConsultationDto?> GetByIdAsync(int id)
    {
        return await _context.Consultations
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new ConsultationDto
            {
                Id = c.Id,
                VisitId = c.VisitId,
                ReasonForVisit = c.ReasonForVisit,
                Diagnosis = c.Diagnosis,
                Notes = c.Notes,
                FollowUpPlan = c.FollowUpPlan,
                NextAppointmentId = c.NextAppointmentId,
                CreatedBy = c.CreatedBy,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<ConsultationDto> CreateAsync(
        CreateConsultationDto dto)
    {
        await ValidateConsultationAsync(
            dto.VisitId,
            dto.NextAppointmentId,
            dto.CreatedBy
        );

        var existingConsultation = await _context.Consultations
            .AsNoTracking()
            .AnyAsync(c => c.VisitId == dto.VisitId);

        if (existingConsultation)
        {
            throw new ArgumentException(
                $"Visit with ID {dto.VisitId} already has a Consultation."
            );
        }

        var now = DateTime.UtcNow;

        var consultation = new Consultation
        {
            VisitId = dto.VisitId,
            ReasonForVisit = dto.ReasonForVisit,
            Diagnosis = dto.Diagnosis,
            Notes = dto.Notes,
            FollowUpPlan = dto.FollowUpPlan,
            NextAppointmentId = dto.NextAppointmentId,
            CreatedBy = dto.CreatedBy,
            CreatedAt = now,
            UpdatedAt = now
        };

        _context.Consultations.Add(consultation);
        await _context.SaveChangesAsync();

        return ToDto(consultation);
    }

    public async Task<ConsultationDto?> UpdateAsync(
        int id,
        UpdateConsultationDto dto)
    {
        var consultation = await _context.Consultations.FindAsync(id);

        if (consultation is null)
            return null;

        await ValidateConsultationAsync(
            dto.VisitId,
            dto.NextAppointmentId,
            dto.CreatedBy
        );

        var visitAlreadyUsed = await _context.Consultations
            .AsNoTracking()
            .AnyAsync(c =>
                c.VisitId == dto.VisitId &&
                c.Id != id
            );

        if (visitAlreadyUsed)
        {
            throw new ArgumentException(
                $"Visit with ID {dto.VisitId} already has a Consultation."
            );
        }

        consultation.VisitId = dto.VisitId;
        consultation.ReasonForVisit = dto.ReasonForVisit;
        consultation.Diagnosis = dto.Diagnosis;
        consultation.Notes = dto.Notes;
        consultation.FollowUpPlan = dto.FollowUpPlan;
        consultation.NextAppointmentId = dto.NextAppointmentId;
        consultation.CreatedBy = dto.CreatedBy;
        consultation.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return ToDto(consultation);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var consultation = await _context.Consultations.FindAsync(id);

        if (consultation is null)
            return false;

        _context.Consultations.Remove(consultation);
        await _context.SaveChangesAsync();

        return true;
    }

    private async Task ValidateConsultationAsync(
        int visitId,
        int? nextAppointmentId,
        int createdBy)
    {
        var visitExists = await _context.Visits
            .AsNoTracking()
            .AnyAsync(v => v.Id == visitId);

        if (!visitExists)
        {
            throw new ArgumentException(
                $"Visit with ID {visitId} does not exist."
            );
        }

        var userExists = await _context.Users
            .AsNoTracking()
            .AnyAsync(u => u.Id == createdBy);

        if (!userExists)
        {
            throw new ArgumentException(
                $"User with ID {createdBy} does not exist."
            );
        }

        if (nextAppointmentId.HasValue)
        {
            var appointmentExists = await _context.Appointments
                .AsNoTracking()
                .AnyAsync(a => a.Id == nextAppointmentId.Value);

            if (!appointmentExists)
            {
                throw new ArgumentException(
                    $"Appointment with ID {nextAppointmentId.Value} does not exist."
                );
            }
        }
    }

    private static ConsultationDto ToDto(Consultation consultation)
    {
        return new ConsultationDto
        {
            Id = consultation.Id,
            VisitId = consultation.VisitId,
            ReasonForVisit = consultation.ReasonForVisit,
            Diagnosis = consultation.Diagnosis,
            Notes = consultation.Notes,
            FollowUpPlan = consultation.FollowUpPlan,
            NextAppointmentId = consultation.NextAppointmentId,
            CreatedBy = consultation.CreatedBy,
            CreatedAt = consultation.CreatedAt,
            UpdatedAt = consultation.UpdatedAt
        };
    }
}