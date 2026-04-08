using TaskManagementAPI.Modules.Tasks.Application.DTOs;
using TaskManagementAPI.Modules.Tasks.Application.Services;
using TaskManagementAPI.Modules.Tasks.Domain.Enums;

namespace TaskManagementAPI.Modules.Tasks.Presentation.Controllers;

/// <summary>
/// API controller for task management operations.
/// </summary>
[ApiController]
[Route("api/tasks")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;

    /// <summary>
    /// Initializes a new instance of the TasksController class.
    /// </summary>
    /// <param name="taskService">The task service.</param>
    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    /// <summary>
    /// Creates a new task.
    /// </summary>
    /// <param name="request">The create task request.</param>
    /// <returns>The created task.</returns>
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TaskDto>> CreateTask([FromBody] CreateTaskRequest request)
    {
        try
        {
            var task = await _taskService.CreateTaskAsync(
                request.ProjectId, request.Title, request.Description, request.Priority, request.DueDate);

            var dto = MapToDto(task);
            return CreatedAtAction(nameof(GetTask), new { id = task.Id }, dto);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gets a task by ID.
    /// </summary>
    /// <param name="id">The task ID.</param>
    /// <returns>The task.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskDto>> GetTask(Guid id)
    {
        var task = await _taskService.GetTaskByIdAsync(id);
        if (task == null)
            return NotFound();

        return Ok(MapToDto(task));
    }

    /// <summary>
    /// Updates a task.
    /// </summary>
    /// <param name="id">The task ID.</param>
    /// <param name="request">The update task request.</param>
    /// <returns>The updated task.</returns>
    [HttpPut("{id}")]
    [Authorize]
    [ValidateAntiForgeryToken]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TaskDto>> UpdateTask(Guid id, [FromBody] UpdateTaskRequest request)
    {
        try
        {
            var task = await _taskService.UpdateTaskAsync(
                id, request.Title, request.Description, request.Priority, request.DueDate);

            if (task == null)
                return NotFound();

            return Ok(MapToDto(task));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Deletes a task.
    /// </summary>
    /// <param name="id">The task ID.</param>
    /// <returns>No content.</returns>
    [HttpDelete("{id}")]
    [Authorize]
    [ValidateAntiForgeryToken]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTask(Guid id)
    {
        var result = await _taskService.DeleteTaskAsync(id);
        if (!result)
            return NotFound();

        return NoContent();
    }

    /// <summary>
    /// Gets tasks for a project with optional filters and pagination.
    /// </summary>
    /// <param name="projectId">The project ID.</param>
    /// <param name="status">Optional status filter.</param>
    /// <param name="priority">Optional priority filter.</param>
    /// <param name="assigneeId">Optional assignee filter.</param>
    /// <param name="pageNumber">The page number (default 1).</param>
    /// <param name="pageSize">The page size (default 20).</param>
    /// <returns>A paginated list of tasks.</returns>
    [HttpGet("project/{projectId}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TaskListResponse>> GetProjectTasks(
        Guid projectId,
        [FromQuery] TaskManagementAPI.Modules.Tasks.Domain.Enums.TaskStatus? status = null,
        [FromQuery] TaskPriority? priority = null,
        [FromQuery] string? assigneeId = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            // Validate pagination parameters
            if (pageNumber < 1)
                pageNumber = 1;
            if (pageSize < 1)
                pageSize = 20;
            if (pageSize > 100)
                pageSize = 100;

            var (tasks, totalCount) = await _taskService.GetProjectTasksAsync(
                projectId, status, priority, assigneeId, pageNumber, pageSize);

            var response = new TaskListResponse
            {
                Data = tasks.Select(MapToDto).ToList(),
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return Ok(response);
        }
        catch
        {
            // Return empty list on error to allow graceful degradation
            return Ok(new TaskListResponse
            {
                Data = new List<TaskDto>(),
                TotalCount = 0,
                PageNumber = pageNumber,
                PageSize = pageSize
            });
        }
    }

    /// <summary>
    /// Updates a task status.
    /// </summary>
    /// <param name="id">The task ID.</param>
    /// <param name="request">The update status request.</param>
    /// <returns>The updated task.</returns>
    [HttpPut("{id}/status")]
    [Authorize]
    [ValidateAntiForgeryToken]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TaskDto>> UpdateTaskStatus(Guid id, [FromBody] UpdateTaskStatusRequest request)
    {
        try
        {
            var task = await _taskService.UpdateTaskStatusAsync(id, request.Status);
            if (task == null)
                return NotFound();

            return Ok(MapToDto(task));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Assigns a task to a user.
    /// </summary>
    /// <param name="id">The task ID.</param>
    /// <param name="request">The assign task request.</param>
    /// <returns>The updated task.</returns>
    [HttpPut("{id}/assignee")]
    [Authorize]
    [ValidateAntiForgeryToken]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskDto>> AssignTask(Guid id, [FromBody] AssignTaskRequest request)
    {
        var task = await _taskService.AssignTaskAsync(id, request.AssigneeId);
        if (task == null)
            return NotFound();

        return Ok(MapToDto(task));
    }

    /// <summary>
    /// Adds a time tracking entry to a task.
    /// </summary>
    /// <param name="id">The task ID.</param>
    /// <param name="request">The add time tracking request.</param>
    /// <returns>The created time tracking entry.</returns>
    [HttpPost("{id}/time-entries")]
    [Authorize]
    [ValidateAntiForgeryToken]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddTimeTrackingEntry(Guid id, [FromBody] AddTimeTrackingRequest request)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value ?? "unknown";
            var entry = await _taskService.AddTimeTrackingEntryAsync(id, userId, request.Hours, request.Date);
            return CreatedAtAction(nameof(AddTimeTrackingEntry), new { id }, entry);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Adds a dependency between two tasks.
    /// </summary>
    /// <param name="id">The task ID that is blocked.</param>
    /// <param name="request">The add dependency request.</param>
    /// <returns>The created dependency.</returns>
    [HttpPost("{id}/dependencies")]
    [Authorize]
    [ValidateAntiForgeryToken]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddTaskDependency(Guid id, [FromBody] AddTaskDependencyRequest request)
    {
        try
        {
            var dependency = await _taskService.AddTaskDependencyAsync(id, request.BlockedByTaskId);
            return CreatedAtAction(nameof(AddTaskDependency), new { id }, dependency);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gets a task by its SEO-friendly slug.
    /// </summary>
    /// <param name="projectId">The project ID.</param>
    /// <param name="slug">The task slug.</param>
    /// <returns>The task.</returns>
    [HttpGet("project/{projectId}/slug/{slug}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskDto>> GetTaskBySlug(Guid projectId, string slug)
    {
        var task = await _taskService.GetTaskBySlugAsync(projectId, slug);
        if (task == null)
            return NotFound();

        return Ok(MapToDto(task));
    }

    /// <summary>
    /// Maps a WorkTask entity to a TaskDto.
    /// </summary>
    private static TaskDto MapToDto(Domain.Entities.WorkTask task)
    {
        return new TaskDto
        {
            Id = task.Id,
            ProjectId = task.ProjectId,
            Title = task.Title,
            Slug = task.Slug,
            Description = task.Description,
            Status = task.Status,
            Priority = task.Priority,
            AssigneeId = task.AssigneeId,
            DueDate = task.DueDate,
            CreatedAt = task.CreatedAt,
            UpdatedAt = task.UpdatedAt
        };
    }
}
