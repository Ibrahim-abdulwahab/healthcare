namespace Healthcare.Api.DTOs.ScheduleExceptions;

public class UpdateScheduleExceptionDto
{
    public int DoctorLocationId { get; set; }

    public DateOnly Date { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public bool IsAvailable { get; set; }

    public string? Reason { get; set; }
}