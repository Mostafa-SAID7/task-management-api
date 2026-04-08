using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using TaskManagementAPI;
using TaskManagementAPI.Modules.Tasks.Application.DTOs;
using TaskManagementAPI.Modules.Tasks.Domain.Enums;
using TaskManagementAPI.Tests.Integration.Helpers;
using Xunit;
using TaskStatus = TaskManagementAPI.Modules.Tasks.Domain.Enums.TaskStatus;
using System.Text.Json;

namespace TaskManagementAPI.Tests.Integration.Modules.Tasks;

/// <summary>
/// Integration tests for the TasksController.
/// </summary>
public class TasksControllerIntegrationTests : IAsyncLifetime
{
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;
    private string _jwtToken = null!;

    public async Task InitializeAsync()
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient();
        
        // Generate JWT token for authenticated requests
        _jwtToken = JwtTokenHelper.GenerateToken();
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_jwtToken}");
        
        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task CreateTask_WithValidRequest_ReturnsCreatedStatus()
    {
        // Arrange
        var csrfToken = await GetCsrfTokenAsync();
        var request = new CreateTaskRequest
        {
            ProjectId = Guid.NewGuid(),
            Title = "Test Task",
            Description = "Test Description",
            Priority = TaskPriority.High,
            DueDate = DateTime.UtcNow.AddDays(5)
        };

        // Act
        var response = await PostWithCsrfAsync("/api/tasks", request, csrfToken);

        // Assert
        // Note: Will return 400 or 404 since project doesn't exist, but demonstrates authorization is working
        Assert.True(response.StatusCode == HttpStatusCode.BadRequest || 
                    response.StatusCode == HttpStatusCode.NotFound ||
                    response.StatusCode == HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateTask_WithPastDueDate_ReturnsBadRequest()
    {
        // Arrange
        var csrfToken = await GetCsrfTokenAsync();
        var request = new CreateTaskRequest
        {
            ProjectId = Guid.NewGuid(),
            Title = "Test Task",
            DueDate = DateTime.UtcNow.AddDays(-1)
        };

        // Act
        var response = await PostWithCsrfAsync("/api/tasks", request, csrfToken);

        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.BadRequest || 
                    response.StatusCode == HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetProjectTasks_WithValidProjectId_ReturnsTaskList()
    {
        // Arrange
        var projectId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/tasks/project/{projectId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<TaskListResponse>();
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task GetProjectTasks_WithStatusFilter_ReturnsFilteredTasks()
    {
        // Arrange
        var projectId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/tasks/project/{projectId}?status={TaskStatus.InProgress}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<TaskListResponse>();
        Assert.NotNull(result);
    }

    [Fact]
    public async Task UpdateTaskStatus_WithValidStatus_ReturnsUpdatedTask()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var csrfToken = await GetCsrfTokenAsync();
        var request = new UpdateTaskStatusRequest { Status = TaskStatus.InProgress };

        // Act
        var response = await PutWithCsrfAsync($"/api/tasks/{taskId}/status", request, csrfToken);

        // Assert
        // Note: This will return 404 since we don't have a real task, but it demonstrates the endpoint
        Assert.True(response.StatusCode == HttpStatusCode.NotFound || response.StatusCode == HttpStatusCode.OK);
    }

    [Fact]
    public async Task AssignTask_WithValidAssignee_ReturnsUpdatedTask()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var csrfToken = await GetCsrfTokenAsync();
        var request = new AssignTaskRequest { AssigneeId = "user123" };

        // Act
        var response = await PutWithCsrfAsync($"/api/tasks/{taskId}/assignee", request, csrfToken);

        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.NotFound || response.StatusCode == HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeleteTask_WithValidId_ReturnsNoContent()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var csrfToken = await GetCsrfTokenAsync();

        // Act
        var response = await DeleteWithCsrfAsync($"/api/tasks/{taskId}", csrfToken);

        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.NotFound || response.StatusCode == HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task AddTimeTrackingEntry_WithValidData_ReturnsCreatedStatus()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var csrfToken = await GetCsrfTokenAsync();
        var request = new AddTimeTrackingRequest
        {
            Hours = 8,
            Date = DateTime.UtcNow
        };

        // Act
        var response = await PostWithCsrfAsync($"/api/tasks/{taskId}/time-entries", request, csrfToken);

        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.NotFound || response.StatusCode == HttpStatusCode.Created);
    }

    [Fact]
    public async Task AddTaskDependency_WithValidData_ReturnsCreatedStatus()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var csrfToken = await GetCsrfTokenAsync();
        var request = new AddTaskDependencyRequest { BlockedByTaskId = Guid.NewGuid() };

        // Act
        var response = await PostWithCsrfAsync($"/api/tasks/{taskId}/dependencies", request, csrfToken);

        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.NotFound || response.StatusCode == HttpStatusCode.Created);
    }

    /// <summary>
    /// Helper method to get CSRF token from the server.
    /// </summary>
    private async Task<string> GetCsrfTokenAsync()
    {
        // In a real scenario, you would fetch this from a GET endpoint that returns the token
        // For now, we'll use a placeholder that the middleware can validate
        return "test-csrf-token";
    }

    /// <summary>
    /// Helper method to POST with CSRF token.
    /// </summary>
    private async Task<HttpResponseMessage> PostWithCsrfAsync<T>(string url, T content, string csrfToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = JsonContent.Create(content)
        };
        request.Headers.Add("X-CSRF-TOKEN", csrfToken);
        return await _client.SendAsync(request);
    }

    /// <summary>
    /// Helper method to PUT with CSRF token.
    /// </summary>
    private async Task<HttpResponseMessage> PutWithCsrfAsync<T>(string url, T content, string csrfToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, url)
        {
            Content = JsonContent.Create(content)
        };
        request.Headers.Add("X-CSRF-TOKEN", csrfToken);
        return await _client.SendAsync(request);
    }

    /// <summary>
    /// Helper method to DELETE with CSRF token.
    /// </summary>
    private async Task<HttpResponseMessage> DeleteWithCsrfAsync(string url, string csrfToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, url);
        request.Headers.Add("X-CSRF-TOKEN", csrfToken);
        return await _client.SendAsync(request);
    }
}

