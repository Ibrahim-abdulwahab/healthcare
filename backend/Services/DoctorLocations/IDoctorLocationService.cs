using Healthcare.Api.DTOs.DoctorLocations;

namespace Healthcare.Api.Services.DoctorLocations;

public interface IDoctorLocationService
{
    Task<List<DoctorLocationDto>> GetAllAsync();

    Task<DoctorLocationDto?> GetByIdAsync(int id);

    Task<DoctorLocationDto> CreateAsync(
        CreateDoctorLocationDto dto
    );

    Task<DoctorLocationDto?> UpdateAsync(
        int id,
        UpdateDoctorLocationDto dto
    );

    Task<bool> DeleteAsync(int id);
}