using Healthcare.Api.Security;
using Healthcare.Api.Data;
using Healthcare.Api.DTOs.VisitTypes;
using Healthcare.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Healthcare.Api.Services.VisitTypes;

public class VisitTypeService : IVisitTypeService
{
    private readonly HealthcareDbContext _context;
    private readonly ResourceAccess _access;

    public VisitTypeService(HealthcareDbContext context, ResourceAccess access)
    {
        _context = context;
        _access = access;
    }

    public async Task<IEnumerable<VisitTypeDto>> GetAllAsync()
    {
        return await _access.List<VisitType>()
            .Select(v => new VisitTypeDto
            {
                Id = v.Id,
                Name = v.Name,
                Description = v.Description,
                DefaultDuration = v.DefaultDuration,
                IsActive = v.IsActive,
                CreatedAt = v.CreatedAt,
                UpdatedAt = v.UpdatedAt
            })
            .ToListAsync();
    }

    public async Task<VisitTypeDto?> GetByIdAsync(int id)
    {
        return await _access.Query<VisitType>()
            .Where(v => v.Id == id)
            .Select(v => new VisitTypeDto
            {
                Id = v.Id,
                Name = v.Name,
                Description = v.Description,
                DefaultDuration = v.DefaultDuration,
                IsActive = v.IsActive,
                CreatedAt = v.CreatedAt,
                UpdatedAt = v.UpdatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<VisitTypeDto> CreateAsync(CreateVisitTypeDto dto)
    {
        if (dto.DefaultDuration <= 0 || dto.DefaultDuration >= 1440)
        {
            throw new ArgumentException(
                "Default duration must be between 1 and 1439 minutes."
            );
        }

        var visitType = new VisitType
        {
            Name = dto.Name,
            Description = dto.Description,
            DefaultDuration = dto.DefaultDuration,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.VisitTypes.Add(visitType);
        await _context.SaveChangesAsync();

        return ToDto(visitType);
    }

    public async Task<VisitTypeDto?> UpdateAsync(
        int id,
        UpdateVisitTypeDto dto)
    {
        var visitType = await _context.VisitTypes.FindAsync(id);

        if (visitType is null)
        {
            return null;
        }

        if (dto.DefaultDuration <= 0 || dto.DefaultDuration >= 1440)
        {
            throw new ArgumentException(
                "Default duration must be between 1 and 1439 minutes."
            );
        }

        visitType.Name = dto.Name;
        visitType.Description = dto.Description;
        visitType.DefaultDuration = dto.DefaultDuration;
        visitType.IsActive = dto.IsActive;
        visitType.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return ToDto(visitType);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var visitType = await _context.VisitTypes.FindAsync(id);

        if (visitType is null)
        {
            return false;
        }

        _context.VisitTypes.Remove(visitType);
        await _context.SaveChangesAsync();

        return true;
    }

    private static VisitTypeDto ToDto(VisitType visitType)
    {
        return new VisitTypeDto
        {
            Id = visitType.Id,
            Name = visitType.Name,
            Description = visitType.Description,
            DefaultDuration = visitType.DefaultDuration,
            IsActive = visitType.IsActive,
            CreatedAt = visitType.CreatedAt,
            UpdatedAt = visitType.UpdatedAt
        };
    }
}