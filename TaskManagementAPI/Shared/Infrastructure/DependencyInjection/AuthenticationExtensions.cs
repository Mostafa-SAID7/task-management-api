using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TaskManagementAPI.Shared.Infrastructure.Authorization;

namespace TaskManagementAPI.Shared.Infrastructure.DependencyInjection;

/// <summary>
/// Extension methods for configuring authentication and authorization.
/// </summary>
public static class AuthenticationExtensions
{
    /// <summary>
    /// Configures JWT Bearer authentication with token validation.
    /// </summary>
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtKey = configuration["Jwt:Key"];
        if (string.IsNullOrEmpty(jwtKey) || jwtKey.Length < 32)
        {
            throw new InvalidOperationException("JWT Key must be configured and at least 32 characters long");
        }

        var jwtKeyBytes = Encoding.UTF8.GetBytes(jwtKey);
        
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(jwtKeyBytes),
                ValidateIssuer = true,
                ValidIssuer = configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = configuration["Jwt:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
                RequireExpirationTime = true
            };
            
            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    if (context.Exception is SecurityTokenExpiredException)
                    {
                        context.Response.Headers.Add("X-Token-Expired", "true");
                    }
                    return Task.CompletedTask;
                }
            };
        });

        // Configure authorization policies
        services.AddAuthorization(options =>
        {
            options.AddPolicy("CanEditTask", policy =>
                policy.Requirements.Add(new CanEditTaskRequirement()));
            options.AddPolicy("IsProjectMember", policy =>
                policy.Requirements.Add(new IsProjectMemberRequirement()));
        });

        return services;
    }

    /// <summary>
    /// Configures antiforgery protection for state-changing operations.
    /// </summary>
    public static IServiceCollection AddAntiforgeryProtection(this IServiceCollection services)
    {
        services.AddAntiforgery(options =>
        {
            options.HeaderName = "X-CSRF-TOKEN";
            options.FormFieldName = "_RequestVerificationToken";
            options.SuppressXFrameOptionsHeader = false;
        });

        return services;
    }
}
