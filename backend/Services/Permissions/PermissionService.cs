using Healthcare.Api.Data;
using Healthcare.Api.DTOs.Permissions;
using Healthcare.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Healthcare.Api.Services.Permissions;

public class PermissionService : IPermissionService
{
    private readonly HealthcareDbContext _context;

    public PermissionService(HealthcareDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PermissionDto>> GetAllAsync()
    {
        return await _context.Permissions
            .AsNoTracking()
            .Select(p => new PermissionDto
            {
                Id = p.Id,
                Name = p.Name,
                Code = p.Code,
                Description = p.Description
            })
            .ToListAsync();
    }

    public async Task<PermissionDto?> GetByIdAsync(int id)
    {
        return await _context.Permissions
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new PermissionDto
            {
                Id = p.Id,
                Name = p.Name,
                Code = p.Code,
                Description = p.Description
            })
            .FirstOrDefaultAsync();
    }

    public async Task<PermissionDto> CreateAsync(
        CreatePermissionDto dto)
    {
        var permission = new Permission
        {
            Name = dto.Name,
            Code = dto.Code,
            Description = dto.Description
        };

        _context.Permissions.Add(permission);
        await _context.SaveChangesAsync();

        return ToDto(permission);
    }

    public async Task<PermissionDto?> UpdateAsync(
        int id,
        UpdatePermissionDto dto)
    {
        var permission = await _context.Permissions.FindAsync(id);

        if (permission is null)
            return null;

        permission.Name = dto.Name;
        permission.Code = dto.Code;
        permission.Description = dto.Description;

        await _context.SaveChangesAsync();

        return ToDto(permission);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var permission = await _context.Permissions.FindAsync(id);

        if (permission is null)
            return false;

        _context.Permissions.Remove(permission);
        await _context.SaveChangesAsync();

        return true;
    }

    private static PermissionDto ToDto(Permission permission)
    {
        return new PermissionDto
        {
            Id = permission.Id,
            Name = permission.Name,
            Code = permission.Code,
            Description = permission.Description
        };
    }
}