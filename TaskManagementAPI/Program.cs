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
builder.Services.AddSwaggerGen();

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
        var projectsContext = scope.ServiceProvider.GetRequiredService<ProjectsDbContext>();
        var tasksContext = scope.ServiceProvider.GetRequiredService<TasksDbContext>();
        var usersContext = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
        var notificationsContext = scope.ServiceProvider.GetRequiredService<NotificationsDbContext>();

        // Using MigrateAsync to ensure correct tracking via __EFMigrationsHistory
        // Skip if provider is in-memory (for tests)
        if (projectsContext.Database.IsRelational())
        {
            try { await projectsContext.Database.MigrateAsync(); }
            catch (Exception ex) { Log.Warning(ex, "Error migrating ProjectsDbContext"); }
        }

        if (tasksContext.Database.IsRelational())
        {
            try { await tasksContext.Database.MigrateAsync(); }
            catch (Exception ex) { Log.Warning(ex, "Error migrating TasksDbContext"); }
        }

        if (usersContext.Database.IsRelational())
        {
            try { await usersContext.Database.MigrateAsync(); }
            catch (Exception ex) { Log.Warning(ex, "Error migrating UsersDbContext"); }
        }

        if (notificationsContext.Database.IsRelational())
        {
            try { await notificationsContext.Database.MigrateAsync(); }
            catch (Exception ex) { Log.Warning(ex, "Error migrating NotificationsDbContext"); }
        }
    }
}
catch (Exception ex)
{
    Log.Error(ex, "An error occurred while initializing the database");
}

// Configure the HTTP request pipeline
// Configure the HTTP request pipeline
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Task Management API v1");
    c.RoutePrefix = "swagger";
    c.InjectStylesheet("/swagger-nav.css");
    c.InjectJavascript("/swagger-nav.js");
});


app.MapControllers();
app.UseSharedMiddleware();
app.UseHttpsRedirection();

// Add HSTS for HTTPS enforcement
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseAntiforgery();
app.UseAuthentication();
app.UseAuthorization();

// Configure default files and static files
var defaultFilesOptions = new DefaultFilesOptions();
defaultFilesOptions.DefaultFileNames.Clear();
defaultFilesOptions.DefaultFileNames.Add("index.html");
app.UseDefaultFiles(defaultFilesOptions);
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

app.Run();

public partial class Program { }
