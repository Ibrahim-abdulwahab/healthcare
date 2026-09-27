namespace Healthcare.Api.DTOs.Appointments;

public class AppointmentDto
{
    public int Id { get; set; }
    public string PatientName { get; set; } = "";
    public string DoctorName { get; set; } = "";
    public string LocationName { get; set; } = "";
    public string VisitTypeName { get; set; } = "";


    public int PatientId { get; set; }

    public int DoctorId { get; set; }

    public int LocationId { get; set; }

    public int VisitTypeId { get; set; }

    public DateTime StartDatetime { get; set; }

    public DateTime EndDatetime { get; set; }

    public string Status { get; set; } = string.Empty;

    public string? Notes { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
    public bool HasVisits { get; set; }
}
