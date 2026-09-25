using System.ComponentModel.DataAnnotations;

namespace Healthcare.Api.DTOs.PaymentRecords;

public class CreatePaymentRecordDto
{
    public int VisitId { get; set; }

    public decimal Amount { get; set; }

    [Required]
    [RegularExpression(
        "^(cash|card|transfer|insurance)$",
        ErrorMessage = "Invalid payment method."
    )]
    public string PaymentMethod { get; set; } = string.Empty;

    public int CollectedBy { get; set; }

    [Required]
    [RegularExpression(
        "^(hospital|doctor)$",
        ErrorMessage = "Invalid paid-to value."
    )]
    public string PaidTo { get; set; } = string.Empty;

    [Required]
    [RegularExpression(
        "^(paid|partial|unpaid)$",
        ErrorMessage = "Invalid payment status."
    )]
    public string PaymentStatus { get; set; } = string.Empty;

    public string? Notes { get; set; }
}