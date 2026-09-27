using Healthcare.Api.Security;
using Healthcare.Api.Data;
using Healthcare.Api.DTOs.Locations;
using Microsoft.EntityFrameworkCore;
using Healthcare.Api.Models;

namespace Healthcare.Api.Services.Locations;

public class LocationService : ILocationService
{
    private readonly HealthcareDbContext _context;
    private readonly ResourceAccess _access;

    public LocationService(HealthcareDbContext context, ResourceAccess access)
    {
        _context = context;
        _access = access;
    }

    public async Task<List<LocationDto>> GetAllAsync()
    {
        return await _access.List<Location>()
            .Select(l => new LocationDto
            {
                Id = l.Id,
                OrganizationId = l.OrganizationId,
                Name = l.Name,
                Address = l.Address,
                City = l.City,
                Phone = l.Phone,
                RoomNumber = l.RoomNumber,
                IsActive = l.IsActive,
                CreatedAt = l.CreatedAt,
                UpdatedAt = l.UpdatedAt
            })
            .ToListAsync();
    }


    public async Task<List<LocationDto>> GetByDoctorIdAsync(int doctorId)
    {
        return await _context.DoctorLocations
            .AsNoTracking()
            .Where(dl => dl.DoctorId == doctorId)
            .Select(dl => dl.Location)
            .Select(l => new LocationDto
            {
                Id = l.Id,
                OrganizationId = l.OrganizationId,
                Name = l.Name,
                Address = l.Address,
                City = l.City,
                Phone = l.Phone,
                RoomNumber = l.RoomNumber,
                IsActive = l.IsActive,
                CreatedAt = l.CreatedAt,
                UpdatedAt = l.UpdatedAt
            })
            .ToListAsync();
    }

    public async Task<List<LocationDto>> GetByOrganizationIdAsync(
    int organizationId)
    {
        return await _context.Locations
            .AsNoTracking()
            .Where(l => l.OrganizationId == organizationId)
            .Select(l => new LocationDto
            {
                Id = l.Id,
                OrganizationId = l.OrganizationId,
                Name = l.Name,
                Address = l.Address,
                City = l.City,
                Phone = l.Phone,
                RoomNumber = l.RoomNumber,
                IsActive = l.IsActive,
                CreatedAt = l.CreatedAt,
                UpdatedAt = l.UpdatedAt
            })
            .ToListAsync();
    }

    public async Task<(int? StaffDoctorId, int? StaffOrganizationId)>
    GetStaffAssignmentAsync(int staffUserId)
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
            return (null, null);
        }

        return (
            staff.StaffDoctorId,
            staff.StaffOrganizationId
        );
    }

    public async Task<LocationDto?> GetByIdAsync(int id)
    {
        return await _access.Query<Location>()
            .Where(l => l.Id == id)
            .Select(l => new LocationDto
            {
                Id = l.Id,
                OrganizationId = l.OrganizationId,
                Name = l.Name,
                Address = l.Address,
                City = l.City,
                Phone = l.Phone,
                RoomNumber = l.RoomNumber,
                IsActive = l.IsActive,
                CreatedAt = l.CreatedAt,
                UpdatedAt = l.UpdatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<LocationDto> CreateAsync(CreateLocationDto dto)
    {
        var organizationExists = await _context.Organizations
            .AnyAsync(o => o.Id == dto.OrganizationId);

        if (!organizationExists)
        {
            throw new ArgumentException(
                $"Organization with ID {dto.OrganizationId} does not exist."
            );
        }

        var now = DateTime.UtcNow;

        var location = new Location
        {
            OrganizationId = dto.OrganizationId,
            Name = dto.Name,
            Address = dto.Address,
            City = dto.City,
            Phone = dto.Phone,
            RoomNumber = dto.RoomNumber,
            IsActive = dto.IsActive,
            CreatedAt = now,
            UpdatedAt = now
        };

        _context.Locations.Add(location);

        await _context.SaveChangesAsync();

        return new LocationDto
        {
            Id = location.Id,
            OrganizationId = location.OrganizationId,
            Name = location.Name,
            Address = location.Address,
            City = location.City,
            Phone = location.Phone,
            RoomNumber = location.RoomNumber,
            IsActive = location.IsActive,
            CreatedAt = location.CreatedAt,
            UpdatedAt = location.UpdatedAt
        };
    }

    public async Task<LocationDto?> UpdateAsync(
    int id,
    UpdateLocationDto dto)
    {
        var location = await _context.Locations
            .FirstOrDefaultAsync(l => l.Id == id);

        if (location is null)
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

        location.OrganizationId = dto.OrganizationId;
        location.Name = dto.Name;
        location.Address = dto.Address;
        location.City = dto.City;
        location.Phone = dto.Phone;
        location.RoomNumber = dto.RoomNumber;
        location.IsActive = dto.IsActive;
        location.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new LocationDto
        {
            Id = location.Id,
            OrganizationId = location.OrganizationId,
            Name = location.Name,
            Address = location.Address,
            City = location.City,
            Phone = location.Phone,
            RoomNumber = location.RoomNumber,
            IsActive = location.IsActive,
            CreatedAt = location.CreatedAt,
            UpdatedAt = location.UpdatedAt
        };
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var location = await _context.Locations
            .FirstOrDefaultAsync(l => l.Id == id);

        if (location is null)
        {
            return false;
        }

        _context.Locations.Remove(location);

        await _context.SaveChangesAsync();

        return true;
    }
}