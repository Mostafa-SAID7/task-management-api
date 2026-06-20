using System.Linq.Expressions;
using TaskManagementAPI.Shared.Application.Services;
using TaskManagementAPI.Shared.Domain;

namespace TaskManagementAPI.Shared.Infrastructure;

/// <summary>
/// Abstract base DbContext shared by all module DbContexts.
///
/// Responsibilities:
///  1. Global soft-delete query filter — every BaseEntity-derived set
///     automatically excludes IsDeleted = true records.
///  2. Automatic timestamp management — CreatedAt on insert,
///     UpdatedAt on every update, DeletedAt on soft-delete.
///  3. Actor tracking — CreatedBy / UpdatedBy / DeletedBy resolved
///     from ICurrentUserService and stamped on every save.
///  4. Structured audit events — every CUD operation emits a
///     Serilog "Audit.*" structured log event for the audit file sink.
/// </summary>
public abstract class BaseDbContext : DbContext
{
    private readonly ICurrentUserService? _currentUser;

    /// <summary>
    /// Constructor used when ICurrentUserService is available via DI
    /// (standard runtime path — full audit actor support).
    /// </summary>
    protected BaseDbContext(DbContextOptions options, ICurrentUserService? currentUser = null)
        : base(options)
    {
        _currentUser = currentUser;
    }

    // ─── Model configuration ─────────────────────────────────────────────────

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply soft-delete global query filter to every BaseEntity descendant
        // using a compiled lambda: e => !e.IsDeleted
        foreach (var entityType in modelBuilder.Model.GetEntityTypes()
            .Where(e => typeof(BaseEntity).IsAssignableFrom(e.ClrType)))
        {
            var param = Expression.Parameter(entityType.ClrType, "e");
            var isDeletedProp = Expression.Property(param, nameof(BaseEntity.IsDeleted));
            var filter = Expression.Lambda(Expression.Not(isDeletedProp), param);
            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
        }
    }

    // ─── SaveChanges overrides ────────────────────────────────────────────────

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var auditEntries = StampEntities();
        var result = await base.SaveChangesAsync(cancellationToken);
        EmitAuditEvents(auditEntries);
        return result;
    }

    public override int SaveChanges()
    {
        var auditEntries = StampEntities();
        var result = base.SaveChanges();
        EmitAuditEvents(auditEntries);
        return result;
    }

    // ─── Private helpers ──────────────────────────────────────────────────────

    private record AuditSnapshot(
        string Action,
        string EntityType,
        Guid   EntityId,
        string ActorId);

    /// <summary>
    /// Stamps all tracked BaseEntity entries with timestamps and actor IDs,
    /// then returns lightweight audit snapshots for post-save event emission.
    /// </summary>
    private List<AuditSnapshot> StampEntities()
    {
        var now     = DateTime.UtcNow;
        var actorId = _currentUser?.UserId ?? "system";
        var snapshots = new List<AuditSnapshot>();

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    if (entry.Entity.CreatedBy is null)
                        entry.Entity.CreatedBy = actorId;
                    snapshots.Add(new AuditSnapshot("Created",
                        entry.Entity.GetType().Name, entry.Entity.Id, actorId));
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    entry.Entity.UpdatedBy = actorId;

                    // Stamp soft-delete on the first pass that sets IsDeleted = true
                    if (entry.Entity.IsDeleted && entry.Entity.DeletedAt is null)
                    {
                        entry.Entity.DeletedAt = now;
                        entry.Entity.DeletedBy = actorId;
                        snapshots.Add(new AuditSnapshot("Deleted",
                            entry.Entity.GetType().Name, entry.Entity.Id, actorId));
                    }
                    else
                    {
                        snapshots.Add(new AuditSnapshot("Updated",
                            entry.Entity.GetType().Name, entry.Entity.Id, actorId));
                    }
                    break;
            }
        }

        return snapshots;
    }

    /// <summary>
    /// Emits one structured Serilog event per changed entity.
    /// The Serilog "Audit" sink filter captures these into a separate audit log file.
    /// </summary>
    private static void EmitAuditEvents(List<AuditSnapshot> snapshots)
    {
        foreach (var s in snapshots)
        {
            Log.ForContext("IsAuditEvent", true)
               .Information(
                   "AUDIT {AuditAction} {EntityType} {EntityId} by {ActorId}",
                   s.Action, s.EntityType, s.EntityId, s.ActorId);
        }
    }
}
