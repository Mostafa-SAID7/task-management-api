using TaskManagementAPI.Shared.Domain.Interfaces;

namespace TaskManagementAPI.Shared.Infrastructure.Services;

/// <summary>
/// Default implementation of INotificationService for broadcasting notifications.
/// </summary>
public class NotificationService : INotificationService
{
    private readonly ILogger<NotificationService> _logger;

    /// <summary>
    /// Initializes a new instance of the NotificationService class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public NotificationService(ILogger<NotificationService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Sends a notification to a specific user.
    /// </summary>
    /// <param name="userId">The ID of the user to notify.</param>
    /// <param name="message">The notification message.</param>
    public async Task NotifyAsync(string userId, string message)
    {
        _logger.LogInformation("Sending notification to user '{UserId}': {Message}", userId, message);
        // TODO: Implement SignalR notification when Notifications module is ready
        await Task.CompletedTask;
    }

    /// <summary>
    /// Broadcasts a message to a specific group.
    /// </summary>
    /// <param name="groupName">The group name.</param>
    /// <param name="message">The message to broadcast.</param>
    public async Task BroadcastAsync(string groupName, string message)
    {
        _logger.LogInformation("Broadcasting message to group '{Group}': {Message}", groupName, message);
        // TODO: Implement SignalR broadcasting when Notifications module is ready
        await Task.CompletedTask;
    }
}
