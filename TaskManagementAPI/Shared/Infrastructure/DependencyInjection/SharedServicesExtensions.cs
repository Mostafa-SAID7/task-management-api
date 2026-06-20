using TaskManagementAPI.Shared.Application.Services;
using TaskManagementAPI.Shared.Domain.Interfaces;
using TaskManagementAPI.Shared.Infrastructure.Middleware;
using TaskManagementAPI.Shared.Infrastructure.Services;

namespace TaskManagementAPI.Shared.Infrastructure.DependencyInjection;

/// <summary>
/// Extension methods for registering shared cross-cutting services and middleware.
/// </summary>
public static class SharedServicesExtensions
{
    /// <summary>
    /// Registers all shared infrastructure services:
    /// HTTP context accessor, current-user service, notification service, and CORS.
    /// </summary>
    public static IServiceCollection AddSharedServices(this IServiceCollection services)
    {
        // HTTP context accessor — required by CurrentUserService and other infra
        services.AddHttpContextAccessor();

        // Current-user resolution for audit actor tracking in DbContexts and repos
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // Notification fanout service
        services.AddScoped<INotificationService, NotificationService>();

        // Data protection for sensitive values
        services.AddDataProtection();

        // CORS — development allows any origin; production restricts to known hosts
        services.AddCors(options =>
        {
            options.AddPolicy("Development", policy =>
                policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

            options.AddPolicy("Production", policy =>
                policy
                    .WithOrigins(
                        "https://yourdomain.com",
                        "https://www.yourdomain.com",
                        "https://app.yourdomain.com")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials()
                    .WithExposedHeaders("X-Total-Count", "X-Page-Number", "X-Page-Size")
                    .SetPreflightMaxAge(TimeSpan.FromHours(1)));
        });

        return services;
    }

    /// <summary>
    /// Registers middleware in the application pipeline.
    /// Order matters: logging first so every subsequent middleware is covered,
    /// then exception handling, rate limiting, and CORS.
    /// </summary>
    public static IApplicationBuilder UseSharedMiddleware(this IApplicationBuilder app)
    {
        // Structured HTTP logging (uses real Serilog.Context enrichment)
        app.UseMiddleware<LoggingMiddleware>();

        // Centralised exception → RFC 7807 ProblemDetails conversion
        app.UseMiddleware<ExceptionHandlingMiddleware>();

        // Token-bucket rate limiting
        app.UseMiddleware<RateLimitingMiddleware>();

        // CORS
        var env = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();
        app.UseCors(env.IsProduction() ? "Production" : "Development");

        return app;
    }
}
