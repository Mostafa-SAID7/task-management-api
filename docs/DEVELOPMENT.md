# Development Guide - Task Management API

This guide covers local development setup and workflows.

## Development Environment Setup

### Prerequisites

- .NET 9.0 SDK
- Visual Studio 2022 or VS Code
- SQL Server 2019+ or Docker
- Git

### Initial Setup

   ```bash
   git clone https://github.com/Mostafa-SAID7/task-management-api.git
   cd task-management-api
   ```

2. **Install dependencies**
   ```bash
   dotnet restore
   ```

3. **Configure database**
   ```bash
   # Copy and edit appsettings.Development.json
   cp TaskManagementAPI/appsettings.json TaskManagementAPI/appsettings.Development.json
   
   # Edit connection string
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=TaskManagementAPI;..."
     }
   }
   ```

4. **Run migrations**
   ```bash
   cd TaskManagementAPI
   dotnet ef database update
   ```

5. **Start application**
   ```bash
   dotnet run
   ```

## Project Structure

```
TaskManagementAPI/
├── Program.cs                 # Application entry point
├── appsettings.json          # Configuration
├── Shared/                   # Shared infrastructure
│   ├── Domain/
│   │   ├── BaseEntity.cs
│   │   └── Interfaces/
│   └── Infrastructure/
│       ├── BaseDbContext.cs
│       ├── Repositories/
│       ├── Middleware/
│       └── DependencyInjection/
└── Modules/                  # Feature modules
    ├── Projects/
    ├── Tasks/
    ├── Users/
    └── Notifications/
```

## Adding a New Feature

### 1. Create Domain Model

```csharp
// Modules/Projects/Domain/Entities/Project.cs
public class Project : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ProjectStatus Status { get; set; }
    public List<ProjectMember> Members { get; set; } = new();
}
```

### 2. Create DTOs

```csharp
// Modules/Projects/Application/DTOs/CreateProjectRequest.cs
public class CreateProjectRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

// Modules/Projects/Application/DTOs/ProjectDto.cs
public class ProjectDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ProjectStatus Status { get; set; }
}
```

### 3. Create Repository

```csharp
// Modules/Projects/Infrastructure/Services/IProjectRepository.cs
public interface IProjectRepository : IRepository<Project>
{
    Task<Project?> GetByIdWithMembersAsync(Guid id);
    Task<IEnumerable<Project>> GetUserProjectsAsync(string userId);
}

// Modules/Projects/Infrastructure/Services/ProjectRepository.cs
public class ProjectRepository : GenericRepository<Project>, IProjectRepository
{
    private readonly ProjectsDbContext _context;
    
    public ProjectRepository(ProjectsDbContext context) : base(context)
    {
        _context = context;
    }
    
    public async Task<Project?> GetByIdWithMembersAsync(Guid id)
    {
        return await _context.Projects
            .Include(p => p.Members)
            .FirstOrDefaultAsync(p => p.Id == id);
    }
}
```

### 4. Create Service

```csharp
// Modules/Projects/Application/Services/ProjectService.cs
public class ProjectService
{
    private readonly IProjectRepository _repository;
    private readonly INotificationService _notificationService;
    
    public ProjectService(
        IProjectRepository repository,
        INotificationService notificationService)
    {
        _repository = repository;
        _notificationService = notificationService;
    }
    
    public async Task<Project> CreateProjectAsync(string name, string? description = null)
    {
        var project = new Project { Name = name, Description = description };
        await _repository.AddAsync(project);
        await _notificationService.BroadcastAsync("projects", $"New project: {name}");
        return project;
    }
}
```

### 5. Create Controller

```csharp
// Modules/Projects/Presentation/Controllers/ProjectsController.cs
[ApiController]
[Route("api/projects")]
[Authorize]
public class ProjectsController : ControllerBase
{
    private readonly ProjectService _service;
    
    public ProjectsController(ProjectService service)
    {
        _service = service;
    }
    
    [HttpPost]
    public async Task<ActionResult<ProjectDto>> CreateProject([FromBody] CreateProjectRequest request)
    {
        var project = await _service.CreateProjectAsync(request.Name, request.Description);
        return CreatedAtAction(nameof(GetProject), new { id = project.Id }, MapToDto(project));
    }
}
```

### 6. Register in DI

```csharp
// Modules/Projects/Configuration/ProjectsModuleExtensions.cs
public static IServiceCollection AddProjectsModule(
    this IServiceCollection services, IConfiguration configuration)
{
    services.AddDbContext<ProjectsDbContext>(options =>
        options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
    
    services.AddScoped<IProjectRepository, ProjectRepository>();
    services.AddScoped<ProjectService>();
    
    return services;
}
```

### 7. Add Tests

