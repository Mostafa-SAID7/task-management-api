# Testing Guide - Task Management API

Comprehensive guide for writing and running tests.

## Testing Strategy

### Test Pyramid

```
        /\
       /  \        E2E Tests (Few)
      /____\
     /      \
    /  Unit  \    Integration Tests (Some)
   /  Tests   \
  /____________\
  Unit Tests (Many)
```

### Coverage Goals

- **Overall**: >80%
- **Per Module**: >85%
- **Critical Paths**: 100%

## Unit Tests

### Structure

```csharp
public class ProjectServiceTests
{
    // Arrange - Setup test data and mocks
    private readonly Mock<IProjectRepository> _mockRepository;
    private readonly Mock<INotificationService> _mockNotificationService;
    private readonly ProjectService _service;
    
    public ProjectServiceTests()
    {
        _mockRepository = new Mock<IProjectRepository>();
        _mockNotificationService = new Mock<INotificationService>();
        _service = new ProjectService(_mockRepository.Object, _mockNotificationService.Object);
    }
    
    // Act - Execute the method
    // Assert - Verify the results
}
```

### Test Naming Convention

```csharp
[Fact]
public async Task MethodName_Scenario_ExpectedResult()
{
    // Arrange
    // Act
    // Assert
}
```

### Example Unit Tests

```csharp
[Fact]
public async Task CreateProjectAsync_WithValidName_ReturnsProject()
{
    // Arrange
    var name = "Test Project";
    var description = "Test Description";
    
    // Act
    var result = await _service.CreateProjectAsync(name, description);
    
    // Assert
    Assert.NotNull(result);
    Assert.Equal(name, result.Name);
    Assert.Equal(description, result.Description);
    _mockRepository.Verify(r => r.AddAsync(It.IsAny<Project>()), Times.Once);
}

[Fact]
public async Task CreateProjectAsync_WithEmptyName_ThrowsException()
{
    // Arrange
    var name = string.Empty;
    
    // Act & Assert
    await Assert.ThrowsAsync<ArgumentException>(() => 
        _service.CreateProjectAsync(name));
}

[Fact]
public async Task UpdateProjectAsync_WithNonExistentId_ReturnsNull()
{
    // Arrange
    var projectId = Guid.NewGuid();
    _mockRepository.Setup(r => r.GetByIdAsync(projectId))
        .ReturnsAsync((Project?)null);
    
    // Act
    var result = await _service.UpdateProjectAsync(projectId, "New Name");
    
    // Assert
    Assert.Null(result);
}
```

## Integration Tests

### Setup with Testcontainers

```csharp
public class ProjectRepositoryIntegrationTests : IAsyncLifetime
{
    private readonly MsSqlContainer _container = new MsSqlBuilder().Build();
    private ProjectsDbContext _context = null!;
    private ProjectRepository _repository = null!;
    
    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        
        var options = new DbContextOptionsBuilder<ProjectsDbContext>()
            .UseSqlServer(_container.GetConnectionString())
            .Options;
        
        _context = new ProjectsDbContext(options);
        await _context.Database.MigrateAsync();
        _repository = new ProjectRepository(_context);
    }
    
    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
        await _container.StopAsync();
    }
}
```

### Example Integration Tests

```csharp
[Fact]
public async Task CreateProject_WithValidData_PersistsToDatabase()
{
    // Arrange
    var project = new Project { Name = "Test Project", Status = ProjectStatus.Active };
    
    // Act
    await _repository.AddAsync(project);
    await _context.SaveChangesAsync();
    
    // Assert
    var retrieved = await _repository.GetByIdAsync(project.Id);
    Assert.NotNull(retrieved);
    Assert.Equal("Test Project", retrieved.Name);
}

[Fact]
public async Task SoftDelete_MarksEntityAsDeleted()
{
    // Arrange
    var project = new Project { Name = "Test Project" };
    await _repository.AddAsync(project);
    await _context.SaveChangesAsync();
    
    // Act
    await _repository.DeleteAsync(project.Id);
    await _context.SaveChangesAsync();
    
    // Assert
    var retrieved = await _context.Projects
        .IgnoreQueryFilters()
        .FirstOrDefaultAsync(p => p.Id == project.Id);
    
    Assert.NotNull(retrieved);
    Assert.True(retrieved.IsDeleted);
    Assert.NotNull(retrieved.DeletedAt);
}

[Fact]
public async Task GetByIdAsync_ExcludesSoftDeletedEntities()
{
    // Arrange
    var project = new Project { Name = "Test Project" };
    await _repository.AddAsync(project);
    await _context.SaveChangesAsync();
    
    await _repository.DeleteAsync(project.Id);
    await _context.SaveChangesAsync();
    
    // Act
    var retrieved = await _repository.GetByIdAsync(project.Id);
    
    // Assert
    Assert.Null(retrieved);
}
```

## Controller Tests

### Example Controller Tests

