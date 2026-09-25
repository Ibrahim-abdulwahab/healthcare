namespace Healthcare.Api.Models;

public class Prescription
{
    public int Id { get; set; }

    public int ConsultationId { get; set; }

    public int PatientId { get; set; }

    public int DoctorId { get; set; }

    public DateOnly PrescriptionDate { get; set; }

    public string? Notes { get; set; }

    // Allowed values: active, canceled
    public string Status { get; set; } = "active";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;


    // Relationships

    public Consultation Consultation { get; set; } = null!;

    public Patient Patient { get; set; } = null!;

    public Doctor Doctor { get; set; } = null!;

    public ICollection<PrescriptionItem> Items { get; set; } = [];
}