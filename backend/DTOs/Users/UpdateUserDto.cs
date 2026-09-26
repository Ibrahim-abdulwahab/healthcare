using System.ComponentModel.DataAnnotations;

namespace Healthcare.Api.DTOs.Users;

public class UpdateUserDto
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [RegularExpression(
        "^(admin|doctor|staff)$",
        ErrorMessage = "Role must be 'admin', 'doctor', or 'staff'."
    )]
    public string Role { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public int? StaffDoctorId { get; set; }

    public int? StaffOrganizationId { get; set; }
}