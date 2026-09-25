namespace Healthcare.Api.DTOs.Prescriptions;

public class PrescriptionDto
{
    public int Id { get; set; }

    public int ConsultationId { get; set; }

    public int PatientId { get; set; }

    public int DoctorId { get; set; }

    public DateOnly PrescriptionDate { get; set; }

    public string? Notes { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}