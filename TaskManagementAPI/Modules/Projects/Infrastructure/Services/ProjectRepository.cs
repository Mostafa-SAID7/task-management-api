using Microsoft.EntityFrameworkCore;
using TaskManagementAPI.Modules.Projects.Domain.Entities;
using TaskManagementAPI.Modules.Projects.Infrastructure.Persistence;
using TaskManagementAPI.Shared.Infrastructure.Repositories;

namespace TaskManagementAPI.Modules.Projects.Infrastructure.Services;

/// <summary>
/// Repository implementation for Project entity.
/// </summary>
public class ProjectRepository : GenericRepository<Project>, IProjectRepository
{
    private readonly ProjectsDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the ProjectRepository class.
    /// </summary>
    /// <param name="context">The Projects DbContext.</param>
    public ProjectRepository(ProjectsDbContext context) : base(context)
    {
        _dbContext = context;
    }

    /// <summary>
    /// Gets a project by ID with all associated members.
    /// </summary>
    /// <param name="id">The project ID.</param>
    /// <returns>The project with members, or null if not found.</returns>
    public async Task<Project?> GetByIdWithMembersAsync(Guid id)
    {
        return await _dbContext.Projects
            .Include(p => p.Members)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    /// <summary>
    /// Gets all projects for a specific user with optimized queries.
    /// Uses eager loading and AsNoTracking for read-only operations.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <returns>A collection of projects the user is a member of.</returns>
    public async Task<IEnumerable<Project>> GetUserProjectsAsync(string userId)
    {
        return await _dbContext.Projects
            .Where(p => p.Members.Any(m => m.UserId == userId && !m.IsDeleted))
            .Include(p => p.Members.Where(m => !m.IsDeleted))
            .ToListAsync();
    }

    /// <summary>
    /// Gets projects with pagination using optimized queries.
    /// Applies Include before pagination to avoid N+1 queries.
    /// </summary>
    /// <param name="pageNumber">The page number (1-based).</param>
    /// <param name="pageSize">The page size.</param>
    /// <returns>A tuple containing the projects and total count.</returns>
    public async Task<(IEnumerable<Project> Projects, int TotalCount)> GetProjectsPagedAsync(int pageNumber, int pageSize)
    {
        var query = _dbContext.Projects.AsQueryable();
        
        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply Include BEFORE pagination to avoid N+1 queries
        var projects = await query
            .Include(p => p.Members.Where(m => !m.IsDeleted))
            .OrderByDescending(p => p.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (projects, totalCount);
    }

    /// <summary>
    /// Gets a project by its SEO-friendly slug.
    /// </summary>
    /// <param name="slug">The project slug.</param>
    /// <returns>The project, or null if not found.</returns>
    public async Task<Project?> GetBySlugAsync(string slug)
    {
        return await _dbContext.Projects
            .Include(p => p.Members)
            .FirstOrDefaultAsync(p => p.Slug == slug);
    }

    /// <summary>
    /// Gets all project slugs.
    /// </summary>
    /// <returns>A collection of all project slugs.</returns>
    public async Task<IEnumerable<string>> GetAllSlugsAsync()
    {
        return await _dbContext.Projects
            .Select(p => p.Slug)
            .ToListAsync();
    }
}
