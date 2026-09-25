using System.ComponentModel.DataAnnotations;

namespace Healthcare.Api.DTOs.Locations;

public class CreateLocationDto
{
    public int OrganizationId { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public string? Address { get; set; }

    public string? City { get; set; }

    public string? Phone { get; set; }

    public string? RoomNumber { get; set; }

    public bool IsActive { get; set; } = true;
}