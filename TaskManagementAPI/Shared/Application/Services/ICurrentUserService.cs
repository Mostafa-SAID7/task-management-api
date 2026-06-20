namespace TaskManagementAPI.Shared.Application.Services;

/// <summary>
/// Provides information about the currently authenticated user
/// for use in audit trails and business logic.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// The unique identifier of the current user.
    /// Returns null when the request is unauthenticated.
    /// </summary>
    string? UserId { get; }

    /// <summary>
    /// Whether the current request is authenticated.
    /// </summary>
    bool IsAuthenticated { get; }
}
