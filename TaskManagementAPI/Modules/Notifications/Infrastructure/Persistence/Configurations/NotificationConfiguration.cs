using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManagementAPI.Modules.Notifications.Domain.Entities;

namespace TaskManagementAPI.Modules.Notifications.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity configuration for the Notification entity.
/// </summary>
public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    /// <summary>
    /// Configures the Notification entity.
    /// </summary>
    /// <param name="builder">The entity type builder.</param>
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.HasKey(n => n.Id);

        builder.Property(n => n.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(n => n.Message)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(n => n.Type)
            .IsRequired();

        builder.Property(n => n.Status)
            .IsRequired();

        builder.Property(n => n.IsRead)
            .HasDefaultValue(false);

        builder.Property(n => n.CreatedAt)
            .HasDefaultValueSql("NOW()");

        // Soft delete filter
        builder.HasQueryFilter(n => !n.IsDeleted);

        // Indexes
        builder.HasIndex(n => n.UserId);
        builder.HasIndex(n => n.CreatedAt);
        builder.HasIndex(n => n.IsRead);
        builder.HasIndex(n => n.Status);
        
        // Composite indexes for performance optimization
        builder.HasIndex(n => new { n.UserId, n.IsRead })
            .HasDatabaseName("IX_Notification_UserId_IsRead");
        builder.HasIndex(n => new { n.UserId, n.CreatedAt })
            .HasDatabaseName("IX_Notification_UserId_CreatedAt");
    }
}
