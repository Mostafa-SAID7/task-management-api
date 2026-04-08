using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagementAPI.Modules.Projects.Application.DTOs;
using TaskManagementAPI.Modules.Projects.Application.Services;

namespace TaskManagementAPI.Modules.Projects.Presentation.Controllers;

/// <summary>
/// API controller for project management.
/// </summary>
[ApiController]
[Route("api/projects")]
[Authorize]
public class ProjectsController : ControllerBase
{
    private readonly ProjectService _projectService;

    /// <summary>
    /// Initializes a new instance of the ProjectsController class.
    /// </summary>
    /// <param name="projectService">The project service.</param>
    public ProjectsController(ProjectService projectService)
    {
        _projectService = projectService;
    }

    /// <summary>
    /// Creates a new project.
    /// </summary>
    /// <param name="request">The create project request.</param>
    /// <returns>The created project.</returns>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult<ProjectDto>> CreateProject([FromBody] CreateProjectRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { error = "Project name is required." });

        if (request.Description?.Length > 2000)
            return BadRequest(new { error = "Project description cannot exceed 2000 characters." });

        var project = await _projectService.CreateProjectAsync(request.Name, request.Description);

        var dto = MapToDto(project);
        return CreatedAtAction(nameof(GetProject), new { id = project.Id }, dto);
    }

    /// <summary>
    /// Gets a project by ID.
    /// </summary>
    /// <param name="id">The project ID.</param>
    /// <returns>The project.</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<ProjectDto>> GetProject(Guid id)
    {
        var project = await _projectService.GetProjectWithMembersAsync(id);
        if (project == null)
            return NotFound(new { error = "Project not found." });

        return Ok(MapToDto(project));
    }

    /// <summary>
    /// Updates a project.
    /// </summary>
    /// <param name="id">The project ID.</param>
    /// <param name="request">The update project request.</param>
    /// <returns>The updated project.</returns>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult<ProjectDto>> UpdateProject(Guid id, [FromBody] UpdateProjectRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { error = "Project name is required." });

        if (request.Description?.Length > 2000)
            return BadRequest(new { error = "Project description cannot exceed 2000 characters." });

        var project = await _projectService.UpdateProjectAsync(id, request.Name, request.Description);
        if (project == null)
            return NotFound(new { error = "Project not found." });

        return Ok(MapToDto(project));
    }

    /// <summary>
    /// Deletes a project (soft delete).
    /// </summary>
    /// <param name="id">The project ID.</param>
    /// <returns>No content.</returns>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteProject(Guid id)
    {
        var success = await _projectService.DeleteProjectAsync(id);
        if (!success)
            return NotFound(new { error = "Project not found." });

        return NoContent();
    }

    /// <summary>
    /// Gets a paginated list of projects.
    /// </summary>
    /// <param name="pageNumber">The page number (default: 1).</param>
    /// <param name="pageSize">The page size (default: 20, max: 100).</param>
    /// <returns>A paginated list of projects.</returns>
    [HttpGet]
    public async Task<ActionResult<ProjectListResponse>> GetProjects(int pageNumber = 1, int pageSize = 20)
    {
        if (pageNumber < 1)
            pageNumber = 1;

        if (pageSize < 1)
            pageSize = 20;

        if (pageSize > 100)
            pageSize = 100;

        var (projects, totalCount) = await _projectService.GetProjectsPagedAsync(pageNumber, pageSize);

        var response = new ProjectListResponse
        {
            Data = projects.Select(MapToDto),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        return Ok(response);
    }

    /// <summary>
    /// Adds a member to a project.
    /// </summary>
    /// <param name="id">The project ID.</param>
    /// <param name="request">The add member request.</param>
    /// <returns>The added member.</returns>
    [HttpPost("{id}/members")]
    [Authorize(Roles = "Admin,Manager")]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult<ProjectMemberDto>> AddMember(Guid id, [FromBody] AddProjectMemberRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.UserId))
            return BadRequest(new { error = "User ID is required." });

        try
        {
            var member = await _projectService.AddMemberAsync(id, request.UserId, request.Role);
            return CreatedAtAction(nameof(GetProject), new { id }, MapMemberToDto(member));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Removes a member from a project.
    /// </summary>
    /// <param name="id">The project ID.</param>
    /// <param name="userId">The user ID to remove.</param>
    /// <returns>No content.</returns>
    [HttpDelete("{id}/members/{userId}")]
    [Authorize(Roles = "Admin,Manager")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveMember(Guid id, string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return BadRequest(new { error = "User ID is required." });

        var success = await _projectService.RemoveMemberAsync(id, userId);
        if (!success)
            return NotFound(new { error = "Project or member not found." });

        return NoContent();
    }

    /// <summary>
    /// Gets a project by its SEO-friendly slug.
    /// </summary>
    /// <param name="slug">The project slug.</param>
    /// <returns>The project.</returns>
    [HttpGet("slug/{slug}")]
    [Authorize]
    public async Task<ActionResult<ProjectDto>> GetProjectBySlug(string slug)
    {
        var project = await _projectService.GetProjectBySlugAsync(slug);
        if (project == null)
            return NotFound(new { error = "Project not found." });

        return Ok(MapToDto(project));
    }

    private ProjectDto MapToDto(Domain.Entities.Project project)
    {
        return new ProjectDto
        {
            Id = project.Id,
            Name = project.Name,
            Slug = project.Slug,
            Description = project.Description,
            Status = project.Status,
            CreatedAt = project.CreatedAt,
            UpdatedAt = project.UpdatedAt,
            Members = project.Members.Select(MapMemberToDto).ToList()
        };
    }

    private ProjectMemberDto MapMemberToDto(Domain.Entities.ProjectMember member)
    {
        return new ProjectMemberDto
        {
            Id = member.Id,
            UserId = member.UserId,
            Role = member.Role,
            JoinedAt = member.JoinedAt
        };
    }

    /// <summary>
    /// Gets projects for the current user.
    /// </summary>
    /// <returns>A list of projects the user is a member of.</returns>
    [HttpGet("user/projects")]
    public async Task<ActionResult<IEnumerable<ProjectDto>>> GetUserProjects()
    {
        var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("nameid")?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { error = "User ID not found in token." });

        var projects = await _projectService.GetUserProjectsAsync(userId);
        return Ok(projects.Select(MapToDto));
    }
}
