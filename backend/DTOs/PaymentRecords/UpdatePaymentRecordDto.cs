using System.ComponentModel.DataAnnotations;

namespace Healthcare.Api.DTOs.PaymentRecords;

public class UpdatePaymentRecordDto
{
    public int VisitId { get; set; }

    [Range(typeof(decimal), "0.0000000000000000000000000001", "79228162514264337593543950335")]
    public decimal Amount { get; set; }

    [Required]
    [RegularExpression(
        "^(cash|card|transfer|insurance)$",
        ErrorMessage = "Invalid payment method."
    )]
    public string PaymentMethod { get; set; } = string.Empty;

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