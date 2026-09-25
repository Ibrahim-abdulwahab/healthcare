namespace Healthcare.Api.Models;

public class Organization
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public required string Type { get; set; }

    public string? Address { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Relationships
    public ICollection<Location> Locations { get; set; } = [];

    public ICollection<Doctor> Doctors { get; set; } = [];
}