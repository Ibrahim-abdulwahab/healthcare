using Healthcare.Api.DTOs.Doctors;

namespace Healthcare.Api.Services.Doctors;

public interface IDoctorService
{
    Task<List<DoctorDto>> GetAllAsync();

    Task<DoctorDto?> GetByIdAsync(int id);

    Task<DoctorDto> CreateAsync(CreateDoctorDto dto);

    Task<DoctorDto?> UpdateAsync(
        int id,
        UpdateDoctorDto dto
    );

    Task<bool> DeleteAsync(int id);
}