```csharp
public class ProjectsControllerTests
{
    private readonly Mock<ProjectService> _mockService;
    private readonly ProjectsController _controller;
    
    public ProjectsControllerTests()
    {
        _mockService = new Mock<ProjectService>();
        _controller = new ProjectsController(_mockService.Object);
    }
    
    [Fact]
    public async Task CreateProject_WithValidRequest_ReturnsCreatedAtAction()
    {
        // Arrange
        var request = new CreateProjectRequest { Name = "Test" };
        var project = new Project { Id = Guid.NewGuid(), Name = "Test" };
        _mockService.Setup(s => s.CreateProjectAsync(request.Name, request.Description))
            .ReturnsAsync(project);
        
        // Act
        var result = await _controller.CreateProject(request);
        
        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(ProjectsController.GetProject), createdResult.ActionName);
    }
    
    [Fact]
    public async Task CreateProject_WithEmptyName_ReturnsBadRequest()
    {
        // Arrange
        var request = new CreateProjectRequest { Name = "" };
        
        // Act
        var result = await _controller.CreateProject(request);
        
        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }
}
```

## Running Tests

### Run All Tests

```bash
dotnet test
```

### Run Specific Test Class

```bash
dotnet test --filter "FullyQualifiedName~ProjectServiceTests"
```

### Run Specific Test Method

```bash
dotnet test --filter "FullyQualifiedName~ProjectServiceTests.CreateProjectAsync_WithValidName_ReturnsProject"
```

### Run with Verbosity

```bash
dotnet test --verbosity detailed
```

### Run with Coverage

```bash
dotnet test /p:CollectCoverage=true /p:CoverageFormat=opencover
```

### Generate Coverage Report

```bash
reportgenerator -reports:"**/coverage.opencover.xml" -targetdir:"coverage-report"
```

## Test Data Builders

### Builder Pattern

```csharp
public class ProjectBuilder
{
    private Guid _id = Guid.NewGuid();
    private string _name = "Test Project";
    private string? _description = null;
    private ProjectStatus _status = ProjectStatus.Active;
    
    public ProjectBuilder WithName(string name)
    {
        _name = name;
        return this;
    }
    
    public ProjectBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }
    
    public ProjectBuilder WithStatus(ProjectStatus status)
    {
        _status = status;
        return this;
    }
    
    public Project Build()
    {
        return new Project
        {
            Id = _id,
            Name = _name,
            Description = _description,
            Status = _status
        };
    }
}
```

### Usage

```csharp
var project = new ProjectBuilder()
    .WithName("My Project")
    .WithDescription("My Description")
    .WithStatus(ProjectStatus.Active)
    .Build();
```

## Mocking Best Practices

### Mock Setup

```csharp
// Setup return value
_mockRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
    .ReturnsAsync(new Project { Name = "Test" });

// Setup with specific parameter
_mockRepository.Setup(r => r.GetByIdAsync(projectId))
    .ReturnsAsync(project);

// Setup to throw exception
_mockRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
    .ThrowsAsync(new Exception("Database error"));
```

### Verify Calls

```csharp
// Verify called once
_mockRepository.Verify(r => r.AddAsync(It.IsAny<Project>()), Times.Once);

// Verify called specific number of times
_mockRepository.Verify(r => r.AddAsync(It.IsAny<Project>()), Times.Exactly(2));

// Verify never called
_mockRepository.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);

// Verify called with specific parameter
_mockRepository.Verify(r => r.GetByIdAsync(projectId), Times.Once);
```

## Test Organization

### Project Structure

```
tests/
├── TaskManagementAPI.Tests.Unit/
│   ├── Modules/
│   │   ├── Projects/
│   │   │   ├── ProjectServiceTests.cs
│   │   │   └── ProjectRepositoryTests.cs
│   │   ├── Tasks/
│   │   └── Users/
│   └── Shared/
│
├── TaskManagementAPI.Tests.Integration/
│   ├── Modules/
│   │   ├── Projects/
│   │   │   └── ProjectsControllerTests.cs
│   │   ├── Tasks/
│   │   └── Users/
│   ├── Fixtures/
│   │   ├── DatabaseFixture.cs
│   │   └── TestDataBuilder.cs
│   └── Testcontainers/
│
└── TaskManagementAPI.Tests.Common/
    ├── Builders/
    ├── Fixtures/
    └── Extensions/
```

## CI/CD Integration

### GitHub Actions

```yaml
name: Tests

on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v4
    - uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '9.0.x'
    
    - run: dotnet test /p:CollectCoverage=true
    
    - uses: codecov/codecov-action@v4
      with:
        files: ./coverage.opencover.xml
        token: ${{ secrets.CODECOV_TOKEN }}
```

## Performance Testing

### Load Testing with k6

```javascript
import http from 'k6/http';
import { check } from 'k6';

export let options = {
  vus: 10,
  duration: '30s',
};

export default function () {
  let response = http.get('http://localhost:5000/api/projects');
  check(response, {
    'status is 200': (r) => r.status === 200,
    'response time < 500ms': (r) => r.timings.duration < 500,
  });
}
```

## Troubleshooting

### Tests Fail Locally but Pass in CI

- Check .NET version
- Check database connection
- Check environment variables
- Run `dotnet clean` and rebuild

### Slow Tests

- Use mocks instead of real dependencies
- Reduce test data size
- Run tests in parallel: `dotnet test --parallel`

### Flaky Tests

- Avoid time-dependent assertions
- Use fixed test data
- Avoid external dependencies
- Use proper async/await

## Resources

- [xUnit Documentation](https://xunit.net/)
- [Moq Documentation](https://github.com/moq/moq4)
- [Testcontainers](https://www.testcontainers.org/)
- [FluentAssertions](https://fluentassertions.com/)

