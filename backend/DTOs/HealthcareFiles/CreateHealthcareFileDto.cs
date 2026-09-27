using System.ComponentModel.DataAnnotations;

namespace Healthcare.Api.DTOs.HealthcareFiles;

public class CreateHealthcareFileDto
{
    [Required]
    public string TableName { get; set; } = null!;

    public int RecordId { get; set; }

    [Required]
    public string FileName { get; set; } = null!;

    [Required]
    public string FileUrl { get; set; } = null!;
}