using Microsoft.Extensions.Logging;
using TaskManagementAPI.Modules.Projects.Infrastructure.Persistence;
using TaskManagementAPI.Modules.Tasks.Infrastructure.Persistence;
using TaskManagementAPI.Modules.Users.Infrastructure.Persistence;
using TaskManagementAPI.Modules.Notifications.Infrastructure.Persistence;

namespace TaskManagementAPI.Shared.Infrastructure.DependencyInjection;

/// <summary>
/// Extension methods for database initialization and migration.
/// </summary>
public static class DatabaseInitializationExtensions
{
    /// <summary>
    /// Initializes all database contexts and runs pending migrations.
    /// Handles DNS resolution failures gracefully (for IPv6-only networks).
    /// </summary>
    public static async Task InitializeDatabasesAsync(this WebApplication app)
    {
        try
        {
            using (var scope = app.Services.CreateScope())
            {
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                
                logger.LogInformation("🔄 Starting database initialization...");

                // Attempt to get DbContexts - wrapped in try/catch since DNS resolution failures
                // will occur here in IPv6-only networks when constructing the contexts
                ProjectsDbContext? projectsContext = null;
                TasksDbContext? tasksContext = null;
                UsersDbContext? usersContext = null;
                NotificationsDbContext? notificationsContext = null;

                try { projectsContext = scope.ServiceProvider.GetRequiredService<ProjectsDbContext>(); }
                catch (System.Net.Sockets.SocketException ex) when (ex.ErrorCode == 11004)
                {
                    logger.LogWarning("DNS resolution failed for ProjectsDbContext (error 11004) - app will continue");
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to create ProjectsDbContext");
                }

                try { tasksContext = scope.ServiceProvider.GetRequiredService<TasksDbContext>(); }
                catch (System.Net.Sockets.SocketException ex) when (ex.ErrorCode == 11004)
                {
                    logger.LogWarning("DNS resolution failed for TasksDbContext (error 11004) - app will continue");
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to create TasksDbContext");
                }

                try { usersContext = scope.ServiceProvider.GetRequiredService<UsersDbContext>(); }
                catch (System.Net.Sockets.SocketException ex) when (ex.ErrorCode == 11004)
                {
                    logger.LogWarning("DNS resolution failed for UsersDbContext (error 11004) - app will continue");
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to create UsersDbContext");
                }

                try { notificationsContext = scope.ServiceProvider.GetRequiredService<NotificationsDbContext>(); }
                catch (System.Net.Sockets.SocketException ex) when (ex.ErrorCode == 11004)
                {
                    logger.LogWarning("DNS resolution failed for NotificationsDbContext (error 11004) - app will continue");
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to create NotificationsDbContext");
                }

                // Migrate each context if available
                await MigrateContextAsync(projectsContext, logger, "ProjectsDbContext");
                await MigrateContextAsync(tasksContext, logger, "TasksDbContext");
                await MigrateContextAsync(usersContext, logger, "UsersDbContext");
                await MigrateContextAsync(notificationsContext, logger, "NotificationsDbContext");

                logger.LogInformation("✅ Database initialization completed");
            }
        }
        catch (Exception ex)
        {
            var logger = app.Services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "❌ An error occurred while initializing the database");
        }
    }

    private static async Task MigrateContextAsync<T>(T? context, Microsoft.Extensions.Logging.ILogger logger, string contextName) where T : class
    {
        if (context == null)
            return;

        try
        {
            var dbContext = context as Microsoft.EntityFrameworkCore.DbContext;
            if (dbContext?.Database.IsRelational() == true)
            {
                await dbContext.Database.MigrateAsync();
                logger.LogInformation("✅ {ContextName} migration completed", contextName);
            }
        }
        catch (System.Net.Sockets.SocketException ex) when (ex.ErrorCode == 11004)
        {
            // Suppress DNS errors - app continues in degraded mode
            logger.LogInformation("⚠️  DNS error during {ContextName} migration (error 11004) - continuing", contextName);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "⚠️  Error during {ContextName} migration", contextName);
        }
    }
}
