using Microsoft.AspNetCore.Authorization;

namespace TaskManagementAPI.Shared.Infrastructure.Authorization;

/// <summary>
/// Authorization requirement for editing tasks.
/// Verifies user has permission to edit a specific task.
/// </summary>
public class CanEditTaskRequirement : IAuthorizationRequirement
{
}
