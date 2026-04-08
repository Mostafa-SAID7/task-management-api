# 🚦 Testing Excellence - Task Management API

This guide outlines our comprehensive testing strategy, infrastructure, and best practices to ensure the **Task Management API** remains resilient and reliable.

---

## 🏛️ Testing Strategy

We follow the **Test Pyramid** approach, prioritizing fast, isolated unit tests while maintaining a robust set of integration tests for cross-module verification.

> [!TIP]
> **Portability First**: Our integration tests use an in-memory database strategy, ensuring they run instantly in any environment (Local, CI/CD, Docker) without external dependencies like SQL Server.

### Test Pyramid Overview
```text
        /\
       /  \        End-to-End (E2E) - Critical paths only
      /____\
     /      \      Integration - Horizontal module verification
    /________\
   /          \    Unit Tests - Logic & Domain isolation
  /____________\
```

---

## 🧪 Unit Testing

Unit tests focus on individual components (Services, Helpers, Domain logic) in complete isolation using **xUnit** and **Moq**.

### Pattern: Arrange-Act-Assert (AAA)
```csharp
[Fact]
public async Task CreateProjectAsync_WithValidName_ReturnsProject()
{
    // Arrange
    var name = "Enterprise Cloud Integration";
    _mockRepository.Setup(r => r.AddAsync(It.IsAny<Project>()))
        .ReturnsAsync(new Project { Name = name });

    // Act
    var result = await _service.CreateProjectAsync(name);

    // Assert
    Assert.NotNull(result);
    Assert.Equal(name, result.Name);
}
```

---

## 🔗 Integration Testing

We use a custom `TestWebApplicationFactory` to spin up a fully functional application environment in memory.

### 🛠️ Infrastructure: TestWebApplicationFactory
Our factory automates the following for every test run:
1. **In-Memory Database**: Replaces all module-specific SQL Server contexts with isolated in-memory providers.
2. **Mock Authentication**: Bypasses JWT complexity using a `TestAuthHandler` that provides a stable `test-user-123` identity.
3. **Antiforgery Bypass**: Suppresses CSRF checks to simplify API request verification.
4. **Middleware Hardening**: Disables production-only redirects (like custom 404 pages) to ensure accurate HTTP status reporting.

### Example: Controller Integration
```csharp
public class TasksControllerIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public TasksControllerIntegrationTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateTask_WithValidRequest_ReturnsCreatedStatus()
    {
        // Act
        var response = await _client.PostAsJsonAsync("/api/tasks", new { Title = "New Task" });

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
}
```

---

## 🏃 Running Tests

### Standard Execution
```powershell
# Run all tests in the solution
dotnet test

# Run tests for a specific module
dotnet test --filter "FullyQualifiedName~Tasks"

# Run with detailed output
dotnet test --verbosity normal
```

### Coverage Reporting
We use **FineCodeCoverage** and **ReportGenerator** to track our quality metrics.
- **Target Overall Coverage**: >80%
- **Critical Path Coverage**: 100%

---

## 🛡️ Best Practices

> [!IMPORTANT]
> **Naming**: Use `MethodName_Scenario_ExpectedResult` (e.g., `AddDependency_SelfReference_ThrowsException`).
> **Isolation**: Never let tests depend on each other. Use `Guid.NewGuid()` for test data identifiers.
> **Mocks**: Only mock what you don't own (External APIs, Repositories). Keep service logic real where possible.

---

## 👤 Author & Quality Assurance
**M.Said**  
*Lead Architect & Quality Engineer*  
[Portfolio](https://m-said-portfolio.netlify.app) | [GitHub](https://github.com/Mostafa-SAID7)
