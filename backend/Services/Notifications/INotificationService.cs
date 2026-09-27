using Healthcare.Api.DTOs.Notifications;

namespace Healthcare.Api.Services.Notifications;

public interface INotificationService
{
    Task<IEnumerable<NotificationDto>> GetAllAsync();

    Task<NotificationDto?> GetByIdAsync(int id);

    Task<NotificationDto> CreateAsync(CreateNotificationDto dto);

    Task<NotificationDto?> UpdateAsync(
        int id,
        UpdateNotificationDto dto
    );

    Task<bool> DeleteAsync(int id);
    Task<NotificationDto?> MarkReadAsync(int id);
}
