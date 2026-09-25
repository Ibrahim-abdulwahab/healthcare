using Healthcare.Api.DTOs.Locations;

namespace Healthcare.Api.Services.Locations;

public interface ILocationService
{
    Task<List<LocationDto>> GetAllAsync();

    Task<LocationDto?> GetByIdAsync(int id);

    Task<LocationDto> CreateAsync(CreateLocationDto dto);

    Task<LocationDto?> UpdateAsync(
        int id,
        UpdateLocationDto dto
    );

    Task<bool> DeleteAsync(int id);
}