using Healthcare.Api.DTOs.Visits;

namespace Healthcare.Api.Services.Visits;

public interface IVisitService
{
    Task<IEnumerable<VisitDto>> GetAllAsync();

    Task<VisitDto?> GetByIdAsync(int id);

    Task<VisitDto> CreateAsync(CreateVisitDto dto);

    Task<VisitDto?> UpdateAsync(int id, UpdateVisitDto dto);

    Task<bool> DeleteAsync(int id);
}