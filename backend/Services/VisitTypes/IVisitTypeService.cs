using Healthcare.Api.DTOs.VisitTypes;

namespace Healthcare.Api.Services.VisitTypes;

public interface IVisitTypeService
{
    Task<IEnumerable<VisitTypeDto>> GetAllAsync();

    Task<VisitTypeDto?> GetByIdAsync(int id);

    Task<VisitTypeDto> CreateAsync(CreateVisitTypeDto dto);

    Task<VisitTypeDto?> UpdateAsync(int id, UpdateVisitTypeDto dto);

    Task<bool> DeleteAsync(int id);
}