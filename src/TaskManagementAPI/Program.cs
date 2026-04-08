using Serilog;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.OpenApi.Models;
using TaskManagementAPI.Shared.Infrastructure.Configuration;
using TaskManagementAPI.Shared.Infrastructure.DependencyInjection;
using TaskManagementAPI.Modules.Projects.Configuration;
using TaskManagementAPI.Modules.Tasks.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Load configuration from appsettings and module-specific files
ConfigurationLoader.LoadConfiguration(builder);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthorization();
builder.Services.AddAuthentication();
builder.Services.AddSharedServices();
builder.Services.AddProjectsModule(builder.Configuration);
builder.Services.AddTasksModule(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Task Management API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseSharedMiddleware();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Configure default files and static files
var defaultFilesOptions = new DefaultFilesOptions();
defaultFilesOptions.DefaultFileNames.Clear();
defaultFilesOptions.DefaultFileNames.Add("index.html");
app.UseDefaultFiles(defaultFilesOptions);
app.UseStaticFiles();

// Handle 404 errors for non-API routes
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

app.Run();
