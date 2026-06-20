using System.Linq.Expressions;
using TaskManagementAPI.Shared.Application.Services;
using TaskManagementAPI.Shared.Domain;
using TaskManagementAPI.Shared.Domain.Interfaces;

namespace TaskManagementAPI.Shared.Infrastructure.Repositories;

/// <summary>
/// Generic repository providing the full IRepository contract for any BaseEntity.
///
/// Performance decisions:
///  - All read paths use AsNoTracking() — no change-tracker overhead.
///  - GetPagedAsync uses OrderBy + Skip/Take; suitable for tables up to
///    several hundred thousand rows. Module-specific repos override this
///    with keyset/cursor pagination for ultra-large datasets.
///  - UpdateAsync and DeleteAsync call SaveChangesAsync immediately so
///    single-entity mutations are self-contained.
///  - AddAsync stages only — callers batch multiple adds then SaveChanges.
/// </summary>
/// <typeparam name="T">Domain entity inheriting from BaseEntity.</typeparam>
public class GenericRepository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly DbContext _context;
    private readonly ICurrentUserService? _currentUser;

    public GenericRepository(DbContext context, ICurrentUserService? currentUser = null)
    {
        _context     = context ?? throw new ArgumentNullException(nameof(context));
        _currentUser = currentUser;
    }

    // ─── Reads ───────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public virtual async Task<T?> GetByIdAsync(Guid id)
    {
        return await _context.Set<T>()
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    /// <inheritdoc/>
    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _context.Set<T>()
            .AsNoTracking()
            .ToListAsync();
    }

    /// <inheritdoc/>
    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return await _context.Set<T>()
            .AsNoTracking()
            .Where(predicate)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public virtual async Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(
        int pageNumber, int pageSize)
    {
        pageNumber = Math.Max(1, pageNumber);
        pageSize   = Math.Clamp(pageSize, 1, 100);

        var query = _context.Set<T>().AsNoTracking();

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(e => e.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    /// <inheritdoc/>
    public virtual async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Set<T>()
            .AsNoTracking()
            .AnyAsync(e => e.Id == id);
    }

    /// <inheritdoc/>
    public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
    {
        var query = _context.Set<T>().AsNoTracking();
        return predicate is null
            ? await query.CountAsync()
            : await query.CountAsync(predicate);
    }

    // ─── Writes ──────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public virtual async Task<T> AddAsync(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        await _context.Set<T>().AddAsync(entity);
        return entity;
    }

    /// <inheritdoc/>
    public virtual async Task UpdateAsync(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        _context.Set<T>().Update(entity);
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public virtual async Task DeleteAsync(Guid id)
    {
        // Fetch with tracking so the change is picked up by SaveChangesAsync
        var entity = await _context.Set<T>().FirstOrDefaultAsync(e => e.Id == id);
        if (entity is null) return;

        entity.IsDeleted = true;
        // DeletedAt and DeletedBy are stamped by BaseDbContext.StampEntities()
        // so we don't set them here — avoids duplication.
        _context.Set<T>().Update(entity);
        await _context.SaveChangesAsync();
    }
}
