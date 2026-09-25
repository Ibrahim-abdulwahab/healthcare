namespace Healthcare.Api.DTOs.CodeLists;

public class CodeListDto
{
    public int Id { get; set; }

    public string Category { get; set; } = null!;

    public string Code { get; set; } = null!;

    public string ValueEn { get; set; } = null!;

    public string? ValueAr { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }
}