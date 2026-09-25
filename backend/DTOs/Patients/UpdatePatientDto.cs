using System.ComponentModel.DataAnnotations;

namespace Healthcare.Api.DTOs.Patients;

public class UpdatePatientDto
{
    [Required]
    public string Mrn { get; set; } = string.Empty;

    [Required]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    public string LastName { get; set; } = string.Empty;

    public string? Sex { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public string? Phone { get; set; }

    [EmailAddress]
    public string? Email { get; set; }

    public string? Address { get; set; }

    public string? NationalId { get; set; }
}