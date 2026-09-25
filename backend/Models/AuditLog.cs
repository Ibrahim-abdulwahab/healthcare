namespace Healthcare.Api.Models;

public class AuditLog
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public required string Action { get; set; }

    public required string TableName { get; set; }

    public int RecordId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


    // Relationships

    public User User { get; set; } = null!;
}