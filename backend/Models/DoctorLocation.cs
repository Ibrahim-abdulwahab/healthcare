namespace Healthcare.Api.Models;

public class DoctorLocation
{
    public int Id { get; set; }

    public int DoctorId { get; set; }

    public int LocationId { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;


    // Relationships

    public Doctor Doctor { get; set; } = null!;

    public Location Location { get; set; } = null!;

    public ICollection<Schedule> Schedules { get; set; } = [];

    public ICollection<ScheduleException> ScheduleExceptions { get; set; } = [];
}