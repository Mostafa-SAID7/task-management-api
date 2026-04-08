using TaskManagementAPI.Modules.Tasks.Domain.Entities;
using TaskManagementAPI.Modules.Tasks.Domain.Enums;
using TaskManagementAPI.Modules.Tasks.Infrastructure.Services;
using TaskManagementAPI.Shared.Domain.Interfaces;
using TaskManagementAPI.Shared.Domain.Services;

namespace TaskManagementAPI.Modules.Tasks.Application.Services;

/// <summary>
/// Service for managing tasks and task-related operations.
/// Handles business logic for task creation, updates, deletion, queries, and SEO-friendly slug generation.
/// </summary>
public class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepository;
    private readonly INotificationService _notificationService;

    /// <summary>
    /// Initializes a new instance of the TaskService class.
    /// </summary>
    /// <param name="taskRepository">The task repository.</param>
    /// <param name="notificationService">The notification service.</param>
    public TaskService(ITaskRepository taskRepository, INotificationService notificationService)
    {
        _taskRepository = taskRepository;
        _notificationService = notificationService;
    }

    /// <summary>
    /// Creates a new task.
    /// </summary>
    /// <param name="projectId">The project ID.</param>
    /// <param name="title">The task title.</param>
    /// <param name="description">The task description.</param>
    /// <param name="priority">The task priority.</param>
    /// <param name="dueDate">The task due date.</param>
    /// <returns>The created task.</returns>
    /// <exception cref="ArgumentException">Thrown when due date is in the past.</exception>
    public async System.Threading.Tasks.Task<WorkTask> CreateTaskAsync(
        Guid projectId, string title, string? description = null,
        TaskPriority priority = TaskPriority.Medium, DateTime? dueDate = null)
    {
        // Validate due date is not in the past
        if (dueDate.HasValue && dueDate.Value < DateTime.UtcNow)
        {
            throw new ArgumentException("Due date cannot be in the past.");
        }

        // Generate slug for SEO (optional - may fail if database doesn't support it yet)
        string? slug = null;
        try
        {
            var baseSlug = SlugService.GenerateSlug(title);
            var existingSlugs = await _taskRepository.GetProjectTaskSlugsAsync(projectId);
            slug = SlugService.GenerateUniqueSlug(baseSlug, existingSlugs ?? Enumerable.Empty<string>());
        }
        catch
        {
            // Slug generation failed - continue without it
        }

        var task = new WorkTask
        {
            ProjectId = projectId,
            Title = title,
            Slug = slug,
            Description = description,
            Priority = priority,
            DueDate = dueDate,
            Status = TaskManagementAPI.Modules.Tasks.Domain.Enums.TaskStatus.New
        };

        await _taskRepository.AddAsync(task);
        await _notificationService.BroadcastAsync($"project-{projectId}", $"New task created: {task.Title}");

        return task;
    }

    /// <summary>
    /// Updates an existing task and regenerates slug if title changes.
    /// </summary>
    /// <param name="taskId">The task ID.</param>
    /// <param name="title">The new title.</param>
    /// <param name="description">The new description.</param>
    /// <param name="priority">The new priority.</param>
    /// <param name="dueDate">The new due date.</param>
    /// <returns>The updated task, or null if not found.</returns>
    public async System.Threading.Tasks.Task<WorkTask?> UpdateTaskAsync(
        Guid taskId, string title, string? description = null,
        TaskPriority? priority = null, DateTime? dueDate = null)
    {
        var task = await _taskRepository.GetByIdAsync(taskId);
        if (task == null)
            return null;

        // Validate due date is not in the past
        if (dueDate.HasValue && dueDate.Value < DateTime.UtcNow)
            throw new ArgumentException("Due date cannot be in the past.");

        // Regenerate slug if title changed
        if (task.Title != title)
        {
            var baseSlug = SlugService.GenerateSlug(title);
            var existingSlugs = (await _taskRepository.GetProjectTaskSlugsAsync(task.ProjectId))
                .Where(s => s != task.Slug) // Exclude current slug
                .ToList();
            task.Slug = SlugService.GenerateUniqueSlug(baseSlug, existingSlugs);
        }

        task.Title = title;
        task.Description = description;
        if (priority.HasValue)
            task.Priority = priority.Value;
        task.DueDate = dueDate;
        task.UpdatedAt = DateTime.UtcNow;

        await _taskRepository.UpdateAsync(task);
        await _notificationService.BroadcastAsync($"project-{task.ProjectId}", $"Task updated: {task.Title}");

        return task;
    }

    /// <summary>
    /// Soft-deletes a task.
    /// </summary>
    /// <param name="taskId">The task ID.</param>
    /// <returns>True if the task was deleted; otherwise false.</returns>
    public async System.Threading.Tasks.Task<bool> DeleteTaskAsync(Guid taskId)
    {
        var task = await _taskRepository.GetByIdAsync(taskId);
        if (task == null)
            return false;

        await _taskRepository.DeleteAsync(taskId);
        await _notificationService.BroadcastAsync($"project-{task.ProjectId}", $"Task deleted: {task.Title}");

        return true;
    }

    /// <summary>
    /// Updates the status of a task.
    /// </summary>
    /// <param name="taskId">The task ID.</param>
    /// <param name="status">The new status.</param>
    /// <returns>The updated task, or null if not found.</returns>
    public async System.Threading.Tasks.Task<WorkTask?> UpdateTaskStatusAsync(Guid taskId, TaskManagementAPI.Modules.Tasks.Domain.Enums.TaskStatus status)
    {
        var task = await _taskRepository.GetByIdAsync(taskId);
        if (task == null)
            return null;

        // Validate that blocking tasks are complete before marking as complete
        if (status == TaskManagementAPI.Modules.Tasks.Domain.Enums.TaskStatus.Completed)
        {
            var hasIncompleteBlockers = await _taskRepository.HasIncompleteBlockingTasksAsync(taskId);
            if (hasIncompleteBlockers)
                throw new InvalidOperationException("Cannot complete task while blocking tasks are incomplete.");
        }

        task.Status = status;
        task.UpdatedAt = DateTime.UtcNow;

        await _taskRepository.UpdateAsync(task);
        await _notificationService.BroadcastAsync($"project-{task.ProjectId}", $"Task status updated: {task.Title} -> {status}");

        return task;
    }

    /// <summary>
    /// Assigns a task to a user.
    /// </summary>
    /// <param name="taskId">The task ID.</param>
    /// <param name="assigneeId">The assignee ID.</param>
    /// <returns>The updated task, or null if not found.</returns>
    public async System.Threading.Tasks.Task<WorkTask?> AssignTaskAsync(Guid taskId, string assigneeId)
    {
        var task = await _taskRepository.GetByIdAsync(taskId);
        if (task == null)
            return null;

        task.AssigneeId = assigneeId;
        task.UpdatedAt = DateTime.UtcNow;

        await _taskRepository.UpdateAsync(task);

        // Parallelize broadcast notifications to improve performance
        var broadcastTasks = new List<System.Threading.Tasks.Task>
        {
            _notificationService.BroadcastAsync($"project-{task.ProjectId}", $"Task assigned: {task.Title} -> {assigneeId}"),
            _notificationService.BroadcastAsync($"user-{assigneeId}", $"You have been assigned to task: {task.Title}")
        };

        // Notify project manager if critical priority
        if (task.Priority == TaskPriority.Critical)
        {
            broadcastTasks.Add(_notificationService.BroadcastAsync($"project-{task.ProjectId}", $"CRITICAL: Task assigned: {task.Title}"));
        }

        await System.Threading.Tasks.Task.WhenAll(broadcastTasks);

        return task;
    }

    /// <summary>
    /// Adds a time tracking entry to a task.
    /// </summary>
    /// <param name="taskId">The task ID.</param>
    /// <param name="userId">The user ID.</param>
    /// <param name="hours">The number of hours.</param>
    /// <param name="date">The date of the entry.</param>
    /// <returns>The created time tracking entry.</returns>
    public async System.Threading.Tasks.Task<TimeTrackingEntry> AddTimeTrackingEntryAsync(
        Guid taskId, string userId, decimal hours, DateTime date)
    {
        var task = await _taskRepository.GetByIdAsync(taskId);
        if (task == null)
            throw new InvalidOperationException($"Task with ID {taskId} not found.");

        if (task.Status != TaskManagementAPI.Modules.Tasks.Domain.Enums.TaskStatus.InProgress)
            throw new InvalidOperationException("Time can only be tracked for tasks in progress.");

        var entry = new TimeTrackingEntry
        {
            TaskId = taskId,
            UserId = userId,
            Hours = hours,
            Date = date
        };

        task.TimeTrackingEntries.Add(entry);
        await _taskRepository.UpdateAsync(task);

        return entry;
    }

    /// <summary>
    /// Adds a dependency between two tasks.
    /// </summary>
    /// <param name="taskId">The task that is blocked.</param>
    /// <param name="blockedByTaskId">The task that is blocking.</param>
    /// <returns>The created task dependency.</returns>
    public async System.Threading.Tasks.Task<TaskDependency> AddTaskDependencyAsync(Guid taskId, Guid blockedByTaskId)
    {
        if (taskId == blockedByTaskId)
            throw new ArgumentException("A task cannot depend on itself.");

        var task = await _taskRepository.GetByIdAsync(taskId);
        if (task == null)
            throw new InvalidOperationException($"Task with ID {taskId} not found.");

        var blockingTask = await _taskRepository.GetByIdAsync(blockedByTaskId);
        if (blockingTask == null)
            throw new InvalidOperationException($"Task with ID {blockedByTaskId} not found.");

        var dependency = new TaskDependency
        {
            TaskId = taskId,
            BlockedByTaskId = blockedByTaskId
        };

        task.BlockedByDependencies.Add(dependency);
        await _taskRepository.UpdateAsync(task);

        return dependency;
    }

    /// <summary>
    /// Gets tasks for a project with optional filters.
    /// </summary>
    /// <param name="projectId">The project ID.</param>
    /// <param name="status">Optional status filter.</param>
    /// <param name="priority">Optional priority filter.</param>
    /// <param name="assigneeId">Optional assignee filter.</param>
    /// <param name="pageNumber">The page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <returns>A tuple containing the tasks and total count.</returns>
    public async System.Threading.Tasks.Task<(List<WorkTask> tasks, int totalCount)> GetProjectTasksAsync(
        Guid projectId, TaskManagementAPI.Modules.Tasks.Domain.Enums.TaskStatus? status = null, TaskPriority? priority = null,
        string? assigneeId = null, int pageNumber = 1, int pageSize = 20)
    {
        var (tasks, totalCount) = await _taskRepository.GetProjectTasksWithFiltersAsync(
            projectId, status, priority, assigneeId, pageNumber, pageSize);
        return (tasks.ToList(), totalCount);
    }

    /// <summary>
    /// Gets tasks assigned to a user.
    /// </summary>
    /// <param name="assigneeId">The assignee ID.</param>
    /// <param name="pageNumber">The page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <returns>A tuple containing the tasks and total count.</returns>
    public async System.Threading.Tasks.Task<(IEnumerable<WorkTask> Tasks, int TotalCount)> GetUserTasksAsync(
        string assigneeId, int pageNumber = 1, int pageSize = 20)
    {
        return await _taskRepository.GetUserTasksAsync(assigneeId, pageNumber, pageSize);
    }

    /// <summary>
    /// Gets a task by ID.
    /// </summary>
    /// <param name="taskId">The task ID.</param>
    /// <returns>The task, or null if not found.</returns>
    public async System.Threading.Tasks.Task<WorkTask?> GetTaskByIdAsync(Guid taskId)
    {
        return await _taskRepository.GetByIdAsync(taskId);
    }

    /// <summary>
    /// Gets a task by its SEO-friendly slug within a project.
    /// </summary>
    /// <param name="projectId">The project ID.</param>
    /// <param name="slug">The task slug.</param>
    /// <returns>The task, or null if not found.</returns>
    public async System.Threading.Tasks.Task<WorkTask?> GetTaskBySlugAsync(Guid projectId, string slug)
    {
        return await _taskRepository.GetBySlugAsync(projectId, slug);
    }
}
