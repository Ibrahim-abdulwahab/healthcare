namespace Healthcare.Api.Models;

public class Visit
{
    public int Id { get; set; }

    public int AppointmentId { get; set; }

    public int PatientId { get; set; }

    public int DoctorId { get; set; }

    public int LocationId { get; set; }

    // Allowed values: in_progress, completed
    public string Status { get; set; } = "in_progress";

    public DateTime? CheckInTime { get; set; }

    public DateTime? CheckOutTime { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;


    // Relationships

    public Appointment Appointment { get; set; } = null!;

    public Patient Patient { get; set; } = null!;

    public Doctor Doctor { get; set; } = null!;

    public Location Location { get; set; } = null!;

    public Consultation? Consultation { get; set; }

    public ICollection<PaymentRecord> PaymentRecords { get; set; } = [];

    public ICollection<LabRequest> LabRequests { get; set; } = [];

    public ICollection<ImagingRequest> ImagingRequests { get; set; } = [];
}