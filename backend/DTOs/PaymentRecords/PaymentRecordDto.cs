namespace Healthcare.Api.DTOs.PaymentRecords;

public class PaymentRecordDto
{
    public int Id { get; set; }

    public int VisitId { get; set; }

    public decimal Amount { get; set; }

    public string PaymentMethod { get; set; } = string.Empty;

    public int CollectedBy { get; set; }

    public string PaidTo { get; set; } = string.Empty;

    public string PaymentStatus { get; set; } = string.Empty;

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}