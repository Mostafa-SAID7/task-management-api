using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TaskManagementAPI.Modules.Projects.Infrastructure.Persistence;
using TaskManagementAPI.Modules.Tasks.Infrastructure.Persistence;
using TaskManagementAPI.Modules.Users.Infrastructure.Persistence;
using TaskManagementAPI.Modules.Notifications.Infrastructure.Persistence;

namespace TaskManagementAPI.Tests.Integration.Shared;

/// <summary>
/// Custom WebApplicationFactory for horizontal integration testing with in-memory database and mock auth.
/// </summary>
public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    public const string TestAuthScheme = "TestAuthScheme";
    public const string DefaultUserId = "test-user-123";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Set environment to Testing
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string?>("Jwt:Key", "testing-secret-key-at-least-32-chars-long"),
                new KeyValuePair<string, string?>("Jwt:Issuer", "TaskManagementAPI"),
                new KeyValuePair<string, string?>("Jwt:Audience", "TaskManagementAPI")
            });
        });

        builder.ConfigureTestServices(services =>
        {
            // 1. Replace real DBContexts with In-Memory
            ReplaceDbContext<ProjectsDbContext>(services, "ProjectsTestDb_" + Guid.NewGuid());
            ReplaceDbContext<TasksDbContext>(services, "TasksTestDb_" + Guid.NewGuid());
            ReplaceDbContext<UsersDbContext>(services, "UsersTestDb_" + Guid.NewGuid());
            ReplaceDbContext<NotificationsDbContext>(services, "NotificationsTestDb_" + Guid.NewGuid());

            // 2. Force authentication to use our Test Scheme
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = TestAuthScheme;
                options.DefaultChallengeScheme = TestAuthScheme;
                options.DefaultScheme = TestAuthScheme;
                options.DefaultSignInScheme = TestAuthScheme;
            })
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthScheme, options => { });

            // Ensure the schemes are set correctly even if AddIdentity was called later
            services.PostConfigure<AuthenticationOptions>(options =>
            {
                options.DefaultAuthenticateScheme = TestAuthScheme;
                options.DefaultChallengeScheme = TestAuthScheme;
                options.DefaultScheme = TestAuthScheme;
                options.DefaultSignInScheme = TestAuthScheme;
            });

            // 3. Bypass custom authorization requirements
            services.AddSingleton<IAuthorizationHandler, AllowAllAuthorizationHandler>();

            // 4. Disable Antiforgery for integration tests to simplify POST requests
            services.AddAntiforgery(options =>
            {
                options.HeaderName = "X-CSRF-TOKEN";
            });

            // Replace IAntiforgery with a mock that always passes
            services.RemoveAll(typeof(Microsoft.AspNetCore.Antiforgery.IAntiforgery));
            services.AddSingleton<Microsoft.AspNetCore.Antiforgery.IAntiforgery, TestAntiforgery>();
        });
    }

    private static void ReplaceDbContext<TContext>(IServiceCollection services, string dbName) where TContext : DbContext
    {
        services.RemoveAll(typeof(DbContextOptions<TContext>));
        services.AddDbContext<TContext>(options =>
        {
            options.UseInMemoryDatabase(dbName);
            options.ConfigureWarnings(x => x.Ignore(RelationalEventId.PendingModelChangesWarning));
        });
    }

    public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger, UrlEncoder encoder)
            : base(options, logger, encoder)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var claims = new[] { 
                new Claim(ClaimTypes.Name, "Test User"),
                new Claim(ClaimTypes.NameIdentifier, DefaultUserId),
                new Claim("sub", DefaultUserId)
            };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, TestAuthScheme);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }

    /// <summary>
    /// Authorization handler that always succeeds, bypassing policy checks for tests.
    /// </summary>
    public class AllowAllAuthorizationHandler : IAuthorizationHandler
    {
        public Task HandleAsync(AuthorizationHandlerContext context)
        {
            foreach (var requirement in context.PendingRequirements.ToList())
            {
                context.Succeed(requirement);
            }
            return Task.CompletedTask;
        }
    }

    private class TestAntiforgery : Microsoft.AspNetCore.Antiforgery.IAntiforgery
    {
        public Task<Microsoft.AspNetCore.Antiforgery.AntiforgeryTokenSet> GetAndStoreTokensAsync(Microsoft.AspNetCore.Http.HttpContext httpContext) 
            => Task.FromResult(new Microsoft.AspNetCore.Antiforgery.AntiforgeryTokenSet("test", "test", "test", "test"));
        public Task<Microsoft.AspNetCore.Antiforgery.AntiforgeryTokenSet> GetTokensAsync(Microsoft.AspNetCore.Http.HttpContext httpContext)
            => Task.FromResult(new Microsoft.AspNetCore.Antiforgery.AntiforgeryTokenSet("test", "test", "test", "test"));
        public Task<bool> IsRequestValidAsync(Microsoft.AspNetCore.Http.HttpContext httpContext) => Task.FromResult(true);
        public void SetCookieTokenAndHeader(Microsoft.AspNetCore.Http.HttpContext httpContext) { }
        public Microsoft.AspNetCore.Antiforgery.AntiforgeryTokenSet GetAndStoreTokens(Microsoft.AspNetCore.Http.HttpContext httpContext)
            => new Microsoft.AspNetCore.Antiforgery.AntiforgeryTokenSet("test", "test", "test", "test");
        public Microsoft.AspNetCore.Antiforgery.AntiforgeryTokenSet GetTokens(Microsoft.AspNetCore.Http.HttpContext httpContext)
            => new Microsoft.AspNetCore.Antiforgery.AntiforgeryTokenSet("test", "test", "test", "test");
        public Task ValidateRequestAsync(Microsoft.AspNetCore.Http.HttpContext httpContext) => Task.CompletedTask;
    }
}
