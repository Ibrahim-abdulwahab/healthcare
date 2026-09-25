using System.ComponentModel.DataAnnotations;

namespace Healthcare.Api.DTOs.Visits;

public class UpdateVisitDto
{
    public int AppointmentId { get; set; }

    public int PatientId { get; set; }

    public int DoctorId { get; set; }

    public int LocationId { get; set; }

    [Required]
    [RegularExpression(
        "^(in_progress|completed)$",
        ErrorMessage = "Invalid visit status."
    )]
    public string Status { get; set; } = "in_progress";

    public DateTime? CheckInTime { get; set; }

    public DateTime? CheckOutTime { get; set; }
}