namespace TaskManagementAPI.Shared.Domain;

/// <summary>
/// Abstract base class for all domain entities.
/// Provides GUID identity, full UTC audit timestamps, actor tracking,
/// and soft-delete support as cross-cutting concerns.
/// </summary>
public abstract class BaseEntity
{
    // ─── Identity ────────────────────────────────────────────────────────────

    /// <summary>
    /// Unique identifier (auto-generated GUID v4 on construction).
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    // ─── Audit timestamps ────────────────────────────────────────────────────

    /// <summary>
    /// UTC timestamp when the entity was first persisted.
    /// Set automatically by BaseDbContext on insert.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// UTC timestamp of the most recent modification.
    /// Null until the entity is updated at least once.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    // ─── Audit actors ─────────────────────────────────────────────────────────

    /// <summary>
    /// User ID who created this entity.
    /// Null for system-generated records or unauthenticated requests.
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// User ID who last modified this entity.
    /// Null until the entity has been updated at least once.
    /// </summary>
    public string? UpdatedBy { get; set; }

    // ─── Soft delete ─────────────────────────────────────────────────────────

    /// <summary>
    /// Marks the entity as logically deleted.
    /// Soft-deleted entities are excluded from all standard queries
    /// via a global EF Core query filter applied in BaseDbContext.
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// UTC timestamp when the entity was soft-deleted.
    /// Null until IsDeleted is set to true.
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// User ID who performed the soft-delete.
    /// Null until the entity is deleted.
    /// </summary>
    public string? DeletedBy { get; set; }
}
