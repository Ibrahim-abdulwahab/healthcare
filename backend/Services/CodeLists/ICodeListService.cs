using Healthcare.Api.DTOs.CodeLists;

namespace Healthcare.Api.Services.CodeLists;

public interface ICodeListService
{
    Task<IEnumerable<CodeListDto>> GetAllAsync();

    Task<CodeListDto?> GetByIdAsync(int id);

    Task<CodeListDto> CreateAsync(
        CreateCodeListDto dto
    );

    Task<CodeListDto?> UpdateAsync(
        int id,
        UpdateCodeListDto dto
    );

    Task<bool> DeleteAsync(int id);
}