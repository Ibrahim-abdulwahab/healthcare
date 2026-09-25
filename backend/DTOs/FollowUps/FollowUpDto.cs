namespace Healthcare.Api.DTOs.FollowUps;

public class FollowUpDto
{
    public int Id { get; set; }

    public int PatientId { get; set; }

    public int DoctorId { get; set; }

    public int? AppointmentId { get; set; }

    public string? Notes { get; set; }

    public DateOnly? ReminderDate { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}