using Microsoft.EntityFrameworkCore;
using TaskManagementAPI.Shared.Domain;
using TaskManagementAPI.Shared.Domain.Interfaces;

namespace TaskManagementAPI.Shared.Infrastructure.Repositories;

/// <summary>
/// Generic repository implementation providing CRUD operations for entities.
/// Automatically handles soft delete operations and query filtering.
/// </summary>
/// <typeparam name="T">The entity type, must inherit from BaseEntity.</typeparam>
public class GenericRepository<T> : IRepository<T> where T : BaseEntity
{
    /// <summary>
    /// The DbContext instance used for database operations.
    /// </summary>
    protected readonly DbContext _context;

    /// <summary>
    /// Initializes a new instance of the GenericRepository class.
    /// </summary>
    /// <param name="context">The DbContext instance.</param>
    public GenericRepository(DbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Retrieves an entity by its ID.
    /// Automatically excludes soft-deleted entities.
    /// </summary>
    /// <param name="id">The entity ID.</param>
    /// <returns>The entity if found; otherwise null.</returns>
    public virtual async Task<T?> GetByIdAsync(Guid id)
    {
        return await _context.Set<T>().FirstOrDefaultAsync(e => e.Id == id);
    }

    /// <summary>
    /// Retrieves all entities.
    /// Automatically excludes soft-deleted entities.
    /// </summary>
    /// <returns>A collection of all entities.</returns>
    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _context.Set<T>().ToListAsync();
    }

    /// <summary>
    /// Adds a new entity to the repository.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    /// <returns>The added entity.</returns>
    /// <exception cref="ArgumentNullException">Thrown when entity is null.</exception>
    public virtual async Task<T> AddAsync(T entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        await _context.Set<T>().AddAsync(entity);
        return entity;
    }

    /// <summary>
    /// Updates an existing entity.
    /// The UpdatedAt timestamp is automatically set by the DbContext.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when entity is null.</exception>
    public virtual async Task UpdateAsync(T entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        _context.Set<T>().Update(entity);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Soft-deletes an entity by ID.
    /// Sets IsDeleted = true and DeletedAt = current UTC timestamp.
    /// The entity is automatically excluded from future queries.
    /// </summary>
    /// <param name="id">The ID of the entity to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public virtual async Task DeleteAsync(Guid id)
    {
        var entity = await _context.Set<T>().FirstOrDefaultAsync(e => e.Id == id);
        if (entity != null)
        {
            entity.IsDeleted = true;
            entity.DeletedAt = DateTime.UtcNow;
            _context.Set<T>().Update(entity);
        }
    }
}
