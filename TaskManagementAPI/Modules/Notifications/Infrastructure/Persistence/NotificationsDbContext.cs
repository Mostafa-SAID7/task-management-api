using Microsoft.EntityFrameworkCore;
using TaskManagementAPI.Modules.Notifications.Domain.Entities;
using TaskManagementAPI.Modules.Notifications.Infrastructure.Persistence.Configurations;
using TaskManagementAPI.Shared.Application.Services;
using TaskManagementAPI.Shared.Infrastructure;

namespace TaskManagementAPI.Modules.Notifications.Infrastructure.Persistence;

/// <summary>
/// DbContext for the Notifications module.
/// Inherits soft-delete filters, audit timestamps, and actor tracking from BaseDbContext.
/// </summary>
public class NotificationsDbContext : BaseDbContext
{
    public NotificationsDbContext(
        DbContextOptions<NotificationsDbContext> options,
        ICurrentUserService? currentUser = null)
        : base(options, currentUser)
    {
    }

    public DbSet<Notification> Notifications { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new NotificationConfiguration());
    }
}
