namespace Healthcare.Api.Models;

public class CodeList
{
    public int Id { get; set; }

    public required string Category { get; set; }

    public required string Code { get; set; }

    public required string ValueEn { get; set; }

    public string? ValueAr { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}