using System.Linq.Expressions;

namespace TaskManagementAPI.Shared.Domain.Interfaces;

/// <summary>
/// Generic repository interface defining the full data-access contract for BaseEntity descendants.
/// Read methods use AsNoTracking by default for optimal performance.
/// Write methods (UpdateAsync, DeleteAsync) persist immediately via SaveChangesAsync.
/// AddAsync stages the insert — callers must invoke SaveChangesAsync or use IUnitOfWork.
/// </summary>
/// <typeparam name="T">The entity type; must inherit from BaseEntity.</typeparam>
public interface IRepository<T> where T : BaseEntity
{
    // ─── Reads (AsNoTracking, no change-tracking overhead) ──────────────────

    /// <summary>
    /// Returns the entity with the given ID, or null if not found.
    /// Soft-deleted records are automatically excluded.
    /// </summary>
    Task<T?> GetByIdAsync(Guid id);

    /// <summary>
    /// Returns all non-deleted entities. For large tables prefer GetPagedAsync.
    /// </summary>
    Task<IEnumerable<T>> GetAllAsync();

    /// <summary>
    /// Returns all entities matching the predicate.
    /// Soft-deleted records are excluded unless the predicate explicitly filters them.
    /// </summary>
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);

    /// <summary>
    /// Returns a paginated slice of all entities ordered by CreatedAt descending.
    /// Suitable for large tables — avoids loading everything into memory.
    /// </summary>
    /// <param name="pageNumber">1-based page number.</param>
    /// <param name="pageSize">Records per page (capped at 100 internally).</param>
    Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize);

    /// <summary>
    /// Returns true if any non-deleted entity with the given ID exists.
    /// </summary>
    Task<bool> ExistsAsync(Guid id);

    /// <summary>
    /// Counts all non-deleted entities matching the optional predicate.
    /// When predicate is null, counts all non-deleted entities in the set.
    /// </summary>
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);

    // ─── Writes ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Stages a new entity for insertion.
    /// The caller is responsible for calling SaveChangesAsync (or IUnitOfWork.SaveChangesAsync).
    /// </summary>
    Task<T> AddAsync(T entity);

    /// <summary>
    /// Persists changes to an existing entity immediately.
    /// UpdatedAt and UpdatedBy are set automatically by BaseDbContext.
    /// </summary>
    Task UpdateAsync(T entity);

    /// <summary>
    /// Soft-deletes the entity with the given ID and persists immediately.
    /// Sets IsDeleted = true, DeletedAt = UtcNow, and DeletedBy from the current user.
    /// The entity is excluded from all future standard queries.
    /// </summary>
    Task DeleteAsync(Guid id);
}
