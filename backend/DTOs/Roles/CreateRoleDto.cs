using System.ComponentModel.DataAnnotations;

namespace Healthcare.Api.DTOs.Roles;

public class CreateRoleDto
{
    [Required]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
}