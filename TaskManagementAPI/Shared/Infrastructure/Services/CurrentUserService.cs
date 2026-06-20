using System.Security.Claims;
using TaskManagementAPI.Shared.Application.Services;

namespace TaskManagementAPI.Shared.Infrastructure.Services;

/// <summary>
/// Resolves the currently authenticated user from the HTTP context.
/// Scoped to the lifetime of the request.
/// </summary>
public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <inheritdoc/>
    public string? UserId =>
        _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value
        ?? _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    /// <inheritdoc/>
    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true;
}
