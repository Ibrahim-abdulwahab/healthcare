namespace Healthcare.Api.Models;

public class Patient
{
    public int Id { get; set; }

    public required string Mrn { get; set; }

    public required string FirstName { get; set; }

    public required string LastName { get; set; }

    public string? Sex { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? Address { get; set; }

    public string? NationalId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;


    // Relationships

    public ICollection<Appointment> Appointments { get; set; } = [];

    public ICollection<Visit> Visits { get; set; } = [];

    public ICollection<FollowUp> FollowUps { get; set; } = [];

    public ICollection<Prescription> Prescriptions { get; set; } = [];
}