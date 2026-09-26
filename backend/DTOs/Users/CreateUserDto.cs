using System.ComponentModel.DataAnnotations;

namespace Healthcare.Api.DTOs.Users;

public class CreateUserDto
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    [Required]
    [RegularExpression(
        "^(admin|doctor|staff)$",
        ErrorMessage = "Role must be 'admin', 'doctor', or 'staff'."
    )]
    public string Role { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public int? StaffDoctorId { get; set; }

    public int? StaffOrganizationId { get; set; }
}