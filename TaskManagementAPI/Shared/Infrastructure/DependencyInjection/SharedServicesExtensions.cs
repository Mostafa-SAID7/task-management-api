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
    /// Configures CORS with environment-specific policies for security.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddSharedServices(this IServiceCollection services)
    {
        // Register notification service
        services.AddScoped<INotificationService, NotificationService>();

        // Add CORS with environment-specific configuration
        services.AddCors(options =>
        {
            // Development policy - allows all origins for easier testing
            options.AddPolicy("Development", builder =>
            {
                builder
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });

            // Production policy - restrict to specific origins
            options.AddPolicy("Production", builder =>
            {
                var allowedOrigins = new[]
                {
                    "https://yourdomain.com",
                    "https://www.yourdomain.com",
                    "https://app.yourdomain.com"
                };

                builder
                    .WithOrigins(allowedOrigins)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials()
                    .WithExposedHeaders("X-Total-Count", "X-Page-Number", "X-Page-Size")
                    .SetPreflightMaxAge(TimeSpan.FromHours(1));
            });
        });

        // Add HTTP context accessor for accessing current user context
        services.AddHttpContextAccessor();

        // Add data protection for sensitive data encryption
        services.AddDataProtection();

        return services;
    }

    /// <summary>
    /// Registers middleware in the application pipeline.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The application builder for chaining.</returns>
    public static IApplicationBuilder UseSharedMiddleware(this IApplicationBuilder app)
    {
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.UseMiddleware<RateLimitingMiddleware>();
        
        // Use environment-specific CORS policy
        var corsPolicy = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>().IsProduction()
            ? "Production"
            : "Development";
        app.UseCors(corsPolicy);
        
        return app;
    }
}
