namespace Healthcare.Api.Models;

public class HealthcareFile
{
    public int Id { get; set; }

    public required string TableName { get; set; }

    public int RecordId { get; set; }

    public required string FileName { get; set; }

    public required string FileUrl { get; set; }

    public int UploadedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


    // Relationships

    public User Uploader { get; set; } = null!;
}