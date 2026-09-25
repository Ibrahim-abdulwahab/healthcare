using Healthcare.Api.Data;
using Healthcare.Api.DTOs.Doctors;
using Microsoft.EntityFrameworkCore;
using Healthcare.Api.Models;

namespace Healthcare.Api.Services.Doctors;

public class DoctorService : IDoctorService
{
    private readonly HealthcareDbContext _context;

    public DoctorService(HealthcareDbContext context)
    {
        _context = context;
    }

    public async Task<List<DoctorDto>> GetAllAsync()
    {
        return await _context.Doctors
            .AsNoTracking()
            .Select(d => new DoctorDto
            {
                Id = d.Id,
                OrganizationId = d.OrganizationId,
                FirstName = d.FirstName,
                LastName = d.LastName,
                Specialty = d.Specialty,
                Phone = d.Phone,
                Email = d.Email,
                LicenseNumber = d.LicenseNumber,
                IsActive = d.IsActive,
                CreatedAt = d.CreatedAt,
                UpdatedAt = d.UpdatedAt
            })
            .ToListAsync();
    }

    public async Task<DoctorDto?> GetByIdAsync(int id)
    {
        return await _context.Doctors
            .AsNoTracking()
            .Where(d => d.Id == id)
            .Select(d => new DoctorDto
            {
                Id = d.Id,
                OrganizationId = d.OrganizationId,
                FirstName = d.FirstName,
                LastName = d.LastName,
                Specialty = d.Specialty,
                Phone = d.Phone,
                Email = d.Email,
                LicenseNumber = d.LicenseNumber,
                IsActive = d.IsActive,
                CreatedAt = d.CreatedAt,
                UpdatedAt = d.UpdatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<DoctorDto> CreateAsync(CreateDoctorDto dto)
    {
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == dto.UserId);

        if (user is null)
        {
            throw new ArgumentException(
                $"User with ID {dto.UserId} does not exist."
            );
        }

        if (user.Role != "doctor")
        {
            throw new ArgumentException(
                $"User with ID {dto.UserId} must have the 'doctor' role."
            );
        }

        var doctorExists = await _context.Doctors
            .AnyAsync(d => d.Id == dto.UserId);

        if (doctorExists)
        {
            throw new ArgumentException(
                $"User with ID {dto.UserId} already has a Doctor record."
            );
        }

        var organizationExists = await _context.Organizations
            .AnyAsync(o => o.Id == dto.OrganizationId);

        if (!organizationExists)
        {
            throw new ArgumentException(
                $"Organization with ID {dto.OrganizationId} does not exist."
            );
        }

        var now = DateTime.UtcNow;

        var doctor = new Doctor
        {
            Id = dto.UserId,
            OrganizationId = dto.OrganizationId,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Specialty = dto.Specialty,
            Phone = dto.Phone,
            Email = dto.Email,
            LicenseNumber = dto.LicenseNumber,
            IsActive = dto.IsActive,
            CreatedAt = now,
            UpdatedAt = now
        };

        _context.Doctors.Add(doctor);

        await _context.SaveChangesAsync();

        return new DoctorDto
        {
            Id = doctor.Id,
            OrganizationId = doctor.OrganizationId,
            FirstName = doctor.FirstName,
            LastName = doctor.LastName,
            Specialty = doctor.Specialty,
            Phone = doctor.Phone,
            Email = doctor.Email,
            LicenseNumber = doctor.LicenseNumber,
            IsActive = doctor.IsActive,
            CreatedAt = doctor.CreatedAt,
            UpdatedAt = doctor.UpdatedAt
        };
    }

    public async Task<DoctorDto?> UpdateAsync(
    int id,
    UpdateDoctorDto dto)
    {
        var doctor = await _context.Doctors
            .FirstOrDefaultAsync(d => d.Id == id);

        if (doctor is null)
        {
            return null;
        }

        var organizationExists = await _context.Organizations
            .AnyAsync(o => o.Id == dto.OrganizationId);

        if (!organizationExists)
        {
            throw new ArgumentException(
                $"Organization with ID {dto.OrganizationId} does not exist."
            );
        }

        doctor.OrganizationId = dto.OrganizationId;
        doctor.FirstName = dto.FirstName;
        doctor.LastName = dto.LastName;
        doctor.Specialty = dto.Specialty;
        doctor.Phone = dto.Phone;
        doctor.Email = dto.Email;
        doctor.LicenseNumber = dto.LicenseNumber;
        doctor.IsActive = dto.IsActive;
        doctor.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new DoctorDto
        {
            Id = doctor.Id,
            OrganizationId = doctor.OrganizationId,
            FirstName = doctor.FirstName,
            LastName = doctor.LastName,
            Specialty = doctor.Specialty,
            Phone = doctor.Phone,
            Email = doctor.Email,
            LicenseNumber = doctor.LicenseNumber,
            IsActive = doctor.IsActive,
            CreatedAt = doctor.CreatedAt,
            UpdatedAt = doctor.UpdatedAt
        };
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var doctor = await _context.Doctors
            .FirstOrDefaultAsync(d => d.Id == id);

        if (doctor is null)
        {
            return false;
        }

        _context.Doctors.Remove(doctor);

        await _context.SaveChangesAsync();

        return true;
    }
}