using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TaskManagementAPI.Modules.Users.Domain.Entities;

namespace TaskManagementAPI.Modules.Users.Infrastructure.Persistence;

/// <summary>
/// DbContext for the Users module with ASP.NET Core Identity integration.
/// </summary>
public class UsersDbContext : IdentityDbContext<ApplicationUser>
{
    /// <summary>
    /// Initializes a new instance of the UsersDbContext class.
    /// </summary>
    /// <param name="options">The DbContext options.</param>
    public UsersDbContext(DbContextOptions<UsersDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Configures the model for the Users module.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure ApplicationUser
        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(u => u.FullName).HasMaxLength(255);
            entity.Property(u => u.CreatedAt).HasDefaultValueSql("NOW()");
            entity.HasQueryFilter(u => !u.IsDeleted);
            entity.HasIndex(u => u.Email).IsUnique();
            entity.HasIndex(u => u.CreatedAt);
        });
    }
}
