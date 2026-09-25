namespace Healthcare.Api.Models;

public class FollowUp
{
    public int Id { get; set; }

    public int PatientId { get; set; }

    public int DoctorId { get; set; }

    public int? AppointmentId { get; set; }

    public string? Notes { get; set; }

    public DateOnly? ReminderDate { get; set; }

    // Allowed values: open, completed
    public string Status { get; set; } = "open";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;


    public Patient Patient { get; set; } = null!;

    public Doctor Doctor { get; set; } = null!;

    public Appointment? Appointment { get; set; }
}