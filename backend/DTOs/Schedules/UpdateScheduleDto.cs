namespace Healthcare.Api.DTOs.Schedules;

public class UpdateScheduleDto
{
    public int DoctorLocationId { get; set; }

    public int DayOfWeek { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public int SlotDuration { get; set; }

    public bool IsActive { get; set; }
}