namespace Healthcare.Api.DTOs.AuditLogs;

public class AuditLogDto
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string Action { get; set; } = null!;

    public string TableName { get; set; } = null!;

    public int RecordId { get; set; }

    public DateTime CreatedAt { get; set; }
}