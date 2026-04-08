using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManagementAPI.Modules.Projects.Domain.Entities;

namespace TaskManagementAPI.Modules.Projects.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity configuration for the ProjectMember entity using Fluent API.
/// </summary>
public class ProjectMemberConfiguration : IEntityTypeConfiguration<ProjectMember>
{
    /// <summary>
    /// Configures the ProjectMember entity.
    /// </summary>
    /// <param name="builder">The entity type builder.</param>
    public void Configure(EntityTypeBuilder<ProjectMember> builder)
    {
        // Primary key
        builder.HasKey(pm => pm.Id);

        // Properties
        builder.Property(pm => pm.ProjectId)
            .IsRequired();

        builder.Property(pm => pm.UserId)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(pm => pm.Role)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(pm => pm.JoinedAt)
            .IsRequired();

        builder.Property(pm => pm.CreatedAt)
            .IsRequired();

        builder.Property(pm => pm.UpdatedAt);

        builder.Property(pm => pm.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(pm => pm.DeletedAt);

        // Relationships
        builder.HasOne(pm => pm.Project)
            .WithMany(p => p.Members)
            .HasForeignKey(pm => pm.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(pm => pm.ProjectId);
        builder.HasIndex(pm => pm.UserId);
        builder.HasIndex(pm => new { pm.ProjectId, pm.UserId }).IsUnique();
        builder.HasIndex(pm => pm.IsDeleted);
        
        // Composite index for performance optimization
        builder.HasIndex(pm => new { pm.ProjectId, pm.IsDeleted })
            .HasDatabaseName("IX_ProjectMember_ProjectId_IsDeleted");

        // Query filter for soft delete
        builder.HasQueryFilter(pm => !pm.IsDeleted);

        // Table name
        builder.ToTable("ProjectMembers");
    }
}
