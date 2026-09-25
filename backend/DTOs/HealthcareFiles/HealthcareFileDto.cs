namespace Healthcare.Api.DTOs.HealthcareFiles;

public class HealthcareFileDto
{
    public int Id { get; set; }

    public string TableName { get; set; } = null!;

    public int RecordId { get; set; }

    public string FileName { get; set; } = null!;

    public string FileUrl { get; set; } = null!;

    public int UploadedBy { get; set; }

    public DateTime CreatedAt { get; set; }
}