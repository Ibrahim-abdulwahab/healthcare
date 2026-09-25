using Healthcare.Api.DTOs.FollowUps;

namespace Healthcare.Api.Services.FollowUps;

public interface IFollowUpService
{
    Task<IEnumerable<FollowUpDto>> GetAllAsync();

    Task<FollowUpDto?> GetByIdAsync(int id);

    Task<FollowUpDto> CreateAsync(CreateFollowUpDto dto);

    Task<FollowUpDto?> UpdateAsync(
        int id,
        UpdateFollowUpDto dto
    );

    Task<bool> DeleteAsync(int id);
}