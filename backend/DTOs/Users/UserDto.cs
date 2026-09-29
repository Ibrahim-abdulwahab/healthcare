namespace Healthcare.Api.DTOs.Users;

public class UserDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public int? StaffDoctorId { get; set; }
    public string? StaffDoctorName { get; set; }

    public int? StaffOrganizationId { get; set; }
    public string? StaffOrganizationName { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
