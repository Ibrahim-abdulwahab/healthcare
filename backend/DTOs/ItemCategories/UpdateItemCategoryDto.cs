using System.ComponentModel.DataAnnotations;
namespace Healthcare.Api.DTOs.ItemCategories;

public class UpdateItemCategoryDto
{
    [Required]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int? ParentId { get; set; }

    public bool IsActive { get; set; }
}