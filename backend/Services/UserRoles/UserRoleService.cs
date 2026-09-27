using Healthcare.Api.Security;
using Healthcare.Api.Data;
using Healthcare.Api.DTOs.UserRoles;
using Healthcare.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Healthcare.Api.Services.UserRoles;

public class UserRoleService : IUserRoleService
{
    private readonly HealthcareDbContext _context;
    private readonly ResourceAccess _access;

    public UserRoleService(HealthcareDbContext context, ResourceAccess access)
    {
        _context = context;
        _access = access;
    }

    public async Task<IEnumerable<UserRoleDto>> GetAllAsync()
    {
        return await _access.List<UserRole>()
            .Select(ur => new UserRoleDto
            {
                Id = ur.Id,
                UserId = ur.UserId,
                RoleId = ur.RoleId
            })
            .ToListAsync();
    }

    public async Task<UserRoleDto?> GetByIdAsync(int id)
    {
        return await _access.Query<UserRole>()
            .Where(ur => ur.Id == id)
            .Select(ur => new UserRoleDto
            {
                Id = ur.Id,
                UserId = ur.UserId,
                RoleId = ur.RoleId
            })
            .FirstOrDefaultAsync();
    }

    public async Task<UserRoleDto> CreateAsync(
        CreateUserRoleDto dto)
    {
        await ValidateRelationshipsAsync(
            dto.UserId,
            dto.RoleId
        );

        var userRole = new UserRole
        {
            UserId = dto.UserId,
            RoleId = dto.RoleId
        };

        _context.UserRoles.Add(userRole);
        await _context.SaveChangesAsync();

        return ToDto(userRole);
    }

    public async Task<UserRoleDto?> UpdateAsync(
        int id,
        UpdateUserRoleDto dto)
    {
        var userRole = await _context.UserRoles.FindAsync(id);

        if (userRole is null)
            return null;

        await ValidateRelationshipsAsync(
            dto.UserId,
            dto.RoleId
        );

        userRole.UserId = dto.UserId;
        userRole.RoleId = dto.RoleId;

        await _context.SaveChangesAsync();

        return ToDto(userRole);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var userRole = await _context.UserRoles.FindAsync(id);

        if (userRole is null)
            return false;

        _context.UserRoles.Remove(userRole);
        await _context.SaveChangesAsync();

        return true;
    }

    private async Task ValidateRelationshipsAsync(
        int userId,
        int roleId)
    {
        var userExists = await _context.Users
            .AsNoTracking()
            .AnyAsync(u => u.Id == userId);

        if (!userExists)
        {
            throw new ArgumentException(
                $"User with ID {userId} does not exist."
            );
        }

        var roleExists = await _context.Roles
            .AsNoTracking()
            .AnyAsync(r => r.Id == roleId);

        if (!roleExists)
        {
            throw new ArgumentException(
                $"Role with ID {roleId} does not exist."
            );
        }
    }

    private static UserRoleDto ToDto(UserRole userRole)
    {
        return new UserRoleDto
        {
            Id = userRole.Id,
            UserId = userRole.UserId,
            RoleId = userRole.RoleId
        };
    }
}