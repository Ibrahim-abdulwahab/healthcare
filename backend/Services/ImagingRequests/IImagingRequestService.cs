using Healthcare.Api.DTOs.ImagingRequests;

namespace Healthcare.Api.Services.ImagingRequests;

public interface IImagingRequestService
{
    Task<IEnumerable<ImagingRequestDto>> GetAllAsync();

    Task<ImagingRequestDto?> GetByIdAsync(int id);

    Task<ImagingRequestDto> CreateAsync(CreateImagingRequestDto dto);

    Task<ImagingRequestDto?> UpdateAsync(
        int id,
        UpdateImagingRequestDto dto
    );

    Task<bool> DeleteAsync(int id);
}