using System.ComponentModel.DataAnnotations;

namespace Healthcare.Api.DTOs.CodeLists;

public class UpdateCodeListDto
{
    [Required]
    public string Category { get; set; } = null!;

    [Required]
    public string Code { get; set; } = null!;

    [Required]
    public string ValueEn { get; set; } = null!;

    public string? ValueAr { get; set; }

    public bool IsActive { get; set; }
}