namespace Healthcare.Api.DTOs.Visits;

public class VisitDto
{
    public int Id { get; set; }
    public string PatientName { get; set; } = "";
    public string DoctorName { get; set; } = "";
    public string LocationName { get; set; } = "";


    public int AppointmentId { get; set; }

    public int PatientId { get; set; }

    public int DoctorId { get; set; }

    public int LocationId { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime? CheckInTime { get; set; }

    public DateTime? CheckOutTime { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}