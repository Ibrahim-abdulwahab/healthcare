using System.ComponentModel.DataAnnotations;

namespace Healthcare.Api.DTOs.LabRequests;

public class UpdateLabRequestDto
{
    public int VisitId { get; set; }

    [Required]
    public string TestName { get; set; } = string.Empty;

    [Required]
    public string? Priority { get; set; }

    [Required]
    [RegularExpression(
        "^(requested|completed)$",
        ErrorMessage = "Invalid lab request status."
    )]
    public string Status { get; set; } = string.Empty;

    public string? Notes { get; set; }
}