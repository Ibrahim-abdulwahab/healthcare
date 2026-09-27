using System.ComponentModel.DataAnnotations;

namespace Healthcare.Api.DTOs.Appointments;

public class ChangeAppointmentStatusDto
{
    [Required]
    [RegularExpression("^(arrived|waiting|in_consultation|completed|cancelled)$")]
    public string Status { get; set; } = "";
    public DateTime? ExpectedUpdatedAt { get; set; }
}
