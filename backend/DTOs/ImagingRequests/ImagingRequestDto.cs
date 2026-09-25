namespace Healthcare.Api.DTOs.ImagingRequests;

public class ImagingRequestDto
{
    public int Id { get; set; }

    public int VisitId { get; set; }

    public string ServiceName { get; set; } = string.Empty;

    public string? BodyPart { get; set; }

    public string? Priority { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateOnly? ScheduledDate { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}