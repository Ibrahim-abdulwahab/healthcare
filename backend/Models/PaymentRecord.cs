namespace Healthcare.Api.Models;

public class PaymentRecord
{
    public int Id { get; set; }

    public int VisitId { get; set; }

    public decimal Amount { get; set; }

    // Allowed values: cash, card, transfer, insurance
    public required string PaymentMethod { get; set; }

    public int CollectedBy { get; set; }

    // Allowed values: hospital, doctor
    public required string PaidTo { get; set; }

    // Allowed values: paid, partial, unpaid
    public string PaymentStatus { get; set; } = "unpaid";

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;


    // Relationships

    public Visit Visit { get; set; } = null!;

    public User Collector { get; set; } = null!;
}