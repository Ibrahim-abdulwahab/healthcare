using System.ComponentModel.DataAnnotations;

namespace Healthcare.Api.DTOs.SystemSettings;

public class UpdateSystemSettingDto
{
    [Required]
    public string Key { get; set; } = null!;

    [Required]
    public string? Value { get; set; }

    public string? Description { get; set; }
}