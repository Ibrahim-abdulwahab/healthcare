namespace Healthcare.Api.Models;

public class LabRequest
{
    public int Id { get; set; }

    public int VisitId { get; set; }

    public required string TestName { get; set; }

    public string? Priority { get; set; }

    // Allowed values: requested, completed
    public string Status { get; set; } = "requested";

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;


    // Relationships

    public Visit Visit { get; set; } = null!;
}