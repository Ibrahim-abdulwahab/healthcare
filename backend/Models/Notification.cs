namespace Healthcare.Api.Models;

public class Notification
{
    public int Id { get; set; }

    public int UserId { get; set; }

    // Allowed values: email, sms, in-app
    public required string Type { get; set; }

    public required string Message { get; set; }

    public bool IsRead { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


    // Relationships

    public User User { get; set; } = null!;
}