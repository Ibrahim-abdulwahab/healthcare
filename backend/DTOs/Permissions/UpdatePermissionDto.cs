using System.ComponentModel.DataAnnotations;

namespace Healthcare.Api.DTOs.Permissions;

public class UpdatePermissionDto
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Code { get; set; } = string.Empty;

    public string? Description { get; set; }
}