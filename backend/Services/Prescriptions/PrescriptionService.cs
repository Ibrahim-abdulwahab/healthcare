using Healthcare.Api.Data;
using Healthcare.Api.DTOs.Prescriptions;
using Healthcare.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Healthcare.Api.Services.Prescriptions;

public class PrescriptionService : IPrescriptionService
{
    private readonly HealthcareDbContext _context;

    public PrescriptionService(HealthcareDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PrescriptionDto>> GetAllAsync()
    {
        return await _context.Prescriptions
            .AsNoTracking()
            .Select(p => new PrescriptionDto
            {
                Id = p.Id,
                ConsultationId = p.ConsultationId,
                PatientId = p.PatientId,
                DoctorId = p.DoctorId,
                PrescriptionDate = p.PrescriptionDate,
                Notes = p.Notes,
                Status = p.Status,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            })
            .ToListAsync();
    }

    public async Task<PrescriptionDto?> GetByIdAsync(int id)
    {
        return await _context.Prescriptions
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new PrescriptionDto
            {
                Id = p.Id,
                ConsultationId = p.ConsultationId,
                PatientId = p.PatientId,
                DoctorId = p.DoctorId,
                PrescriptionDate = p.PrescriptionDate,
                Notes = p.Notes,
                Status = p.Status,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<PrescriptionDto> CreateAsync(
        CreatePrescriptionDto dto)
    {
        await ValidatePrescriptionAsync(
            dto.ConsultationId,
            dto.PatientId,
            dto.DoctorId
        );

        var now = DateTime.UtcNow;

        var prescription = new Prescription
        {
            ConsultationId = dto.ConsultationId,
            PatientId = dto.PatientId,
            DoctorId = dto.DoctorId,
            PrescriptionDate = dto.PrescriptionDate,
            Notes = dto.Notes,
            Status = dto.Status,
            CreatedAt = now,
            UpdatedAt = now
        };

        _context.Prescriptions.Add(prescription);
        await _context.SaveChangesAsync();

        return ToDto(prescription);
    }

    public async Task<PrescriptionDto?> UpdateAsync(
        int id,
        UpdatePrescriptionDto dto)
    {
        var prescription = await _context.Prescriptions.FindAsync(id);

        if (prescription is null)
            return null;

        await ValidatePrescriptionAsync(
            dto.ConsultationId,
            dto.PatientId,
            dto.DoctorId
        );

        prescription.ConsultationId = dto.ConsultationId;
        prescription.PatientId = dto.PatientId;
        prescription.DoctorId = dto.DoctorId;
        prescription.PrescriptionDate = dto.PrescriptionDate;
        prescription.Notes = dto.Notes;
        prescription.Status = dto.Status;
        prescription.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return ToDto(prescription);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var prescription = await _context.Prescriptions.FindAsync(id);

        if (prescription is null)
            return false;

        _context.Prescriptions.Remove(prescription);
        await _context.SaveChangesAsync();

        return true;
    }

    private async Task ValidatePrescriptionAsync(
        int consultationId,
        int patientId,
        int doctorId)
    {
        var consultation = await _context.Consultations
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == consultationId);

        if (consultation is null)
        {
            throw new ArgumentException(
                $"Consultation with ID {consultationId} does not exist."
            );
        }

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

        var visit = await _context.Visits
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == consultation.VisitId);

        if (visit is null)
        {
            throw new ArgumentException(
                $"Visit with ID {consultation.VisitId} does not exist."
            );
        }

        if (visit.PatientId != patientId)
        {
            throw new ArgumentException(
                "The Prescription patient must match the Consultation's Visit patient."
            );
        }

        if (visit.DoctorId != doctorId)
        {
            throw new ArgumentException(
                "The Prescription doctor must match the Consultation's Visit doctor."
            );
        }
    }

    private static PrescriptionDto ToDto(Prescription prescription)
    {
        return new PrescriptionDto
        {
            Id = prescription.Id,
            ConsultationId = prescription.ConsultationId,
            PatientId = prescription.PatientId,
            DoctorId = prescription.DoctorId,
            PrescriptionDate = prescription.PrescriptionDate,
            Notes = prescription.Notes,
            Status = prescription.Status,
            CreatedAt = prescription.CreatedAt,
            UpdatedAt = prescription.UpdatedAt
        };
    }
}