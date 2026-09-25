using System.ComponentModel.DataAnnotations;

namespace Healthcare.Api.DTOs.Prescriptions;

public class UpdatePrescriptionDto
{
    public int ConsultationId { get; set; }

    public int PatientId { get; set; }

    public int DoctorId { get; set; }

    public DateOnly PrescriptionDate { get; set; }

    public string? Notes { get; set; }

    [Required]
    [RegularExpression(
        "^(active|canceled)$",
        ErrorMessage = "Invalid prescription status."
    )]
    public string Status { get; set; } = "active";
}