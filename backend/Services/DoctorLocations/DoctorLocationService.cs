using Healthcare.Api.Data;
using Healthcare.Api.DTOs.DoctorLocations;
using Microsoft.EntityFrameworkCore;
using Healthcare.Api.Models;

namespace Healthcare.Api.Services.DoctorLocations;

public class DoctorLocationService : IDoctorLocationService
{
    private readonly HealthcareDbContext _context;

    public DoctorLocationService(HealthcareDbContext context)
    {
        _context = context;
    }

    public async Task<List<DoctorLocationDto>> GetAllAsync()
    {
        return await _context.DoctorLocations
            .AsNoTracking()
            .Select(dl => new DoctorLocationDto
            {
                Id = dl.Id,
                DoctorId = dl.DoctorId,
                LocationId = dl.LocationId,
                IsActive = dl.IsActive,
                CreatedAt = dl.CreatedAt,
                UpdatedAt = dl.UpdatedAt
            })
            .ToListAsync();
    }

    public async Task<DoctorLocationDto?> GetByIdAsync(int id)
    {
        return await _context.DoctorLocations
            .AsNoTracking()
            .Where(dl => dl.Id == id)
            .Select(dl => new DoctorLocationDto
            {
                Id = dl.Id,
                DoctorId = dl.DoctorId,
                LocationId = dl.LocationId,
                IsActive = dl.IsActive,
                CreatedAt = dl.CreatedAt,
                UpdatedAt = dl.UpdatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<DoctorLocationDto> CreateAsync(
    CreateDoctorLocationDto dto)
    {
        var doctorExists = await _context.Doctors
            .AnyAsync(d => d.Id == dto.DoctorId);

        if (!doctorExists)
        {
            throw new ArgumentException(
                $"Doctor with ID {dto.DoctorId} does not exist."
            );
        }

        var locationExists = await _context.Locations
            .AnyAsync(l => l.Id == dto.LocationId);

        if (!locationExists)
        {
            throw new ArgumentException(
                $"Location with ID {dto.LocationId} does not exist."
            );
        }

        var now = DateTime.UtcNow;

        var doctorLocation = new DoctorLocation
        {
            DoctorId = dto.DoctorId,
            LocationId = dto.LocationId,
            IsActive = dto.IsActive,
            CreatedAt = now,
            UpdatedAt = now
        };

        _context.DoctorLocations.Add(doctorLocation);

        await _context.SaveChangesAsync();

        return new DoctorLocationDto
        {
            Id = doctorLocation.Id,
            DoctorId = doctorLocation.DoctorId,
            LocationId = doctorLocation.LocationId,
            IsActive = doctorLocation.IsActive,
            CreatedAt = doctorLocation.CreatedAt,
            UpdatedAt = doctorLocation.UpdatedAt
        };
    }

    public async Task<DoctorLocationDto?> UpdateAsync(
    int id,
    UpdateDoctorLocationDto dto)
    {
        var doctorLocation = await _context.DoctorLocations
            .FirstOrDefaultAsync(dl => dl.Id == id);

        if (doctorLocation is null)
        {
            return null;
        }

        var doctorExists = await _context.Doctors
            .AnyAsync(d => d.Id == dto.DoctorId);

        if (!doctorExists)
        {
            throw new ArgumentException(
                $"Doctor with ID {dto.DoctorId} does not exist."
            );
        }

        var locationExists = await _context.Locations
            .AnyAsync(l => l.Id == dto.LocationId);

        if (!locationExists)
        {
            throw new ArgumentException(
                $"Location with ID {dto.LocationId} does not exist."
            );
        }

        doctorLocation.DoctorId = dto.DoctorId;
        doctorLocation.LocationId = dto.LocationId;
        doctorLocation.IsActive = dto.IsActive;
        doctorLocation.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new DoctorLocationDto
        {
            Id = doctorLocation.Id,
            DoctorId = doctorLocation.DoctorId,
            LocationId = doctorLocation.LocationId,
            IsActive = doctorLocation.IsActive,
            CreatedAt = doctorLocation.CreatedAt,
            UpdatedAt = doctorLocation.UpdatedAt
        };
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var doctorLocation = await _context.DoctorLocations
            .FirstOrDefaultAsync(dl => dl.Id == id);

        if (doctorLocation is null)
        {
            return false;
        }

        _context.DoctorLocations.Remove(doctorLocation);

        await _context.SaveChangesAsync();

        return true;
    }
}