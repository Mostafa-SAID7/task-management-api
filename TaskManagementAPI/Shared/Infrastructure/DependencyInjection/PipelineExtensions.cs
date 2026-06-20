using Microsoft.AspNetCore.StaticFiles;

namespace TaskManagementAPI.Shared.Infrastructure.DependencyInjection;

/// <summary>
/// Extension methods for configuring the HTTP request middleware pipeline.
/// </summary>
public static class PipelineExtensions
{
    /// <summary>
    /// Configures the complete middleware pipeline with proper ordering:
    /// 1. Swagger (dev/staging only)
    /// 2. Controllers mapping
    /// 3. Shared middleware (logging, exception handling, rate limiting, CORS)
    /// 4. HTTPS redirection
    /// 5. HSTS (production only)
    /// 6. Antiforgery, Authentication, Authorization
    /// 7. Static files with security headers
    /// 8. 404 handling for SPA routes
    /// </summary>
    public static IApplicationBuilder UseApplicationPipeline(this IApplicationBuilder app, IWebHostEnvironment env)
    {
        // 1. Swagger (development/staging only)
        app.UseSwaggerConfiguration(env);

        // 2. Map Controllers
        var webApp = (WebApplication)app;
        webApp.MapControllers();

        // 3. Shared middleware (logging, exception handling, rate limiting, CORS)
        app.UseSharedMiddleware();

        // 4. HTTPS redirection
        app.UseHttpsRedirection();

        // 5. HSTS for production/staging
        if (!env.IsDevelopment())
        {
            app.UseHsts();
            app.UseExceptionHandler("/error");
        }

        // 6. Antiforgery, Authentication, Authorization
        app.UseAntiforgery();
        app.UseAuthentication();
        app.UseAuthorization();

        // 7. Static files with security headers
        ConfigureStaticFiles(app, env);

        // 8. Health check endpoints and 404 handling
        ConfigureHealthCheckEndpoints(app);
        ConfigureNotFoundHandling(app);

        return app;
    }

    /// <summary>
    /// Configures default files and static file serving with security headers.
    /// </summary>
    private static void ConfigureStaticFiles(IApplicationBuilder app, IWebHostEnvironment env)
    {
        // Configure default files
        var defaultFilesOptions = new DefaultFilesOptions();
        defaultFilesOptions.DefaultFileNames.Clear();
        defaultFilesOptions.DefaultFileNames.Add("index.html");
        app.UseDefaultFiles(defaultFilesOptions);

        // Add security headers for API routes
        app.Use(async (context, next) =>
        {
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
                context.Response.Headers.Add("X-Frame-Options", "DENY");
                context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
                
                if (!env.IsDevelopment())
                {
                    context.Response.Headers.Add("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
                }
            }
            await next();
        });

        app.UseStaticFiles();
    }

    /// <summary>
    /// Configures health check endpoints (/health and /ready).
    /// </summary>
    private static void ConfigureHealthCheckEndpoints(IApplicationBuilder app)
    {
        var webApp = (WebApplication)app;
        webApp.MapHealthChecks();
    }

    /// <summary>
    /// Configures 404 handling for non-API routes.
    /// </summary>
    private static void ConfigureNotFoundHandling(IApplicationBuilder app)
    {
        var env = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();
        
        // Skip in Testing environment to avoid redirect issues
        if (env.IsEnvironment("Testing"))
            return;

        // 404 handling is handled by asp.net core default behavior
        // Custom routes are managed by individual controllers
    }
}
