using Healthcare.Api.DTOs.HealthcareFiles;

namespace Healthcare.Api.Services.HealthcareFiles;

public interface IHealthcareFileService
{
    Task<IEnumerable<HealthcareFileDto>> GetAllAsync();

    Task<HealthcareFileDto?> GetByIdAsync(int id);

    Task<HealthcareFileDto> CreateAsync(
        CreateHealthcareFileDto dto
    );

    Task<HealthcareFileDto?> UpdateAsync(
        int id,
        UpdateHealthcareFileDto dto
    );

    Task<bool> DeleteAsync(int id);
}