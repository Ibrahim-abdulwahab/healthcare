using Healthcare.Api.Data;
using Healthcare.Api.DTOs.Patients;
using Healthcare.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Healthcare.Api.Services.Patients;

public class PatientService : IPatientService
{
    private readonly HealthcareDbContext _context;

    public PatientService(HealthcareDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PatientDto>> GetAllAsync()
    {
        return await _context.Patients
            .AsNoTracking()
            .Select(p => new PatientDto
            {
                Id = p.Id,
                Mrn = p.Mrn,
                FirstName = p.FirstName,
                LastName = p.LastName,
                Sex = p.Sex,
                DateOfBirth = p.DateOfBirth,
                Phone = p.Phone,
                Email = p.Email,
                Address = p.Address,
                NationalId = p.NationalId,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            })
            .ToListAsync();
    }

    public async Task<PatientDto?> GetByIdAsync(int id)
    {
        return await _context.Patients
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new PatientDto
            {
                Id = p.Id,
                Mrn = p.Mrn,
                FirstName = p.FirstName,
                LastName = p.LastName,
                Sex = p.Sex,
                DateOfBirth = p.DateOfBirth,
                Phone = p.Phone,
                Email = p.Email,
                Address = p.Address,
                NationalId = p.NationalId,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<PatientDto> CreateAsync(CreatePatientDto dto)
    {
        var mrnExists = await _context.Patients
            .AnyAsync(p => p.Mrn == dto.Mrn);

        if (mrnExists)
        {
            throw new ArgumentException(
                $"A patient with MRN '{dto.Mrn}' already exists."
            );
        }

        var patient = new Patient
        {
            Mrn = dto.Mrn,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Sex = dto.Sex,
            DateOfBirth = dto.DateOfBirth,
            Phone = dto.Phone,
            Email = dto.Email,
            Address = dto.Address,
            NationalId = dto.NationalId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Patients.Add(patient);
        await _context.SaveChangesAsync();

        return ToDto(patient);
    }

    public async Task<PatientDto?> UpdateAsync(
        int id,
        UpdatePatientDto dto)
    {
        var patient = await _context.Patients.FindAsync(id);

        if (patient is null)
        {
            return null;
        }

        var mrnExists = await _context.Patients
            .AnyAsync(p => p.Mrn == dto.Mrn && p.Id != id);

        if (mrnExists)
        {
            throw new ArgumentException(
                $"A patient with MRN '{dto.Mrn}' already exists."
            );
        }

        patient.Mrn = dto.Mrn;
        patient.FirstName = dto.FirstName;
        patient.LastName = dto.LastName;
        patient.Sex = dto.Sex;
        patient.DateOfBirth = dto.DateOfBirth;
        patient.Phone = dto.Phone;
        patient.Email = dto.Email;
        patient.Address = dto.Address;
        patient.NationalId = dto.NationalId;
        patient.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return ToDto(patient);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var patient = await _context.Patients.FindAsync(id);

        if (patient is null)
        {
            return false;
        }

        _context.Patients.Remove(patient);
        await _context.SaveChangesAsync();

        return true;
    }

    private static PatientDto ToDto(Patient patient)
    {
        return new PatientDto
        {
            Id = patient.Id,
            Mrn = patient.Mrn,
            FirstName = patient.FirstName,
            LastName = patient.LastName,
            Sex = patient.Sex,
            DateOfBirth = patient.DateOfBirth,
            Phone = patient.Phone,
            Email = patient.Email,
            Address = patient.Address,
            NationalId = patient.NationalId,
            CreatedAt = patient.CreatedAt,
            UpdatedAt = patient.UpdatedAt
        };
    }
}