```csharp
// tests/TaskManagementAPI.Tests.Unit/Modules/Projects/ProjectServiceTests.cs
public class ProjectServiceTests
{
    [Fact]
    public async Task CreateProjectAsync_WithValidName_ReturnsProject()
    {
        // Arrange
        var repository = new Mock<IProjectRepository>();
        var notificationService = new Mock<INotificationService>();
        var service = new ProjectService(repository.Object, notificationService.Object);
        
        // Act
        var result = await service.CreateProjectAsync("Test Project");
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Project", result.Name);
        repository.Verify(r => r.AddAsync(It.IsAny<Project>()), Times.Once);
    }
}
```

## Database Migrations

### Create Migration

```bash
cd TaskManagementAPI
dotnet ef migrations add AddProjectFeature --project . --startup-project .
```

### Apply Migration

```bash
dotnet ef database update
```

### Revert Migration

```bash
dotnet ef database update PreviousMigration
```

### Remove Migration

```bash
dotnet ef migrations remove
```

## Running Tests

### Unit Tests

```bash
dotnet test tests/TaskManagementAPI.Tests.Unit
```

### Integration Tests

```bash
dotnet test tests/TaskManagementAPI.Tests.Integration
```

### Specific Test

```bash
dotnet test --filter "FullyQualifiedName~ProjectServiceTests"
```

### With Coverage

```bash
dotnet test /p:CollectCoverage=true /p:CoverageFormat=opencover
```

## Debugging

### Visual Studio

1. Set breakpoint
2. Press F5 to start debugging
3. Navigate to endpoint
4. Inspect variables in debug window

### VS Code

1. Install C# extension
2. Create `.vscode/launch.json`:
   ```json
   {
     "version": "0.2.0",
     "configurations": [
       {
         "name": ".NET Core Launch",
         "type": "coreclr",
         "request": "launch",
         "preLaunchTask": "build",
         "program": "${workspaceFolder}/TaskManagementAPI/bin/Debug/net8.0/TaskManagementAPI.dll",
         "args": [],
         "cwd": "${workspaceFolder}/TaskManagementAPI",
         "stopAtEntry": false,
         "console": "internalConsole"
       }
     ]
   }
   ```
3. Press F5 to start debugging

### Command Line

```bash
dotnet run --configuration Debug
```

## Code Quality Tools

### Static Analysis

```bash
# Run code analysis
dotnet build /p:EnforceCodeStyleInBuild=true

# Run with specific rules
dotnet build /p:AnalysisLevel=latest
```

### Code Formatting

```bash
# Format code
dotnet format

# Check formatting
dotnet format --verify-no-changes
```

### Code Coverage

```bash
# Generate coverage report
dotnet test /p:CollectCoverage=true /p:CoverageFormat=opencover

# Generate HTML report
reportgenerator -reports:"**/coverage.opencover.xml" -targetdir:"coverage-report"
```

## Performance Profiling

### Using dotnet-trace

```bash
# Collect trace
dotnet trace collect -p <PID>

# Analyze trace
dotnet trace convert trace.nettrace
```

### Using dotnet-dump

```bash
# Collect dump
dotnet dump collect -p <PID>

# Analyze dump
dotnet dump analyze core.dump
```

## Common Tasks

### Add NuGet Package

```bash
dotnet add TaskManagementAPI package PackageName
```

### Update NuGet Packages

```bash
dotnet list package --outdated
dotnet package update
```

### Clean Build

```bash
dotnet clean
dotnet build
```

### Publish Release

```bash
dotnet publish -c Release -o ./publish
```

## Troubleshooting

### Build Errors

```bash
# Clean and rebuild
dotnet clean
dotnet restore
dotnet build
```

### Database Issues

```bash
# Reset database
dotnet ef database drop
dotnet ef database update
```

### Port Already in Use

```bash
# Change port in appsettings.json
# Or kill process using port
lsof -i :5000
kill -9 <PID>
```

## Commit Messages & Versioning

### Conventional Commits
We enforce [Conventional Commits](https://www.conventionalcommits.org/) to maintain a clean and readable project history:
- `feat`: A new feature
- `fix`: A bug fix
- `docs`: Documentation only changes
- `refactor`: A code change that neither fixes a bug nor adds a feature
- `perf`: A code change that improves performance
- `test`: Adding missing tests or correcting existing tests
- `chore`: Changes to the build process or auxiliary tools

### Semantic Versioning
This project follows [Semantic Versioning (SemVer) 2.0.0](https://semver.org/):
- **MAJOR** version for incompatible API changes
- **MINOR** version for functionality in a backwards compatible manner
- **PATCH** version for backwards compatible bug fixes

---

## Resources

- [.NET Documentation](https://docs.microsoft.com/en-us/dotnet/)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/)
- [xUnit Testing](https://xunit.net/)
- [Moq Mocking](https://github.com/moq/moq4)

