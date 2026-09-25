namespace Healthcare.Api.DTOs.LabRequests;

public class LabRequestDto
{
    public int Id { get; set; }

    public int VisitId { get; set; }

    public string TestName { get; set; } = string.Empty;

    public string? Priority { get; set; }

    public string Status { get; set; } = string.Empty;

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}