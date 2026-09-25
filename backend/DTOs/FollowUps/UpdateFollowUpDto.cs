using System.ComponentModel.DataAnnotations;

namespace Healthcare.Api.DTOs.FollowUps;

public class UpdateFollowUpDto
{
    public int PatientId { get; set; }

    public int DoctorId { get; set; }

    public int? AppointmentId { get; set; }

    public string? Notes { get; set; }

    public DateOnly? ReminderDate { get; set; }

    [Required]
    [RegularExpression(
        "^(open|completed)$",
        ErrorMessage = "Invalid follow-up status."
    )]
    public string Status { get; set; } = "open";
}