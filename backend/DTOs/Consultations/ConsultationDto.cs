namespace Healthcare.Api.DTOs.Consultations;

public class ConsultationDto
{
    public int Id { get; set; }

    public int VisitId { get; set; }

    public string? ReasonForVisit { get; set; }

    public string? Diagnosis { get; set; }

    public string? Notes { get; set; }

    public string? FollowUpPlan { get; set; }

    public int? NextAppointmentId { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}