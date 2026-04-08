using TaskManagementAPI.Modules.Notifications.Application.DTOs;
using TaskManagementAPI.Modules.Notifications.Application.Services;

namespace TaskManagementAPI.Modules.Notifications.Presentation.Controllers;

/// <summary>
/// API controller for notification management operations.
/// </summary>
[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly NotificationService _notificationService;

    /// <summary>
    /// Initializes a new instance of the NotificationsController class.
    /// </summary>
    /// <param name="notificationService">The notification service.</param>
    public NotificationsController(NotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    /// <summary>
    /// Gets notifications for the current user.
    /// </summary>
    /// <param name="pageNumber">The page number (default 1).</param>
    /// <param name="pageSize">The page size (default 20).</param>
    /// <returns>A paginated list of notifications.</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<NotificationListResponse>> GetNotifications(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var userId = User.FindFirst("sub")?.Value ?? string.Empty;
        var (notifications, totalCount) = await _notificationService.GetUserNotificationsAsync(userId, pageNumber, pageSize);

        var response = new NotificationListResponse
        {
            Data = notifications.Select(MapToDto),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        return Ok(response);
    }

    /// <summary>
    /// Marks a notification as read.
    /// </summary>
    /// <param name="id">The notification ID.</param>
    /// <returns>No content if successful.</returns>
    [HttpPut("{id}/read")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        var success = await _notificationService.MarkAsReadAsync(id);
        if (!success)
            return NotFound();

        return NoContent();
    }

    /// <summary>
    /// Deletes a notification.
    /// </summary>
    /// <param name="id">The notification ID.</param>
    /// <returns>No content if successful.</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteNotification(Guid id)
    {
        var success = await _notificationService.DeleteNotificationAsync(id);
        if (!success)
            return NotFound();

        return NoContent();
    }

    /// <summary>
    /// Maps a Notification entity to a NotificationDto.
    /// </summary>
    private static NotificationDto MapToDto(Domain.Entities.Notification notification)
    {
        return new NotificationDto
        {
            Id = notification.Id,
            UserId = notification.UserId,
            Type = notification.Type,
            Message = notification.Message,
            IsRead = notification.IsRead,
            Status = notification.Status,
            CreatedAt = notification.CreatedAt,
            ReadAt = notification.ReadAt
        };
    }
}
