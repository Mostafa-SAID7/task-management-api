using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaskManagementAPI.Modules.Projects.Application.Services;
using TaskManagementAPI.Modules.Projects.Infrastructure.Persistence;
using TaskManagementAPI.Modules.Projects.Infrastructure.Persistence.Seeds;
using TaskManagementAPI.Modules.Projects.Infrastructure.Services;

namespace TaskManagementAPI.Modules.Projects.Configuration;

/// <summary>
/// Extension methods for registering the Projects module services.
/// </summary>
public static class ProjectsModuleExtensions
{
    /// <summary>
    /// Adds the Projects module to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddProjectsModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register DbContext
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<ProjectsDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Register repositories
        services.AddScoped<IProjectRepository, ProjectRepository>();

        // Register services
        services.AddScoped<ProjectService>();

        // Register seeders
        services.AddScoped<ISeeder, ProjectSeeder>();
        services.AddScoped<ISeeder, ProjectMemberSeeder>();

        return services;
    }

    /// <summary>
    /// Maps the Projects module endpoints.
    /// </summary>
    /// <param name="app">The web application.</param>
    /// <returns>The web application.</returns>
    public static WebApplication MapProjectsEndpoints(this WebApplication app)
    {
        // Endpoints are automatically mapped via controller discovery
        return app;
    }
}
