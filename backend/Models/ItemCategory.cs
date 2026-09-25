namespace Healthcare.Api.Models;

public class ItemCategory
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public string? Description { get; set; }

    public int? ParentId { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;


    // Relationships

    public ItemCategory? Parent { get; set; }

    public ICollection<ItemCategory> Children { get; set; } = [];

    public ICollection<Item> Items { get; set; } = [];
}