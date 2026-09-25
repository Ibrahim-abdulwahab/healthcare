namespace Healthcare.Api.Models;

public class ImagingRequest
{
    public int Id { get; set; }

    public int VisitId { get; set; }

    public required string ServiceName { get; set; }

    public string? BodyPart { get; set; }

    public string? Priority { get; set; }

    // Allowed values: requested, scheduled, completed
    public string Status { get; set; } = "requested";

    public DateOnly? ScheduledDate { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;


    // Relationships

    public Visit Visit { get; set; } = null!;
}