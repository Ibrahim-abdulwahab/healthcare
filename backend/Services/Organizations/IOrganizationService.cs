using Healthcare.Api.DTOs.Organizations;

namespace Healthcare.Api.Services.Organizations;

public interface IOrganizationService
{
    Task<List<OrganizationDto>> GetAllAsync();

    Task<OrganizationDto?> GetByIdAsync(int id);

    Task<OrganizationDto> CreateAsync(CreateOrganizationDto dto);

    Task<OrganizationDto?> UpdateAsync(int id, UpdateOrganizationDto dto);

    Task<bool> DeleteAsync(int id);
}