using Healthcare.Api.DTOs.SystemSettings;

namespace Healthcare.Api.Services.SystemSettings;

public interface ISystemSettingService
{
    Task<IEnumerable<SystemSettingDto>> GetAllAsync();

    Task<SystemSettingDto?> GetByIdAsync(int id);

    Task<SystemSettingDto> CreateAsync(
        CreateSystemSettingDto dto
    );

    Task<SystemSettingDto?> UpdateAsync(
        int id,
        UpdateSystemSettingDto dto
    );

    Task<bool> DeleteAsync(int id);
}