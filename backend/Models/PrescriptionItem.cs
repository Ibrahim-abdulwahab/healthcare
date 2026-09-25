namespace Healthcare.Api.Models;

public class PrescriptionItem
{
    public int Id { get; set; }

    public int PrescriptionId { get; set; }

    public int ItemId { get; set; }

    public string? Dosage { get; set; }

    public string? Frequency { get; set; }

    public int? DurationDays { get; set; }

    public int? Quantity { get; set; }

    public string? Route { get; set; }

    public string? Instructions { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;


    // Relationships

    public Prescription Prescription { get; set; } = null!;

    public Item Item { get; set; } = null!;
}