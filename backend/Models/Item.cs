namespace Healthcare.Api.Models;

public class Item
{
    public int Id { get; set; }

    public required string Code { get; set; }

    public required string Name { get; set; }

    public string? GenericName { get; set; }

    public string? BrandName { get; set; }

    // Allowed values: medication, supply
    public required string Type { get; set; }

    // Examples: tablet, vial, box, etc.
    public required string Unit { get; set; }

    public int CategoryId { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;


    // Relationships

    public ItemCategory Category { get; set; } = null!;

    public ICollection<PrescriptionItem> PrescriptionItems { get; set; } = [];
}