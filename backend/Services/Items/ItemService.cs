using Healthcare.Api.Data;
using Healthcare.Api.DTOs.Items;
using Healthcare.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Healthcare.Api.Services.Items;

public class ItemService : IItemService
{
    private readonly HealthcareDbContext _context;

    public ItemService(HealthcareDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ItemDto>> GetAllAsync()
    {
        return await _context.Items
            .AsNoTracking()
            .Select(i => new ItemDto
            {
                Id = i.Id,
                Code = i.Code,
                Name = i.Name,
                GenericName = i.GenericName,
                BrandName = i.BrandName,
                Type = i.Type,
                Unit = i.Unit,
                CategoryId = i.CategoryId,
                IsActive = i.IsActive,
                CreatedAt = i.CreatedAt,
                UpdatedAt = i.UpdatedAt
            })
            .ToListAsync();
    }

    public async Task<ItemDto?> GetByIdAsync(int id)
    {
        return await _context.Items
            .AsNoTracking()
            .Where(i => i.Id == id)
            .Select(i => new ItemDto
            {
                Id = i.Id,
                Code = i.Code,
                Name = i.Name,
                GenericName = i.GenericName,
                BrandName = i.BrandName,
                Type = i.Type,
                Unit = i.Unit,
                CategoryId = i.CategoryId,
                IsActive = i.IsActive,
                CreatedAt = i.CreatedAt,
                UpdatedAt = i.UpdatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<ItemDto> CreateAsync(CreateItemDto dto)
    {
        await ValidateCategoryAsync(dto.CategoryId);
        await ValidateCodeAsync(dto.Code);

        var now = DateTime.UtcNow;

        var item = new Item
        {
            Code = dto.Code,
            Name = dto.Name,
            GenericName = dto.GenericName,
            BrandName = dto.BrandName,
            Type = dto.Type,
            Unit = dto.Unit,
            CategoryId = dto.CategoryId,
            IsActive = dto.IsActive,
            CreatedAt = now,
            UpdatedAt = now
        };

        _context.Items.Add(item);
        await _context.SaveChangesAsync();

        return ToDto(item);
    }

    public async Task<ItemDto?> UpdateAsync(
        int id,
        UpdateItemDto dto)
    {
        var item = await _context.Items.FindAsync(id);

        if (item is null)
            return null;

        await ValidateCategoryAsync(dto.CategoryId);
        await ValidateCodeAsync(dto.Code, id);

        item.Code = dto.Code;
        item.Name = dto.Name;
        item.GenericName = dto.GenericName;
        item.BrandName = dto.BrandName;
        item.Type = dto.Type;
        item.Unit = dto.Unit;
        item.CategoryId = dto.CategoryId;
        item.IsActive = dto.IsActive;
        item.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return ToDto(item);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var item = await _context.Items.FindAsync(id);

        if (item is null)
            return false;

        _context.Items.Remove(item);
        await _context.SaveChangesAsync();

        return true;
    }

    private async Task ValidateCategoryAsync(int categoryId)
    {
        var categoryExists = await _context.ItemCategories
            .AsNoTracking()
            .AnyAsync(c => c.Id == categoryId);

        if (!categoryExists)
        {
            throw new ArgumentException(
                $"Item Category with ID {categoryId} does not exist."
            );
        }
    }

    private async Task ValidateCodeAsync(
        string code,
        int? excludeItemId = null)
    {
        var codeExists = await _context.Items
            .AsNoTracking()
            .AnyAsync(i =>
                i.Code == code &&
                (!excludeItemId.HasValue || i.Id != excludeItemId.Value)
            );

        if (codeExists)
        {
            throw new ArgumentException(
                $"An Item with code '{code}' already exists."
            );
        }
    }

    private static ItemDto ToDto(Item item)
    {
        return new ItemDto
        {
            Id = item.Id,
            Code = item.Code,
            Name = item.Name,
            GenericName = item.GenericName,
            BrandName = item.BrandName,
            Type = item.Type,
            Unit = item.Unit,
            CategoryId = item.CategoryId,
            IsActive = item.IsActive,
            CreatedAt = item.CreatedAt,
            UpdatedAt = item.UpdatedAt
        };
    }
}