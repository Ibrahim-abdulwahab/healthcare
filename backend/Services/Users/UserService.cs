using Healthcare.Api.Security;
using Healthcare.Api.Data;
using Healthcare.Api.DTOs.Users;
using Microsoft.EntityFrameworkCore;
using Healthcare.Api.Models;
using Microsoft.AspNetCore.Identity;

namespace Healthcare.Api.Services.Users;

public class UserService : IUserService
{
    private readonly HealthcareDbContext _context;
    private readonly ResourceAccess _access;

    public UserService(HealthcareDbContext context, ResourceAccess access)
    {
        _context = context;
        _access = access;
    }

    public async Task<List<UserDto>> GetAllAsync()
    {
        return await _access.List<User>()
            .Select(u => new UserDto
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                Role = u.Role,
                IsActive = u.IsActive,
                StaffDoctorId = u.StaffDoctorId,
                StaffOrganizationId = u.StaffOrganizationId,
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt
            })
            .ToListAsync();
    }

    public async Task<UserDto?> GetByIdAsync(int id)
    {
        return await _access.Query<User>()
            .Where(u => u.Id == id)
            .Select(u => new UserDto
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                Role = u.Role,
                IsActive = u.IsActive,
                StaffDoctorId = u.StaffDoctorId,
                StaffOrganizationId = u.StaffOrganizationId,
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<UserDto> CreateAsync(CreateUserDto dto)
    {
        var emailExists = await _context.Users
            .AnyAsync(u => u.Email == dto.Email);

        if (emailExists)
        {
            throw new ArgumentException(
                $"A user with email '{dto.Email}' already exists."
            );
        }

        await ValidateStaffAssignmentAsync(
        dto.Role,
        dto.StaffDoctorId,
        dto.StaffOrganizationId
    );

        var now = DateTime.UtcNow;

        var user = new User
        {
            Name = dto.Name,
            Email = dto.Email,
            PasswordHash = string.Empty,
            Role = dto.Role,
            IsActive = dto.IsActive,
            StaffDoctorId = dto.StaffDoctorId,
            StaffOrganizationId = dto.StaffOrganizationId,
            CreatedAt = now,
            UpdatedAt = now
        };

        var passwordHasher = new PasswordHasher<User>();

        user.PasswordHash = passwordHasher.HashPassword(
            user,
            dto.Password
        );

        _context.Users.Add(user);

        await _context.SaveChangesAsync();

        return new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role,
            IsActive = user.IsActive,
            StaffDoctorId = user.StaffDoctorId,
            StaffOrganizationId = user.StaffOrganizationId,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }

    public async Task<UserDto?> UpdateAsync(
    int id,
    UpdateUserDto dto)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user is null)
        {
            return null;
        }

        if (user.Role == "doctor" && dto.Role != "doctor")
        {
            var hasDoctor = await _context.Doctors
                .AnyAsync(d => d.Id == id);

            if (hasDoctor)
            {
                throw new ArgumentException(
                    $"User with ID {id} cannot change role because a Doctor record is linked to this user."
                );
            }
        }

        await ValidateStaffAssignmentAsync(
            dto.Role,
            dto.StaffDoctorId,
            dto.StaffOrganizationId
        );

        var emailExists = await _context.Users
            .AnyAsync(u =>
                u.Email == dto.Email &&
                u.Id != id
            );

        if (emailExists)
        {
            throw new ArgumentException(
                $"A user with email '{dto.Email}' already exists."
            );
        }

        user.Name = dto.Name;
        user.Email = dto.Email;
        user.Role = dto.Role;
        user.IsActive = dto.IsActive;
        user.StaffDoctorId = dto.StaffDoctorId;
        user.StaffOrganizationId = dto.StaffOrganizationId;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role,
            IsActive = user.IsActive,
            StaffDoctorId = user.StaffDoctorId,
            StaffOrganizationId = user.StaffOrganizationId,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }

    private async Task ValidateStaffAssignmentAsync(
    string role,
    int? staffDoctorId,
    int? staffOrganizationId)
    {
        if (role != "staff")
        {
            if (staffDoctorId.HasValue || staffOrganizationId.HasValue)
            {
                throw new ArgumentException(
                    "Only staff users can have a staff assignment."
                );
            }

            return;
        }

        var hasDoctor = staffDoctorId.HasValue;
        var hasOrganization = staffOrganizationId.HasValue;

        if (hasDoctor == hasOrganization)
        {
            throw new ArgumentException(
                "A staff user must be assigned to exactly one doctor or organization."
            );
        }

        if (hasDoctor)
        {
            var doctorExists = await _context.Doctors
                .AnyAsync(d => d.Id == staffDoctorId!.Value);

            if (!doctorExists)
            {
                throw new ArgumentException(
                    $"Doctor with ID {staffDoctorId} does not exist."
                );
            }
        }

        if (hasOrganization)
        {
            var organizationExists = await _context.Organizations
                .AnyAsync(o => o.Id == staffOrganizationId!.Value);

            if (!organizationExists)
            {
                throw new ArgumentException(
                    $"Organization with ID {staffOrganizationId} does not exist."
                );
            }
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user is null)
        {
            return false;
        }

        var hasDoctor = await _context.Doctors
            .AnyAsync(d => d.Id == id);

        if (hasDoctor)
        {
            throw new ArgumentException(
                $"User with ID {id} cannot be deleted because a Doctor record is linked to this user."
            );
        }

        _context.Users.Remove(user);

        await _context.SaveChangesAsync();

        return true;
    }
}