namespace Healthcare.Api.DTOs.Organizations;

using System.ComponentModel.DataAnnotations;
public class CreateOrganizationDto
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    [RegularExpression(
        "^(hospital|clinic)$",
        ErrorMessage = "Type must be either 'hospital' or 'clinic'."
    )]
    public string Type { get; set; } = string.Empty;

    public string? Address { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public bool IsActive { get; set; } = true;
}