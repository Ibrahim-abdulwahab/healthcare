namespace Healthcare.Api.Models;

public class Consultation
{
    public int Id { get; set; }

    public int VisitId { get; set; }

    public string? ReasonForVisit { get; set; }

    public string? Diagnosis { get; set; }

    public string? Notes { get; set; }

    public string? FollowUpPlan { get; set; }

    public int? NextAppointmentId { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;


    // Relationships

    public Visit Visit { get; set; } = null!;

    public Appointment? NextAppointment { get; set; }

    public User Creator { get; set; } = null!;

    public ICollection<Prescription> Prescriptions { get; set; } = [];
}