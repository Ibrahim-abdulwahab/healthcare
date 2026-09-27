using Healthcare.Api.Security;
using Healthcare.Api.Data;
using Healthcare.Api.DTOs.ItemCategories;
using Healthcare.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Healthcare.Api.Services.ItemCategories;

public class ItemCategoryService : IItemCategoryService
{
    private readonly HealthcareDbContext _context;
    private readonly ResourceAccess _access;

    public ItemCategoryService(HealthcareDbContext context, ResourceAccess access)
    {
        _context = context;
        _access = access;
    }

    public async Task<IEnumerable<ItemCategoryDto>> GetAllAsync()
    {
        return await _access.List<ItemCategory>()
            .Select(c => new ItemCategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                ParentId = c.ParentId,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            })
            .ToListAsync();
    }

    public async Task<ItemCategoryDto?> GetByIdAsync(int id)
    {
        return await _access.Query<ItemCategory>()
            .Where(c => c.Id == id)
            .Select(c => new ItemCategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                ParentId = c.ParentId,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<ItemCategoryDto> CreateAsync(
        CreateItemCategoryDto dto)
    {
        await ValidateParentAsync(dto.ParentId);

        var now = DateTime.UtcNow;

        var category = new ItemCategory
        {
            Name = dto.Name,
            Description = dto.Description,
            ParentId = dto.ParentId,
            IsActive = dto.IsActive,
            CreatedAt = now,
            UpdatedAt = now
        };

        _context.ItemCategories.Add(category);
        await _context.SaveChangesAsync();

        return ToDto(category);
    }

    public async Task<ItemCategoryDto?> UpdateAsync(
        int id,
        UpdateItemCategoryDto dto)
    {
        var category = await _context.ItemCategories.FindAsync(id);

        if (category is null)
            return null;

        if (dto.ParentId == id)
        {
            throw new ArgumentException(
                "An Item Category cannot be its own parent."
            );
        }

        var ancestor = dto.ParentId;
        var visited = new HashSet<int> { id };
        while (ancestor.HasValue)
        {
            if (!visited.Add(ancestor.Value)) throw new ArgumentException("Item categories cannot contain an ancestor cycle.");
            ancestor = await _context.ItemCategories.Where(c => c.Id == ancestor.Value).Select(c => c.ParentId).SingleOrDefaultAsync();
        }
        await ValidateParentAsync(dto.ParentId);

        category.Name = dto.Name;
        category.Description = dto.Description;
        category.ParentId = dto.ParentId;
        category.IsActive = dto.IsActive;
        category.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return ToDto(category);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var category = await _context.ItemCategories.FindAsync(id);

        if (category is null)
            return false;

        _context.ItemCategories.Remove(category);
        await _context.SaveChangesAsync();

        return true;
    }

    private async Task ValidateParentAsync(int? parentId)
    {
        if (parentId is null)
            return;

        var parentExists = await _context.ItemCategories
            .AsNoTracking()
            .AnyAsync(c => c.Id == parentId.Value);

        if (!parentExists)
        {
            throw new ArgumentException(
                $"Item Category with ID {parentId.Value} does not exist."
            );
        }
    }

    private static ItemCategoryDto ToDto(ItemCategory category)
    {
        return new ItemCategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            ParentId = category.ParentId,
            IsActive = category.IsActive,
            CreatedAt = category.CreatedAt,
            UpdatedAt = category.UpdatedAt
        };
    }
}