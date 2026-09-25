using Healthcare.Api.DTOs.Permissions;

namespace Healthcare.Api.Services.Permissions;

public interface IPermissionService
{
    Task<IEnumerable<PermissionDto>> GetAllAsync();

    Task<PermissionDto?> GetByIdAsync(int id);

    Task<PermissionDto> CreateAsync(CreatePermissionDto dto);

    Task<PermissionDto?> UpdateAsync(
        int id,
        UpdatePermissionDto dto
    );

    Task<bool> DeleteAsync(int id);
}