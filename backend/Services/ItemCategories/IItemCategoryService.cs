using Healthcare.Api.DTOs.ItemCategories;

namespace Healthcare.Api.Services.ItemCategories;

public interface IItemCategoryService
{
    Task<IEnumerable<ItemCategoryDto>> GetAllAsync();

    Task<ItemCategoryDto?> GetByIdAsync(int id);

    Task<ItemCategoryDto> CreateAsync(
        CreateItemCategoryDto dto
    );

    Task<ItemCategoryDto?> UpdateAsync(
        int id,
        UpdateItemCategoryDto dto
    );

    Task<bool> DeleteAsync(int id);
}