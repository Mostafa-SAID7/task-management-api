using Microsoft.AspNetCore.Authorization;

namespace TaskManagementAPI.Shared.Infrastructure.Authorization;

/// <summary>
/// Authorization requirement for project membership.
/// Verifies user is a member of a specific project.
/// </summary>
public class IsProjectMemberRequirement : IAuthorizationRequirement
{
}
