using Microsoft.EntityFrameworkCore;
using TaskManagementAPI.Modules.Tasks.Domain.Entities;
using TaskManagementAPI.Modules.Tasks.Infrastructure.Persistence.Configurations;
using TaskManagementAPI.Shared.Application.Services;
using TaskManagementAPI.Shared.Infrastructure;

namespace TaskManagementAPI.Modules.Tasks.Infrastructure.Persistence;

/// <summary>
/// DbContext for the Tasks module.
/// Inherits soft-delete filters, audit timestamps, and actor tracking from BaseDbContext.
/// </summary>
public class TasksDbContext : BaseDbContext
{
    public TasksDbContext(
        DbContextOptions<TasksDbContext> options,
        ICurrentUserService? currentUser = null)
        : base(options, currentUser)
    {
    }

    public DbSet<WorkTask> Tasks { get; set; } = null!;
    public DbSet<TaskDependency> TaskDependencies { get; set; } = null!;
    public DbSet<TimeTrackingEntry> TimeTrackingEntries { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new TaskConfiguration());
        modelBuilder.ApplyConfiguration(new TaskDependencyConfiguration());
        modelBuilder.ApplyConfiguration(new TimeTrackingEntryConfiguration());
    }
}
