namespace Healthcare.Api.Models;

public class Appointment
{
    public int Id { get; set; }

    public int PatientId { get; set; }

    public int DoctorId { get; set; }

    public int LocationId { get; set; }

    public int VisitTypeId { get; set; }

    public DateTime StartDatetime { get; set; }

    public DateTime EndDatetime { get; set; }

    // Allowed values:
    // booked, arrived, waiting, in_consultation, completed, cancelled
    public string Status { get; set; } = "booked";

    public string? Notes { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;


    // Relationships

    public Patient Patient { get; set; } = null!;

    public Doctor Doctor { get; set; } = null!;

    public Location Location { get; set; } = null!;

    public VisitType VisitType { get; set; } = null!;

    public User Creator { get; set; } = null!;

    public ICollection<Visit> Visits { get; set; } = [];

    public ICollection<FollowUp> FollowUps { get; set; } = [];
}