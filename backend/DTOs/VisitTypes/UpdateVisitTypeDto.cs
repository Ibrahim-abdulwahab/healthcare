using System.ComponentModel.DataAnnotations;

namespace Healthcare.Api.DTOs.VisitTypes;

public class UpdateVisitTypeDto
{
    [Required]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int DefaultDuration { get; set; }

    public bool IsActive { get; set; }
}