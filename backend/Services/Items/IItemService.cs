using Healthcare.Api.DTOs.Items;

namespace Healthcare.Api.Services.Items;

public interface IItemService
{
    Task<IEnumerable<ItemDto>> GetAllAsync();

    Task<ItemDto?> GetByIdAsync(int id);

    Task<ItemDto> CreateAsync(
        CreateItemDto dto
    );

    Task<ItemDto?> UpdateAsync(
        int id,
        UpdateItemDto dto
    );

    Task<bool> DeleteAsync(int id);
}