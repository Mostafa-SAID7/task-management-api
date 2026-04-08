using Microsoft.Extensions.DependencyInjection;
using TaskManagementAPI.Shared.Domain.Interfaces;
using TaskManagementAPI.Shared.Infrastructure.Middleware;
using TaskManagementAPI.Shared.Infrastructure.Services;

namespace TaskManagementAPI.Shared.Infrastructure.DependencyInjection;

/// <summary>
/// Extension methods for registering shared services and middleware in the DI container.
/// </summary>
public static class SharedServicesExtensions
{
    /// <summary>
    /// Registers all shared infrastructure services including middleware, logging, and common utilities.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddSharedServices(this IServiceCollection services)
    {
        // Register notification service
        services.AddScoped<INotificationService, NotificationService>();

        // Add CORS
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", builder =>
            {
                builder
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });

        // Add HTTP context accessor for accessing current user context
        services.AddHttpContextAccessor();

        return services;
    }

    /// <summary>
    /// Registers middleware in the application pipeline.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The application builder for chaining.</returns>
    public static IApplicationBuilder UseSharedMiddleware(this IApplicationBuilder app)
    {
        app.UseCors("AllowAll");
        // TODO: Add exception handling and logging middleware when properly configured
        return app;
    }
}
