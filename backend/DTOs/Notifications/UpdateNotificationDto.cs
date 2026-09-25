using System.ComponentModel.DataAnnotations;

namespace Healthcare.Api.DTOs.Notifications;

public class UpdateNotificationDto
{
    public int UserId { get; set; }

    [Required]
    public string Type { get; set; } = null!;

    [Required]
    public string Message { get; set; } = null!;

    public bool IsRead { get; set; }
}