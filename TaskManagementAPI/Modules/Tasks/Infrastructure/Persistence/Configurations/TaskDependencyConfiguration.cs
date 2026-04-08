using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManagementAPI.Modules.Tasks.Domain.Entities;

namespace TaskManagementAPI.Modules.Tasks.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity configuration for the TaskDependency entity.
/// </summary>
public class TaskDependencyConfiguration : IEntityTypeConfiguration<TaskDependency>
{
    /// <summary>
    /// Configures the TaskDependency entity.
    /// </summary>
    /// <param name="builder">The entity type builder.</param>
    public void Configure(EntityTypeBuilder<TaskDependency> builder)
    {
        builder.HasKey(d => d.Id);

        builder.Property(d => d.TaskId)
            .IsRequired();

        builder.Property(d => d.BlockedByTaskId)
            .IsRequired();

        // Indexes for common queries
        builder.HasIndex(d => d.TaskId);
        builder.HasIndex(d => d.BlockedByTaskId);
        
        // Composite index for performance optimization
        builder.HasIndex(d => new { d.TaskId, d.BlockedByTaskId })
            .HasDatabaseName("IX_TaskDependency_TaskId_BlockedByTaskId");

        // Relationships
        builder.HasOne(d => d.Task)
            .WithMany(t => t.BlockedByDependencies)
            .HasForeignKey(d => d.TaskId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(d => d.BlockedByTask)
            .WithMany(t => t.BlockingDependencies)
            .HasForeignKey(d => d.BlockedByTaskId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
