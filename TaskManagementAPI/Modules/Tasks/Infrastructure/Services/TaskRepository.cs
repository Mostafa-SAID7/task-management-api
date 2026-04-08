using Microsoft.EntityFrameworkCore;
using TaskManagementAPI.Modules.Tasks.Domain.Entities;
using TaskManagementAPI.Modules.Tasks.Domain.Enums;
using TaskManagementAPI.Modules.Tasks.Domain.Specifications;
using TaskManagementAPI.Modules.Tasks.Infrastructure.Persistence;
using TaskManagementAPI.Shared.Infrastructure.Repositories;

namespace TaskManagementAPI.Modules.Tasks.Infrastructure.Services;

/// <summary>
/// Repository implementation for WorkTask entity operations.
/// </summary>
public class TaskRepository : GenericRepository<WorkTask>, ITaskRepository
{
    private new readonly TasksDbContext _context;

    /// <summary>
    /// Initializes a new instance of the TaskRepository class.
    /// </summary>
    /// <param name="context">The Tasks DbContext.</param>
    public TaskRepository(TasksDbContext context) : base(context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets tasks for a specific project with pagination using optimized queries.
    /// </summary>
    public async System.Threading.Tasks.Task<(IEnumerable<WorkTask> Tasks, int TotalCount)> GetProjectTasksPagedAsync(
        Guid projectId, int pageNumber, int pageSize)
    {
        var query = _context.Tasks
            .Where(t => t.ProjectId == projectId)
            .AsNoTracking();
        
        var totalCount = await query.CountAsync();

        var tasks = await TaskSpecifications.ApplyPagination(query, pageNumber, pageSize)
            .ToListAsync();

        return (tasks, totalCount);
    }

    /// <summary>
    /// Gets tasks for a specific project with optional filters using optimized queries.
    /// Applies eager loading before pagination to avoid N+1 queries.
    /// </summary>
    public async System.Threading.Tasks.Task<(IEnumerable<WorkTask> Tasks, int TotalCount)> GetProjectTasksWithFiltersAsync(
        Guid projectId, TaskManagementAPI.Modules.Tasks.Domain.Enums.TaskStatus? status = null, TaskPriority? priority = null,
        string? assigneeId = null, int pageNumber = 1, int pageSize = 20)
    {
        var query = TaskSpecifications.GetByProjectWithFilters(
            _context.Tasks, projectId, status, priority, assigneeId)
            .AsNoTracking();

        var totalCount = await query.CountAsync();

        var tasks = await TaskSpecifications.ApplyPagination(query, pageNumber, pageSize)
            .ToListAsync();

        return (tasks, totalCount);
    }

    /// <summary>
    /// Gets tasks assigned to a specific user using optimized queries.
    /// </summary>
    public async System.Threading.Tasks.Task<(IEnumerable<WorkTask> Tasks, int TotalCount)> GetUserTasksAsync(
        string assigneeId, int pageNumber = 1, int pageSize = 20)
    {
        var query = TaskSpecifications.GetByAssignee(_context.Tasks, assigneeId)
            .AsNoTracking();
        
        var totalCount = await query.CountAsync();

        var tasks = await TaskSpecifications.ApplyPagination(query, pageNumber, pageSize)
            .ToListAsync();

        return (tasks, totalCount);
    }

    /// <summary>
    /// Gets overdue tasks using optimized queries.
    /// </summary>
    public async System.Threading.Tasks.Task<IEnumerable<WorkTask>> GetOverdueTasksAsync()
    {
        return await TaskSpecifications.GetOverdue(_context.Tasks)
            .AsNoTracking()
            .ToListAsync();
    }

    /// <summary>
    /// Gets tasks due within the specified number of days using optimized queries.
    /// </summary>
    public async System.Threading.Tasks.Task<IEnumerable<WorkTask>> GetTasksDueSoonAsync(int days = 1)
    {
        return await TaskSpecifications.GetDueSoon(_context.Tasks, days)
            .AsNoTracking()
            .ToListAsync();
    }

    /// <summary>
    /// Gets tasks with their dependencies using eager loading.
    /// </summary>
    public async System.Threading.Tasks.Task<WorkTask?> GetTaskWithDependenciesAsync(Guid taskId)
    {
        return await _context.Tasks
            .Include(t => t.BlockedByDependencies.Where(d => !d.IsDeleted))
            .Include(t => t.BlockingDependencies.Where(d => !d.IsDeleted))
            .Include(t => t.TimeTrackingEntries)
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == taskId);
    }

    /// <summary>
    /// Gets all tasks blocking a specific task using optimized queries.
    /// Uses eager loading to avoid N+1 queries.
    /// </summary>
    public async System.Threading.Tasks.Task<IEnumerable<WorkTask>> GetBlockingTasksAsync(Guid taskId)
    {
        return await _context.TaskDependencies
            .Where(d => d.TaskId == taskId && !d.IsDeleted)
            .Include(d => d.BlockedByTask)
            .Select(d => d.BlockedByTask!)
            .AsNoTracking()
            .ToListAsync();
    }

    /// <summary>
    /// Checks if a task has any incomplete blocking tasks using optimized query.
    /// Uses projection to avoid loading entire entity.
    /// </summary>
    public async System.Threading.Tasks.Task<bool> HasIncompleteBlockingTasksAsync(Guid taskId)
    {
        return await _context.TaskDependencies
            .Where(d => d.TaskId == taskId && !d.IsDeleted)
            .AnyAsync(d => d.BlockedByTask != null && 
                          d.BlockedByTask.Status != TaskManagementAPI.Modules.Tasks.Domain.Enums.TaskStatus.Completed &&
                          d.BlockedByTask.Status != TaskManagementAPI.Modules.Tasks.Domain.Enums.TaskStatus.Cancelled);
    }

    /// <summary>
    /// Gets a task by its SEO-friendly slug within a project using optimized queries.
    /// Includes related entities for complete aggregate.
    /// </summary>
    public async System.Threading.Tasks.Task<WorkTask?> GetBySlugAsync(Guid projectId, string slug)
    {
        return await _context.Tasks
            .Where(t => t.ProjectId == projectId && t.Slug == slug)
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Gets all slugs for tasks in a specific project using optimized query.
    /// Uses projection to minimize memory usage.
    /// </summary>
    public async System.Threading.Tasks.Task<IEnumerable<string>> GetProjectTaskSlugsAsync(Guid projectId)
    {
        return await _context.Tasks
            .Where(t => t.ProjectId == projectId && !t.IsDeleted)
            .Select(t => t.Slug)
            .AsNoTracking()
            .ToListAsync();
    }
}
