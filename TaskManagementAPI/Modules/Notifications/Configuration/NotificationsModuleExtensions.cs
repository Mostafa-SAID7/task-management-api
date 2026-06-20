using Microsoft.EntityFrameworkCore;
using TaskManagementAPI.Modules.Notifications.Application.Services;
using TaskManagementAPI.Modules.Notifications.Infrastructure.Persistence;
using TaskManagementAPI.Modules.Notifications.Infrastructure.Services;
using TaskManagementAPI.Modules.Notifications.Presentation.Hubs;

namespace TaskManagementAPI.Modules.Notifications.Configuration;

/// <summary>
/// Extension methods for registering the Notifications module services.
/// </summary>
public static class NotificationsModuleExtensions
{
    /// <summary>
    /// Adds the Notifications module services to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddNotificationsModule(this IServiceCollection services, IConfiguration configuration)
    {
        // Add DbContext
        services.AddDbContext<NotificationsDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        // Add services
        services.AddScoped<NotificationService>();
        services.AddScoped<INotificationRepository, NotificationRepository>();

        // Add SignalR
        services.AddSignalR();

        return services;
    }

    /// <summary>
    /// Maps the Notifications module endpoints and hubs.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The application builder.</returns>
    public static IApplicationBuilder MapNotificationsEndpoints(this IApplicationBuilder app)
    {
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapHub<TaskUpdatesHub>("/hubs/task-updates");
        });

        return app;
    }
}
