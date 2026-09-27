using System.ComponentModel.DataAnnotations;
namespace Healthcare.Api.DTOs.PrescriptionItems;

public class CreatePrescriptionItemDto
{
    public int PrescriptionId { get; set; }

    public int ItemId { get; set; }

    public string? Dosage { get; set; }

    public string? Frequency { get; set; }

    [Range(1, int.MaxValue)]
    public int? DurationDays { get; set; }

    [Range(1, int.MaxValue)]
    public int? Quantity { get; set; }

    public string? Route { get; set; }

    public string? Instructions { get; set; }

    public string? Notes { get; set; }
}