using Microsoft.AspNetCore.StaticFiles;

namespace TaskManagementAPI.Shared.Infrastructure.DependencyInjection;

/// <summary>
/// Extension methods for configuring HTTP pipeline middleware and security.
/// </summary>
public static class MiddlewareExtensions
{
    /// <summary>
    /// Configures security headers, HTTPS redirection, and HSTS.
    /// </summary>
    public static IApplicationBuilder UseSecurityMiddleware(this IApplicationBuilder app, IWebHostEnvironment env)
    {
        // HTTPS redirection
        app.UseHttpsRedirection();

        // HSTS for HTTPS enforcement in production/staging
        if (!env.IsDevelopment())
        {
            app.UseHsts();
            app.UseExceptionHandler("/error");
        }

        return app;
    }

    /// <summary>
    /// Adds security headers to API responses.
    /// </summary>
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.Use(async (context, next) =>
        {
            // Apply security headers to API responses
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
                context.Response.Headers.Add("X-Frame-Options", "DENY");
                context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
                
                if (!env.IsDevelopment())
                {
                    context.Response.Headers.Add("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
                    context.Response.Headers.Add("Content-Security-Policy", "default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'; img-src 'self' data: https:;");
                }
            }
            
            await next();
        });

        return app;
    }

    /// <summary>
    /// Configures static files, default files, and 404 handling.
    /// </summary>
    public static IApplicationBuilder UseStaticFilesConfiguration(this IApplicationBuilder app, IWebHostEnvironment env)
    {
        // Configure default files
        var defaultFilesOptions = new DefaultFilesOptions();
        defaultFilesOptions.DefaultFileNames.Clear();
        defaultFilesOptions.DefaultFileNames.Add("index.html");
        app.UseDefaultFiles(defaultFilesOptions);

        // Serve static files with proper content type mapping
        app.UseStaticFiles(new StaticFileOptions
        {
            OnPrepareResponse = ctx =>
            {
                // Add cache headers for static assets
                if (ctx.File.Name.EndsWith(".js") || ctx.File.Name.EndsWith(".css"))
                {
                    ctx.Context.Response.Headers.Add("Cache-Control", "public, max-age=31536000, immutable");
                }
                else if (ctx.File.Name.EndsWith(".html"))
                {
                    ctx.Context.Response.Headers.Add("Cache-Control", "public, max-age=3600");
                }
            }
        });

        // Handle 404 errors for non-API routes (skip in Testing environment)
        if (!env.IsEnvironment("Testing"))
        {
            app.Use(async (context, next) =>
            {
                await next();
                
                // If response is 404 and not an API route, serve index.html for SPA routing
                if (context.Response.StatusCode == 404 && !context.Request.Path.StartsWithSegments("/api"))
                {
                    context.Request.Path = "/index.html";
                    await next();
                }
            });
        }

        return app;
    }
}
