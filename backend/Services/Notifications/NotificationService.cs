using Healthcare.Api.Data;
using Healthcare.Api.DTOs.Notifications;
using Healthcare.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Healthcare.Api.Services.Notifications;

public class NotificationService : INotificationService
{
    private readonly HealthcareDbContext _context;

    private static readonly string[] ValidTypes =
    {
        "email",
        "sms",
        "in-app"
    };

    public NotificationService(HealthcareDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<NotificationDto>> GetAllAsync()
    {
        return await _context.Notifications
            .AsNoTracking()
            .Select(n => new NotificationDto
            {
                Id = n.Id,
                UserId = n.UserId,
                Type = n.Type,
                Message = n.Message,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<NotificationDto?> GetByIdAsync(int id)
    {
        return await _context.Notifications
            .AsNoTracking()
            .Where(n => n.Id == id)
            .Select(n => new NotificationDto
            {
                Id = n.Id,
                UserId = n.UserId,
                Type = n.Type,
                Message = n.Message,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<NotificationDto> CreateAsync(
        CreateNotificationDto dto)
    {
        await ValidateUserAsync(dto.UserId);
        ValidateType(dto.Type);

        var notification = new Notification
        {
            UserId = dto.UserId,
            Type = dto.Type,
            Message = dto.Message,
            IsRead = dto.IsRead,
            CreatedAt = DateTime.UtcNow
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        return ToDto(notification);
    }

    public async Task<NotificationDto?> UpdateAsync(
        int id,
        UpdateNotificationDto dto)
    {
        var notification =
            await _context.Notifications.FindAsync(id);

        if (notification is null)
            return null;

        await ValidateUserAsync(dto.UserId);
        ValidateType(dto.Type);

        notification.UserId = dto.UserId;
        notification.Type = dto.Type;
        notification.Message = dto.Message;
        notification.IsRead = dto.IsRead;

        await _context.SaveChangesAsync();

        return ToDto(notification);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var notification =
            await _context.Notifications.FindAsync(id);

        if (notification is null)
            return false;

        _context.Notifications.Remove(notification);
        await _context.SaveChangesAsync();

        return true;
    }

    private async Task ValidateUserAsync(int userId)
    {
        var userExists = await _context.Users
            .AsNoTracking()
            .AnyAsync(u => u.Id == userId);

        if (!userExists)
        {
            throw new ArgumentException(
                $"User with ID {userId} does not exist."
            );
        }
    }

    private static void ValidateType(string type)
    {
        if (!ValidTypes.Contains(type))
        {
            throw new ArgumentException(
                "Notification type must be email, sms, or in-app."
            );
        }
    }

    private static NotificationDto ToDto(
        Notification notification)
    {
        return new NotificationDto
        {
            Id = notification.Id,
            UserId = notification.UserId,
            Type = notification.Type,
            Message = notification.Message,
            IsRead = notification.IsRead,
            CreatedAt = notification.CreatedAt
        };
    }
}