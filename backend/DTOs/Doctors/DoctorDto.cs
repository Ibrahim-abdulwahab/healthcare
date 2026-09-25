namespace Healthcare.Api.DTOs.Doctors;

public class DoctorDto
{
    public int Id { get; set; }

    public int OrganizationId { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string? Specialty { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? LicenseNumber { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}