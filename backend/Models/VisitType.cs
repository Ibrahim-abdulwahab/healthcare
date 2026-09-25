namespace Healthcare.Api.Models;

public class VisitType
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public string? Description { get; set; }

    // Default visit duration in minutes
    public int DefaultDuration { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;


    // Relationships

    public ICollection<Appointment> Appointments { get; set; } = [];
}