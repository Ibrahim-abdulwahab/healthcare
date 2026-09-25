namespace Healthcare.Api.Models;

public class ScheduleException
{
    public int Id { get; set; }

    public int DoctorLocationId { get; set; }

    public DateOnly Date { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public bool IsAvailable { get; set; }

    public string? Reason { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;


    // Relationships

    public DoctorLocation DoctorLocation { get; set; } = null!;
}