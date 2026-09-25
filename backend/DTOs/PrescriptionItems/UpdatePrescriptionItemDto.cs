namespace Healthcare.Api.DTOs.PrescriptionItems;

public class UpdatePrescriptionItemDto
{
    public int PrescriptionId { get; set; }

    public int ItemId { get; set; }

    public string? Dosage { get; set; }

    public string? Frequency { get; set; }

    public int? DurationDays { get; set; }

    public int? Quantity { get; set; }

    public string? Route { get; set; }

    public string? Instructions { get; set; }

    public string? Notes { get; set; }
}