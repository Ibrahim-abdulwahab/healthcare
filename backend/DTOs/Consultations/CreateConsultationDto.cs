namespace Healthcare.Api.DTOs.Consultations;

public class CreateConsultationDto
{
    public int VisitId { get; set; }

    public string? ReasonForVisit { get; set; }

    public string? Diagnosis { get; set; }

    public string? Notes { get; set; }

    public string? FollowUpPlan { get; set; }

    public int? NextAppointmentId { get; set; }
}