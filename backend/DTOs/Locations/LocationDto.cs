namespace Healthcare.Api.DTOs.Locations;

public class LocationDto
{
    public int Id { get; set; }

    public int OrganizationId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Address { get; set; }

    public string? City { get; set; }

    public string? Phone { get; set; }

    public string? RoomNumber { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}