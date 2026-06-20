namespace TaskManagementAPI.Shared.Infrastructure.DependencyInjection;

/// <summary>
/// Extension methods for health check and readiness endpoints.
/// </summary>
public static class HealthCheckExtensions
{
    /// <summary>
    /// Maps health check endpoints for monitoring and Kubernetes probes.
    /// </summary>
    public static IEndpointRouteBuilder MapHealthChecks(this IEndpointRouteBuilder routes)
    {
        // Health check endpoint - for general monitoring
        routes.MapGet("/health", (ILogger<Program> logger) =>
        {
            var health = new
            {
                status = "healthy",
                timestamp = DateTime.UtcNow.ToString("o"),
                environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
                version = "1.0.0",
                uptime = Environment.TickCount / 1000.0
            };
            logger.LogInformation("Health check passed");
            return Results.Ok(health);
        })
        .Produces(StatusCodes.Status200OK)
        .WithName("Health")
        .AllowAnonymous();

        // Ready endpoint - for Kubernetes readiness probes
        routes.MapGet("/ready", () => Results.Ok(new { ready = true }))
            .Produces(StatusCodes.Status200OK)
            .WithName("Ready")
            .AllowAnonymous();

        return routes;
    }
}
