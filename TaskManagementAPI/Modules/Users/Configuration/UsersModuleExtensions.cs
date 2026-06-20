using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskManagementAPI.Modules.Users.Application.Services;
using TaskManagementAPI.Modules.Users.Domain.Entities;
using TaskManagementAPI.Modules.Users.Infrastructure.Persistence;
using TaskManagementAPI.Modules.Users.Infrastructure.Services;

namespace TaskManagementAPI.Modules.Users.Configuration;

/// <summary>
/// Extension methods for registering the Users module services.
/// </summary>
public static class UsersModuleExtensions
{
    /// <summary>
    /// Adds the Users module services to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddUsersModule(this IServiceCollection services, IConfiguration configuration)
    {
        // Add DbContext
        services.AddDbContext<UsersDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        // Add Identity
        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.Password.RequiredLength = 8;
            options.Password.RequireDigit = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
        })
        .AddEntityFrameworkStores<UsersDbContext>()
        .AddDefaultTokenProviders();

        // Add services
        services.AddScoped<UserService>();
        services.AddScoped<IUserRepository, UserRepository>();

        return services;
    }

    /// <summary>
    /// Maps the Users module endpoints.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The application builder.</returns>
    public static IApplicationBuilder MapUsersEndpoints(this IApplicationBuilder app)
    {
        // Endpoints are mapped via controller routing
        return app;
    }
}
