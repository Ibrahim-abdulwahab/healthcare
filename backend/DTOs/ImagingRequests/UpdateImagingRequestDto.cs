using System.ComponentModel.DataAnnotations;

namespace Healthcare.Api.DTOs.ImagingRequests;

public class UpdateImagingRequestDto
{
    public int VisitId { get; set; }

    [Required]
    public string ServiceName { get; set; } = string.Empty;

    public string? BodyPart { get; set; }

    public string? Priority { get; set; }

    [Required]
    [RegularExpression(
        "^(requested|scheduled|completed)$",
        ErrorMessage = "Invalid imaging request status."
    )]
    public string Status { get; set; } = string.Empty;

    public DateOnly? ScheduledDate { get; set; }

    public string? Notes { get; set; }
}