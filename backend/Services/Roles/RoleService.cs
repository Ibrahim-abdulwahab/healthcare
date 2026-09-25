using Healthcare.Api.Data;
using Healthcare.Api.DTOs.Roles;
using Healthcare.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Healthcare.Api.Services.Roles;

public class RoleService : IRoleService
{
    private readonly HealthcareDbContext _context;

    public RoleService(HealthcareDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<RoleDto>> GetAllAsync()
    {
        return await _context.Roles
            .AsNoTracking()
            .Select(r => new RoleDto
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description
            })
            .ToListAsync();
    }

    public async Task<RoleDto?> GetByIdAsync(int id)
    {
        return await _context.Roles
            .AsNoTracking()
            .Where(r => r.Id == id)
            .Select(r => new RoleDto
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description
            })
            .FirstOrDefaultAsync();
    }

    public async Task<RoleDto> CreateAsync(CreateRoleDto dto)
    {
        var role = new Role
        {
            Name = dto.Name,
            Description = dto.Description
        };

        _context.Roles.Add(role);
        await _context.SaveChangesAsync();

        return ToDto(role);
    }

    public async Task<RoleDto?> UpdateAsync(
        int id,
        UpdateRoleDto dto)
    {
        var role = await _context.Roles.FindAsync(id);

        if (role is null)
            return null;

        role.Name = dto.Name;
        role.Description = dto.Description;

        await _context.SaveChangesAsync();

        return ToDto(role);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var role = await _context.Roles.FindAsync(id);

        if (role is null)
            return false;

        _context.Roles.Remove(role);
        await _context.SaveChangesAsync();

        return true;
    }

    private static RoleDto ToDto(Role role)
    {
        return new RoleDto
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description
        };
    }
}