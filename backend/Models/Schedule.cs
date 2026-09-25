namespace Healthcare.Api.Models;

public class Schedule
{
    public int Id { get; set; }

    public int DoctorLocationId { get; set; }

    public int DayOfWeek { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    // Duration of each appointment slot in minutes
    public int SlotDuration { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;


    // Relationships

    public DoctorLocation DoctorLocation { get; set; } = null!;
}