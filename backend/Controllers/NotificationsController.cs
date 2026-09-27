using Healthcare.Api.DTOs.Notifications;
using Healthcare.Api.Services.Notifications;
using Microsoft.AspNetCore.Mvc;

namespace Healthcare.Api.Controllers;

[ApiController]
[Route("api/notifications")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(
        INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<NotificationDto>>> GetAll()
    {
        var notifications =
            await _notificationService.GetAllAsync();

        return Ok(notifications);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<NotificationDto>> GetById(int id)
    {
        var notification =
            await _notificationService.GetByIdAsync(id);

        if (notification is null)
            return NotFound();

        return Ok(notification);
    }

    [HttpPost]
    public async Task<ActionResult<NotificationDto>> Create(
        CreateNotificationDto dto)
    {
        try
        {
            var notification =
                await _notificationService.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = notification.Id },
                notification
            );
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPatch("{id:int}/read")]
    public async Task<ActionResult<NotificationDto>> MarkRead(int id)
    {
        var notification = await _notificationService.MarkReadAsync(id);
        return notification is null ? NotFound() : Ok(notification);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<NotificationDto>> Update(
        int id,
        UpdateNotificationDto dto)
    {
        try
        {
            var notification =
                await _notificationService.UpdateAsync(id, dto);

            if (notification is null)
                return NotFound();

            return Ok(notification);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted =
            await _notificationService.DeleteAsync(id);

        if (!deleted)
            return NotFound();

        return NoContent();
    }
}
