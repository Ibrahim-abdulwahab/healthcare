namespace Healthcare.Api.Models;

public class SystemSetting
{
    public int Id { get; set; }

    public required string Key { get; set; }

    public string? Value { get; set; }

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}