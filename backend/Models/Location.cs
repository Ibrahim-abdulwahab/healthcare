namespace Healthcare.Api.Models;

public class Location
{
    public int Id { get; set; }

    public int OrganizationId { get; set; }

    public required string Name { get; set; }

    public string? Address { get; set; }

    public string? City { get; set; }

    public string? Phone { get; set; }

    public string? RoomNumber { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;


    // Relationships

    public Organization Organization { get; set; } = null!;

    public ICollection<DoctorLocation> DoctorLocations { get; set; } = [];

    public ICollection<Appointment> Appointments { get; set; } = [];

    public ICollection<Visit> Visits { get; set; } = [];
}