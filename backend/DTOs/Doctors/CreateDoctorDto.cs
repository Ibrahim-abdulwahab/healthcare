using System.ComponentModel.DataAnnotations;

namespace Healthcare.Api.DTOs.Doctors;

public class CreateDoctorDto
{
    public int UserId { get; set; }

    public int OrganizationId { get; set; }

    [Required]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    public string LastName { get; set; } = string.Empty;

    public string? Specialty { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? LicenseNumber { get; set; }

    public bool IsActive { get; set; } = true;
}