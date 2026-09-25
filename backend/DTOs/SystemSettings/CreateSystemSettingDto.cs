using System.ComponentModel.DataAnnotations;

namespace Healthcare.Api.DTOs.SystemSettings;

public class CreateSystemSettingDto
{
    [Required]
    public string Key { get; set; } = null!;

    [Required]
    public string? Value { get; set; }

    public string? Description { get; set; }
}