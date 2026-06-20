using Microsoft.OpenApi.Models;

namespace TaskManagementAPI.Shared.Infrastructure.DependencyInjection;

/// <summary>
/// Extension methods for Swagger/OpenAPI configuration.
/// </summary>
public static class SwaggerExtensions
{
    /// <summary>
    /// Configures Swagger generation with JWT authentication and API documentation.
    /// </summary>
    public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Task Management API",
                Version = "v1",
                Description = "Comprehensive task management system with project collaboration, real-time updates, and audit logging",
                Contact = new OpenApiContact
                {
                    Name = "API Support",
                    Email = "support@taskmanagement.api"
                },
                License = new OpenApiLicense
                {
                    Name = "MIT License",
                    Url = new Uri("https://opensource.org/licenses/MIT")
                }
            });

            // Add JWT authentication to Swagger
            var securityScheme = new OpenApiSecurityScheme
            {
                Name = "JWT Authentication",
                Description = "JWT Bearer token authentication. Example: `eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...`",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Reference = new OpenApiReference
                {
                    Id = "Bearer",
                    Type = ReferenceType.SecurityScheme
                }
            };

            c.AddSecurityDefinition("Bearer", securityScheme);
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                { securityScheme, new List<string>() }
            });

            // Include XML comments if available
            var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (System.IO.File.Exists(xmlPath))
            {
                c.IncludeXmlComments(xmlPath);
            }
        });

        return services;
    }

    /// <summary>
    /// Configures Swagger UI middleware (only in Development/Staging).
    /// </summary>
    public static IApplicationBuilder UseSwaggerConfiguration(this IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment() || env.IsEnvironment("Staging"))
        {
            app.UseSwagger();
            
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Task Management API v1");
                c.RoutePrefix = "swagger";
                c.DisplayOperationId();
                c.DefaultModelExpandDepth(2);
                c.DefaultModelsExpandDepth(1);
                
                // Custom styling
                c.InjectStylesheet("/swagger-nav.css");
                c.InjectJavascript("/swagger-nav.js");
                
                // Disable try-it-out in production
                if (!env.IsDevelopment())
                {
                    c.SupportedSubmitMethods();
                }
            });
        }

        return app;
    }
}
