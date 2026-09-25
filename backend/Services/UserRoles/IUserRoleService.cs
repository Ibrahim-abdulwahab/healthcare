using Healthcare.Api.DTOs.UserRoles;

namespace Healthcare.Api.Services.UserRoles;

public interface IUserRoleService
{
    Task<IEnumerable<UserRoleDto>> GetAllAsync();

    Task<UserRoleDto?> GetByIdAsync(int id);

    Task<UserRoleDto> CreateAsync(CreateUserRoleDto dto);

    Task<UserRoleDto?> UpdateAsync(
        int id,
        UpdateUserRoleDto dto
    );

    Task<bool> DeleteAsync(int id);
}