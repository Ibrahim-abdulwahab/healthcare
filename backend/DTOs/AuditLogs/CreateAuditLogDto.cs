using System.ComponentModel.DataAnnotations;

namespace Healthcare.Api.DTOs.AuditLogs;

public class CreateAuditLogDto
{
    public int UserId { get; set; }

    [Required]
    public string Action { get; set; } = null!;

    [Required]
    public string TableName { get; set; } = null!;

    public int RecordId { get; set; }
}