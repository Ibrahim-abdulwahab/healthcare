using System.ComponentModel.DataAnnotations;

namespace Healthcare.Api.DTOs.Items;

public class CreateItemDto
{
    [Required]
    public string Code { get; set; } = string.Empty;

    [Required]
    public string Name { get; set; } = string.Empty;

    public string? GenericName { get; set; }

    public string? BrandName { get; set; }

    [Required]
    [RegularExpression(
        "^(medication|supply)$",
        ErrorMessage = "Invalid item type."
    )]
    public string Type { get; set; } = string.Empty;

    [Required]
    public string Unit { get; set; } = string.Empty;

    public int CategoryId { get; set; }

    public bool IsActive { get; set; } = true;
}