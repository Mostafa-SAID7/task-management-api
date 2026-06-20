using Microsoft.EntityFrameworkCore;
using TaskManagementAPI.Modules.Projects.Domain.Entities;
using TaskManagementAPI.Modules.Projects.Infrastructure.Persistence.Configurations;
using TaskManagementAPI.Shared.Application.Services;
using TaskManagementAPI.Shared.Infrastructure;

namespace TaskManagementAPI.Modules.Projects.Infrastructure.Persistence;

/// <summary>
/// DbContext for the Projects module.
/// Inherits soft-delete filters, audit timestamps, and actor tracking from BaseDbContext.
/// </summary>
public class ProjectsDbContext : BaseDbContext
{
    public ProjectsDbContext(
        DbContextOptions<ProjectsDbContext> options,
        ICurrentUserService? currentUser = null)
        : base(options, currentUser)
    {
    }

    public DbSet<Project> Projects { get; set; } = null!;
    public DbSet<ProjectMember> ProjectMembers { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new ProjectConfiguration());
        modelBuilder.ApplyConfiguration(new ProjectMemberConfiguration());
    }
}
