using Microsoft.AspNetCore.StaticFiles;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TaskManagementAPI.Shared.Infrastructure.Authorization;
using TaskManagementAPI.Modules.Projects.Configuration;
using TaskManagementAPI.Modules.Tasks.Configuration;
using TaskManagementAPI.Modules.Users.Configuration;
using TaskManagementAPI.Modules.Notifications.Configuration;
using TaskManagementAPI.Modules.Projects.Infrastructure.Persistence;
using TaskManagementAPI.Modules.Tasks.Infrastructure.Persistence;
using TaskManagementAPI.Modules.Users.Infrastructure.Persistence;
using TaskManagementAPI.Modules.Notifications.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Load configuration from appsettings and module-specific files
ConfigurationLoader.LoadConfiguration(builder);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers()
    .AddApplicationPart(typeof(Program).Assembly);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
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

// Configure JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrEmpty(jwtKey) || jwtKey.Length < 32)
{
    throw new InvalidOperationException("JWT Key must be configured and at least 32 characters long");
}

var jwtKeyBytes = Encoding.UTF8.GetBytes(jwtKey);
builder.Services.AddAuthentication(options =>
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
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
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

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanEditTask", policy =>
        policy.Requirements.Add(new CanEditTaskRequirement()));
    options.AddPolicy("IsProjectMember", policy =>
        policy.Requirements.Add(new IsProjectMemberRequirement()));
});

builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.FormFieldName = "_RequestVerificationToken";
    options.SuppressXFrameOptionsHeader = false;
});

builder.Services.AddSharedServices();
builder.Services.AddProjectsModule(builder.Configuration);
builder.Services.AddTasksModule(builder.Configuration);
builder.Services.AddUsersModule(builder.Configuration);
builder.Services.AddNotificationsModule(builder.Configuration);

var app = builder.Build();

// Initialize databases
try
{
    using (var scope = app.Services.CreateScope())
    {
        // Attempt to get DbContexts - wrapped in try/catch since DNS resolution failures
        // will occur here in IPv6-only networks when constructing the contexts
        ProjectsDbContext? projectsContext = null;
        TasksDbContext? tasksContext = null;
        UsersDbContext? usersContext = null;
        NotificationsDbContext? notificationsContext = null;

        try { projectsContext = scope.ServiceProvider.GetRequiredService<ProjectsDbContext>(); }
        catch (System.Net.Sockets.SocketException ex) when (ex.ErrorCode == 11004) { }
        catch (Exception) { }

        try { tasksContext = scope.ServiceProvider.GetRequiredService<TasksDbContext>(); }
        catch (System.Net.Sockets.SocketException ex) when (ex.ErrorCode == 11004) { }
        catch (Exception) { }

        try { usersContext = scope.ServiceProvider.GetRequiredService<UsersDbContext>(); }
        catch (System.Net.Sockets.SocketException ex) when (ex.ErrorCode == 11004) { }
        catch (Exception) { }

        try { notificationsContext = scope.ServiceProvider.GetRequiredService<NotificationsDbContext>(); }
        catch (System.Net.Sockets.SocketException ex) when (ex.ErrorCode == 11004) { }
        catch (Exception) { }

        // Using MigrateAsync to ensure correct tracking via __EFMigrationsHistory
        // Skip if provider is in-memory (for tests) or if context couldn't be created
        if (projectsContext?.Database.IsRelational() == true)
        {
            try { await projectsContext.Database.MigrateAsync(); }
            catch (System.Net.Sockets.SocketException ex) 
            { 
                if (ex.ErrorCode != 11004)
                    Log.Warning(ex, "Network error migrating ProjectsDbContext"); 
            }
            catch (Exception ex) { Log.Warning(ex, "Error migrating ProjectsDbContext"); }
        }

        if (tasksContext?.Database.IsRelational() == true)
        {
            try { await tasksContext.Database.MigrateAsync(); }
            catch (System.Net.Sockets.SocketException ex) 
            { 
                if (ex.ErrorCode != 11004)
                    Log.Warning(ex, "Network error migrating TasksDbContext"); 
            }
            catch (Exception ex) { Log.Warning(ex, "Error migrating TasksDbContext"); }
        }

        if (usersContext?.Database.IsRelational() == true)
        {
            try { await usersContext.Database.MigrateAsync(); }
            catch (System.Net.Sockets.SocketException ex) 
            { 
                if (ex.ErrorCode != 11004)
                    Log.Warning(ex, "Network error migrating UsersDbContext"); 
            }
            catch (Exception ex) { Log.Warning(ex, "Error migrating UsersDbContext"); }
        }

        if (notificationsContext?.Database.IsRelational() == true)
        {
            try { await notificationsContext.Database.MigrateAsync(); }
            catch (System.Net.Sockets.SocketException ex) 
            { 
                if (ex.ErrorCode != 11004)
                    Log.Warning(ex, "Network error migrating NotificationsDbContext"); 
            }
            catch (Exception ex) { Log.Warning(ex, "Error migrating NotificationsDbContext"); }
        }
    }
}
catch (Exception ex)
{
    Log.Error(ex, "An error occurred while initializing the database");
}

// Configure the HTTP request pipeline
// Enable Swagger only in Development and Staging environments
if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Staging"))
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
        if (!app.Environment.IsDevelopment())
        {
            c.SupportedSubmitMethods();
        }
    });
}


app.MapControllers();
app.UseSharedMiddleware();

// Security middleware - apply before other middleware
app.UseHttpsRedirection();

// Add HSTS for HTTPS enforcement in production/staging
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseExceptionHandler("/error");
}

app.UseAntiforgery();
app.UseAuthentication();
app.UseAuthorization();

// Configure default files and static files
var defaultFilesOptions = new DefaultFilesOptions();
defaultFilesOptions.DefaultFileNames.Clear();
defaultFilesOptions.DefaultFileNames.Add("index.html");
app.UseDefaultFiles(defaultFilesOptions);

// Add security headers for static files
app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/api"))
    {
        context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Add("X-Frame-Options", "DENY");
        context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
        if (!app.Environment.IsDevelopment())
        {
            context.Response.Headers.Add("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
        }
    }
    await next();
});

app.UseStaticFiles();

// Handle 404 errors for non-API routes (skip in Testing environment to avoid redirect issues)
if (!app.Environment.IsEnvironment("Testing"))
{
    app.Use(async (context, next) =>
    {
        await next();
        
        // If response is 404 and not an API route, serve 404.html
        if (context.Response.StatusCode == 404 && !context.Request.Path.StartsWithSegments("/api"))
        {
            context.Request.Path = "/404.html";
            await next();
        }
    });
}

// Health check endpoint
app.MapGet("/health", (ILogger<Program> logger) =>
{
    var health = new
    {
        status = "healthy",
        timestamp = DateTime.UtcNow.ToString("o"),
        environment = app.Environment.EnvironmentName,
        version = "1.0.0",
        uptime = Environment.TickCount / 1000.0
    };
    logger.LogInformation("Health check: {Status}", health.status);
    return Results.Ok(health);
})
.Produces(StatusCodes.Status200OK)
.WithName("Health")
.AllowAnonymous();

// Ready endpoint (for Kubernetes readiness probes)
app.MapGet("/ready", () => Results.Ok(new { ready = true }))
.Produces(StatusCodes.Status200OK)
.WithName("Ready")
.AllowAnonymous();

app.Run();

public partial class Program { }
