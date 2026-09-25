using Healthcare.Api.DTOs.LabRequests;

namespace Healthcare.Api.Services.LabRequests;

public interface ILabRequestService
{
    Task<IEnumerable<LabRequestDto>> GetAllAsync();

    Task<LabRequestDto?> GetByIdAsync(int id);

    Task<LabRequestDto> CreateAsync(CreateLabRequestDto dto);

    Task<LabRequestDto?> UpdateAsync(
        int id,
        UpdateLabRequestDto dto
    );

    Task<bool> DeleteAsync(int id);
}