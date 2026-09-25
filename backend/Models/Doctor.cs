namespace Healthcare.Api.Models;

public class Doctor
{
    public int Id { get; set; }

    public int OrganizationId { get; set; }

    public required string FirstName { get; set; }

    public required string LastName { get; set; }

    public string? Specialty { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? LicenseNumber { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;


    // Relationships

    public Organization Organization { get; set; } = null!;

    public User User { get; set; } = null!;

    public ICollection<DoctorLocation> DoctorLocations { get; set; } = [];

    public ICollection<Appointment> Appointments { get; set; } = [];

    public ICollection<Visit> Visits { get; set; } = [];

    public ICollection<FollowUp> FollowUps { get; set; } = [];

    public ICollection<Prescription> Prescriptions { get; set; } = [];
}