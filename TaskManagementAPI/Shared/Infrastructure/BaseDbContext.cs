using TaskManagementAPI.Shared.Domain;

namespace TaskManagementAPI.Shared.Infrastructure;

/// <summary>
/// Abstract base class for all module-specific DbContexts.
/// Provides common configuration for soft delete query filters and entity tracking.
/// </summary>
public abstract class BaseDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the BaseDbContext class.
    /// </summary>
    /// <param name="options">The DbContext options.</param>
    protected BaseDbContext(DbContextOptions options) : base(options)
    {
    }

    /// <summary>
    /// Configures the model for the context.
    /// Applies soft delete query filters to all entities inheriting from BaseEntity.
    /// </summary>
    /// <param name="modelBuilder">The model builder used to construct the model for this context.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply soft delete query filter to all entities inheriting from BaseEntity
        var entityTypes = modelBuilder.Model.GetEntityTypes()
            .Where(e => typeof(BaseEntity).IsAssignableFrom(e.ClrType));

        foreach (var entityType in entityTypes)
        {
            // Create a parameter for the entity type
            var parameter = System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "e");

            // Create the expression: e => !e.IsDeleted
            var isDeletedProperty = System.Linq.Expressions.Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
            var notDeleted = System.Linq.Expressions.Expression.Not(isDeletedProperty);
            var lambda = System.Linq.Expressions.Expression.Lambda(notDeleted, parameter);

            // Apply the query filter
            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
        }
    }

    /// <summary>
    /// Saves all changes made in this context to the database.
    /// Updates the UpdatedAt timestamp for modified entities.
    /// </summary>
    /// <param name="cancellationToken">A CancellationToken to observe while waiting for the task to complete.</param>
    /// <returns>The number of state entries written to the database.</returns>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return await base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Saves all changes made in this context to the database synchronously.
    /// Updates the UpdatedAt timestamp for modified entities.
    /// </summary>
    /// <returns>The number of state entries written to the database.</returns>
    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    /// <summary>
    /// Updates the UpdatedAt timestamp for all modified entities.
    /// Sets DeletedAt when an entity is soft-deleted.
    /// </summary>
    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries<BaseEntity>();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }

            if (entry.State == EntityState.Modified && entry.Entity.IsDeleted && entry.Entity.DeletedAt == null)
            {
                entry.Entity.DeletedAt = DateTime.UtcNow;
            }
        }
    }
}
