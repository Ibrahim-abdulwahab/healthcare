using Healthcare.Api.Infrastructure;
using Healthcare.Api.Security;
using Healthcare.Api.Data;
using Healthcare.Api.DTOs.DoctorLocations;
using Microsoft.EntityFrameworkCore;
using Healthcare.Api.Models;

namespace Healthcare.Api.Services.DoctorLocations;

public class DoctorLocationService : IDoctorLocationService
{
    private readonly HealthcareDbContext _context;
    private readonly ResourceAccess _access;

    public DoctorLocationService(HealthcareDbContext context, ResourceAccess access)
    {
        _context = context;
        _access = access;
    }

    public async Task<List<DoctorLocationDto>> GetAllAsync()
    {
        return await _access.List<DoctorLocation>()
            .Select(dl => new DoctorLocationDto
            {
                Id = dl.Id,
                DoctorName = dl.Doctor.FirstName + " " + dl.Doctor.LastName,
                LocationName = dl.Location.Name,

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
        return await _access.Query<DoctorLocation>()
            .Where(dl => dl.Id == id)
            .Select(dl => new DoctorLocationDto
            {
                Id = dl.Id,
                DoctorName = dl.Doctor.FirstName + " " + dl.Doctor.LastName,
                LocationName = dl.Location.Name,

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

        if (dto.IsActive && await _context.DoctorLocations.AnyAsync(dl => dl.DoctorId == dto.DoctorId && dl.LocationId == dto.LocationId && dl.IsActive))
            throw new ApiException(409, "An active mapping already exists for this doctor and location.");
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

        if (dto.IsActive && await _context.DoctorLocations.AnyAsync(dl => dl.Id != id && dl.DoctorId == dto.DoctorId && dl.LocationId == dto.LocationId && dl.IsActive))
            throw new ApiException(409, "An active mapping already exists for this doctor and location.");
        if ((doctorLocation.DoctorId != dto.DoctorId || doctorLocation.LocationId != dto.LocationId) &&
            (await _context.Schedules.AnyAsync(s => s.DoctorLocationId == id) || await _context.ScheduleExceptions.AnyAsync(s => s.DoctorLocationId == id)))
            throw new ApiException(409, "A mapping with schedules or exceptions cannot be reassigned.");
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