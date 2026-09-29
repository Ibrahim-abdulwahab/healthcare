namespace Healthcare.Api.DTOs.Schedules;

public class ScheduleDto
{
    public int Id { get; set; }

    public int DoctorLocationId { get; set; }
    public string DoctorLocationName { get; set; } = "";

    public int DayOfWeek { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public int SlotDuration { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
