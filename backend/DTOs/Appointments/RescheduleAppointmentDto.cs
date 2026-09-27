namespace Healthcare.Api.DTOs.Appointments;

public class RescheduleAppointmentDto
{
    public int DoctorId { get; set; }
    public int LocationId { get; set; }
    public int VisitTypeId { get; set; }
    public DateTime StartDatetime { get; set; }
    public DateTime? ExpectedUpdatedAt { get; set; }
}
