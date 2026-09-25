namespace Healthcare.Api.DTOs.Items;

public class ItemDto
{
    public int Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? GenericName { get; set; }

    public string? BrandName { get; set; }

    public string Type { get; set; } = string.Empty;

    public string Unit { get; set; } = string.Empty;

    public int CategoryId { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}