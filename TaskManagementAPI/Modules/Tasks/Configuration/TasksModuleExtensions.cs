using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaskManagementAPI.Modules.Tasks.Application.Services;
using TaskManagementAPI.Modules.Tasks.Infrastructure.Persistence;
using TaskManagementAPI.Modules.Tasks.Infrastructure.Services;

namespace TaskManagementAPI.Modules.Tasks.Configuration;

/// <summary>
/// Extension methods for registering the Tasks module with dependency injection.
/// </summary>
public static class TasksModuleExtensions
{
    /// <summary>
    /// Adds the Tasks module to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddTasksModule(
        this IServiceCollection services, IConfiguration configuration)
    {
        // Register DbContext
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<TasksDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Register repositories
        services.AddScoped<ITaskRepository, TaskRepository>();

        // Register services
        services.AddScoped<ITaskService, TaskService>();

        return services;
    }

    /// <summary>
    /// Maps the Tasks module endpoints.
    /// </summary>
    /// <param name="app">The web application builder.</param>
    /// <returns>The web application for chaining.</returns>
    public static WebApplication MapTasksEndpoints(this WebApplication app)
    {
        // Endpoints are automatically mapped via controller routing
        return app;
    }
}
