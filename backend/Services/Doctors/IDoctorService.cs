using Healthcare.Api.DTOs.Doctors;

namespace Healthcare.Api.Services.Doctors;

public interface IDoctorService
{
    Task<List<DoctorDto>> GetAllAsync();

    Task<DoctorDto?> GetByIdAsync(int id);

    Task<List<DoctorDto>> GetByOrganizationIdAsync(int organizationId);

    Task<DoctorDto?> GetByStaffUserIdAsync(int staffUserId);

    Task<int?> GetStaffOrganizationIdAsync(int staffUserId);

    Task<bool> StaffCanAccessDoctorAsync(
    int staffUserId,
    int doctorId);

    Task<DoctorDto> CreateAsync(CreateDoctorDto dto);

    Task<DoctorDto?> UpdateAsync(
        int id,
        UpdateDoctorDto dto
    );

    Task<bool> DeleteAsync(int id);
}