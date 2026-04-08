using Microsoft.AspNetCore.Identity;
using TaskManagementAPI.Modules.Users.Application.DTOs;
using TaskManagementAPI.Modules.Users.Application.Services;
using TaskManagementAPI.Modules.Users.Domain.Entities;

namespace TaskManagementAPI.Modules.Users.Presentation.Controllers;

/// <summary>
/// API controller for user management operations.
/// </summary>
[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly UserService _userService;

    /// <summary>
    /// Initializes a new instance of the UsersController class.
    /// </summary>
    public UsersController(UserManager<ApplicationUser> userManager, UserService userService)
    {
        _userManager = userManager;
        _userService = userService;
    }

    /// <summary>
    /// Gets a user by ID.
    /// </summary>
    /// <param name="id">The user ID.</param>
    /// <returns>The user information.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> GetUser(string id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
            return NotFound();

        var roles = await _userManager.GetRolesAsync(user);
        return Ok(new UserDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            FullName = user.FullName,
            Roles = roles,
            CreatedAt = user.CreatedAt
        });
    }

    /// <summary>
    /// Updates a user's profile.
    /// </summary>
    /// <param name="id">The user ID.</param>
    /// <param name="request">The update request.</param>
    /// <returns>The updated user information.</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> UpdateUser(string id, [FromBody] UpdateUserRequest request)
    {
        var user = await _userService.UpdateUserAsync(id, request.FullName);
        if (user == null)
            return NotFound();

        var roles = await _userManager.GetRolesAsync(user);
        return Ok(new UserDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            FullName = user.FullName,
            Roles = roles,
            CreatedAt = user.CreatedAt
        });
    }

    /// <summary>
    /// Changes a user's password.
    /// </summary>
    /// <param name="id">The user ID.</param>
    /// <param name="request">The change password request.</param>
    /// <returns>No content if successful.</returns>
    [HttpPut("{id}/password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangePassword(string id, [FromBody] ChangePasswordRequest request)
    {
        if (request.NewPassword != request.ConfirmPassword)
            return BadRequest(new { error = "Passwords do not match" });

        try
        {
            var success = await _userService.ChangePasswordAsync(id, request.CurrentPassword, request.NewPassword);
            if (!success)
                return BadRequest(new { error = "Failed to change password" });

            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Assigns a role to a user (admin only).
    /// </summary>
    /// <param name="id">The user ID.</param>
    /// <param name="request">The assign role request.</param>
    /// <returns>No content if successful.</returns>
    [HttpPost("{id}/roles")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AssignRole(string id, [FromBody] AssignRoleRequest request)
    {
        var success = await _userService.AssignRoleAsync(id, request.RoleName);
        if (!success)
            return BadRequest(new { error = "Failed to assign role" });

        return NoContent();
    }

    /// <summary>
    /// Gets all users (admin only).
    /// </summary>
    /// <returns>A list of all users.</returns>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers()
    {
        var users = _userService.GetAllUsers();
        var userDtos = new List<UserDto>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            userDtos.Add(new UserDto
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FullName = user.FullName,
                Roles = roles,
                CreatedAt = user.CreatedAt
            });
        }

        return Ok(userDtos);
    }
}
