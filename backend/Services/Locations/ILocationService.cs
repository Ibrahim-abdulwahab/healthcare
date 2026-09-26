using Healthcare.Api.DTOs.Locations;

namespace Healthcare.Api.Services.Locations;

public interface ILocationService
{
    Task<List<LocationDto>> GetAllAsync();

    Task<List<LocationDto>> GetByDoctorIdAsync(int doctorId);

    Task<List<LocationDto>> GetByOrganizationIdAsync(int organizationId);

    Task<(int? StaffDoctorId, int? StaffOrganizationId)>
    GetStaffAssignmentAsync(int staffUserId);

    Task<LocationDto?> GetByIdAsync(int id);

    Task<LocationDto> CreateAsync(CreateLocationDto dto);

    Task<LocationDto?> UpdateAsync(
        int id,
        UpdateLocationDto dto
    );

    Task<bool> DeleteAsync(int id);
}