using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace TaskManagementAPI.Tests.Integration.Helpers;

/// <summary>
/// Helper class for generating JWT tokens for integration tests.
/// </summary>
public static class JwtTokenHelper
{
    private const string JwtKey = "dev-secret-key-change-in-production-minimum-32-characters-dev";
    private const string JwtIssuer = "TaskManagementAPI";
    private const string JwtAudience = "TaskManagementAPI";

    /// <summary>
    /// Generates a valid JWT token for testing.
    /// </summary>
    /// <param name="userId">The user ID (sub claim).</param>
    /// <param name="expirationMinutes">Token expiration in minutes (default 60).</param>
    /// <returns>A valid JWT token string.</returns>
    public static string GenerateToken(string userId = "test-user-123", int expirationMinutes = 60)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim("sub", userId),
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Name, "Test User"),
            new Claim(ClaimTypes.Email, "test@example.com")
        };

        var token = new JwtSecurityToken(
            issuer: JwtIssuer,
            audience: JwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
