using Serilog;
using TaskManagementAPI.Shared.Infrastructure.Configuration;
using TaskManagementAPI.Shared.Infrastructure.DependencyInjection;
using TaskManagementAPI.Modules.Projects.Configuration;
using TaskManagementAPI.Modules.Tasks.Configuration;
using TaskManagementAPI.Modules.Users.Configuration;
using TaskManagementAPI.Modules.Notifications.Configuration;

var builder = WebApplication.CreateBuilder(args);

// ┌─────────────────────────────────────────────────────────────────┐
// │ 1. CONFIGURATION & LOGGING                                      │
// └─────────────────────────────────────────────────────────────────┘

ConfigurationLoader.LoadConfiguration(builder);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

// ┌─────────────────────────────────────────────────────────────────┐
// │ 2. REGISTER SERVICES (ORDER MATTERS!)                           │
// └─────────────────────────────────────────────────────────────────┘

// Controllers and API exploration
builder.Services.AddControllers()
    .AddApplicationPart(typeof(Program).Assembly);

// Swagger/OpenAPI documentation
builder.Services.AddSwaggerConfiguration();

// Authentication & Authorization
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddAntiforgeryProtection();

// Shared cross-cutting services
builder.Services.AddSharedServices();

// Module-specific services
builder.Services.AddProjectsModule(builder.Configuration);
builder.Services.AddTasksModule(builder.Configuration);
builder.Services.AddUsersModule(builder.Configuration);
builder.Services.AddNotificationsModule(builder.Configuration);

// ┌─────────────────────────────────────────────────────────────────┐
// │ 3. BUILD APP & INITIALIZE DATABASE                              │
// └─────────────────────────────────────────────────────────────────┘

var app = builder.Build();

// Initialize databases and run migrations
await app.InitializeDatabasesAsync();

// ┌─────────────────────────────────────────────────────────────────┐
// │ 4. CONFIGURE HTTP MIDDLEWARE PIPELINE                           │
// └─────────────────────────────────────────────────────────────────┘

app.UseApplicationPipeline(app.Environment);

// ┌─────────────────────────────────────────────────────────────────┐
// │ 5. START APPLICATION                                            │
// └─────────────────────────────────────────────────────────────────┘

app.Run();

public partial class Program { }
