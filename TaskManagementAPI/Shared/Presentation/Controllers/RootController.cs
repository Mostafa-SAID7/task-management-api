using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TaskManagementAPI.Shared.Presentation.Controllers;

/// <summary>
/// Root API controller - handles base route and redirects.
/// Shared across all modules (not module-specific).
/// </summary>
[ApiController]
[Route("")]
[AllowAnonymous]
public class RootController : ControllerBase
{
    private readonly IWebHostEnvironment _env;

    public RootController(IWebHostEnvironment env)
    {
        _env = env;
    }

    /// <summary>
    /// Handles GET requests to the root path (/).
    /// - Development: Redirects to Swagger UI for API documentation
    /// - Production: Returns API metadata and available endpoints
    /// </summary>
    [HttpGet]
    public IActionResult Index()
    {
        if (_env.IsDevelopment())
        {
            // Development: redirect to Swagger for testing and documentation
            return Redirect("/swagger");
        }

        // Production: return minimal API info
        return Ok(new
        {
            application = "Task Management API",
            version = "1.0.0",
            status = "operational",
            endpoints = new
            {
                health = "/health",
                ready = "/ready",
                api = "/api/*",
                documentation = "/swagger (dev only)"
            },
            timestamp = DateTime.UtcNow
        });
    }
}
