# Design Document: Task Management API

## 1. Architecture Overview

### 1.1 Modular Monolith Pattern
The Task Management API is built as a **modular monolith** - a single deployable application organized into independent modules with clear boundaries. This approach provides:
- Easier initial development compared to microservices
- Shared database for transactional consistency
- Clear module boundaries for potential future migration to microservices
- Better team collaboration with module ownership

### 1.2 Core Modules
```
Projects Module    → Project creation, management, team membership
Tasks Module       → Task CRUD, status tracking, dependencies, time tracking
Users Module       → User management, authentication, authorization, roles
Notifications Module → Real-time updates via SignalR, notification persistence
```

### 1.3 Layered Architecture (Per Module)
Each module follows N-layered architecture:
```
Presentation Layer    → API Controllers, HTTP routing
Application Layer     → Commands, Queries, DTOs, Validators, Mappers
Domain Layer          → Entities, Value Objects, Enums, Business Rules
Infrastructure Layer  → DbContext, Repositories, External Services
```

---

## 2. Project Structure

### 2.1 Directory Layout
```
TaskManagementAPI/
├── src/
│   ├── TaskManagementAPI/
│   │   ├── Program.cs                    (minimal, loads module configs)
│   │   ├── Shared/
│   │   │   ├── Domain/
│   │   │   │   ├── BaseEntity.cs         (GUID ID, soft delete)
│   │   │   │   ├── Enums/
│   │   │   │   │   └── (shared enums)
│   │   │   │   └── Interfaces/
│   │   │   │       ├── IRepository.cs
│   │   │   │       ├── IUnitOfWork.cs
│   │   │   │       └── INotificationService.cs
│   │   │   ├── Infrastructure/
│   │   │   │   ├── BaseDbContext.cs
│   │   │   │   ├── Repositories/
│   │   │   │   │   └── GenericRepository.cs
│   │   │   │   └── Middleware/
│   │   │   │       ├── ExceptionHandlingMiddleware.cs
│   │   │   │       └── LoggingMiddleware.cs
│   │   │   └── Application/
│   │   │       ├── DTOs/
│   │   │       └── Mappers/
│   │   │
│   │   ├── Modules/
│   │   │   ├── Projects/
│   │   │   │   ├── Presentation/
│   │   │   │   │   └── Controllers/
│   │   │   │   │       └── ProjectsController.cs
│   │   │   │   ├── Application/
│   │   │   │   │   ├── Commands/
│   │   │   │   │   ├── Queries/
│   │   │   │   │   ├── DTOs/
│   │   │   │   │   └── Validators/
│   │   │   │   ├── Domain/
│   │   │   │   │   ├── Entities/
│   │   │   │   │   │   └── Project.cs
│   │   │   │   │   ├── Enums/
│   │   │   │   │   │   └── ProjectStatus.cs
│   │   │   │   │   └── Specifications/
│   │   │   │   ├── Infrastructure/
│   │   │   │   │   ├── Persistence/
│   │   │   │   │   │   ├── ProjectsDbContext.cs
│   │   │   │   │   │   ├── Configurations/
│   │   │   │   │   │   │   └── ProjectConfiguration.cs
│   │   │   │   │   │   └── Seeds/
│   │   │   │   │   │       ├── ProjectSeeder.cs
│   │   │   │   │   │       └── ProjectMemberSeeder.cs
│   │   │   │   │   └── Services/
│   │   │   │   ├── Configuration/
│   │   │   │   │   ├── projects-config.json
│   │   │   │   │   └── DependencyInjection.cs
│   │   │   │   └── ProjectsModuleExtensions.cs
│   │   │   │
│   │   │   ├── Tasks/
│   │   │   │   ├── (same structure as Projects)
│   │   │   │   ├── Domain/
│   │   │   │   │   ├── Entities/
│   │   │   │   │   │   └── Task.cs
│   │   │   │   │   ├── Enums/
│   │   │   │   │   │   ├── TaskStatus.cs
│   │   │   │   │   │   └── TaskPriority.cs
│   │   │   │   │   └── Specifications/
│   │   │   │   │       └── TaskSpecifications.cs
│   │   │   │   └── Infrastructure/
│   │   │   │       └── Persistence/
│   │   │   │           └── Seeds/
│   │   │   │               ├── TaskSeeder.cs
│   │   │   │               └── TaskDependencySeeder.cs
│   │   │   │
│   │   │   ├── Users/
│   │   │   │   ├── Domain/
│   │   │   │   │   ├── Entities/
│   │   │   │   │   │   └── ApplicationUser.cs
│   │   │   │   │   ├── Enums/
│   │   │   │   │   │   ├── UserRole.cs
│   │   │   │   │   │   └── Permission.cs
│   │   │   │   │   └── Specifications/
│   │   │   │   └── Infrastructure/
│   │   │   │       └── Persistence/
│   │   │   │           └── Seeds/
│   │   │   │               ├── UserSeeder.cs
│   │   │   │               └── UserRoleSeeder.cs
│   │   │   │
│   │   │   └── Notifications/
│   │   │       ├── Presentation/
│   │   │       │   └── Hubs/
│   │   │       │       └── TaskUpdatesHub.cs
│   │   │       ├── Domain/
│   │   │       │   ├── Entities/
│   │   │       │   │   └── Notification.cs
│   │   │       │   ├── Enums/
│   │   │       │   │   ├── NotificationType.cs
│   │   │       │   │   └── NotificationStatus.cs
│   │   │       │   └── Specifications/
│   │   │       └── Infrastructure/
│   │   │           └── Persistence/
│   │   │               └── Seeds/
│   │   │                   └── NotificationSeeder.cs
│   │   │
│   │   └── Configuration/
│   │       ├── appsettings.json
│   │       ├── appsettings.Development.json
│   │       ├── appsettings.Production.json
│   │       └── (module-specific configs loaded by Program.cs)
│   │
│   └── (other projects)
│
├── tests/
│   ├── TaskManagementAPI.Tests.Unit/
│   │   ├── Modules/
│   │   │   ├── Projects/
│   │   │   ├── Tasks/
│   │   │   ├── Users/
│   │   │   └── Notifications/
│   │   └── Shared/
│   │
│   ├── TaskManagementAPI.Tests.Integration/
│   │   ├── Modules/
│   │   │   ├── Projects/
│   │   │   ├── Tasks/
│   │   │   ├── Users/
│   │   │   └── Notifications/
│   │   ├── Fixtures/
│   │   │   ├── DatabaseFixture.cs
│   │   │   └── TestDataBuilder.cs
│   │   └── Testcontainers/
│   │       └── SqlServerContainer.cs
│   │
│   └── TaskManagementAPI.Tests.Common/
│       ├── Builders/
│       ├── Fixtures/
│       └── Extensions/
│
└── docs/
    ├── ADR/
    │   ├── 001-modular-monolith-architecture.md
    │   ├── 002-separate-dbcontext-per-module.md
    │   ├── 003-soft-delete-implementation.md
    │   └── 004-signalr-real-time-updates.md
    ├── API.md
    ├── CONTRIBUTING.md
    ├── CHANGELOG.md
    └── README.md
```

---

## 3. Shared Domain Layer

### 3.1 BaseEntity
```csharp
public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
}
```

### 3.2 Soft Delete Implementation
- All entities inherit from BaseEntity
- EF Core query filters exclude soft-deleted entities by default
- `IsDeleted` and `DeletedAt` tracked automatically
- Restoration supported by setting `IsDeleted = false` and `DeletedAt = null`
- Hard deletion after 90 days (configurable) via scheduled job

### 3.3 Shared Interfaces
```csharp
public interface IRepository<T> where T : BaseEntity
{
    Task<T> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(Guid id);
}

public interface IUnitOfWork : IDisposable
{
    IRepository<T> Repository<T>() where T : BaseEntity;
    Task<int> SaveChangesAsync();
}

public interface INotificationService
{
    Task NotifyAsync(string userId, string message);
    Task BroadcastAsync(string groupName, string message);
}
```

---

## 4. Module Structure

### 4.1 Projects Module

**Entities:**
- `Project`: Id, Name, Description, CreatedAt, UpdatedAt, IsDeleted, DeletedAt
- `ProjectMember`: ProjectId, UserId, Role, JoinedAt

**Enums:**
- `ProjectStatus.cs`: Active, Archived, Deleted
- `ProjectMemberRole.cs`: Owner, Manager, Developer, Viewer

**DbContext:**
- `ProjectsDbContext`: Manages Project and ProjectMember entities
- Fluent API configurations in `Configurations/` folder

**Seeders:**
- `ProjectSeeder.cs`: Seeds initial projects
- `ProjectMemberSeeder.cs`: Seeds project memberships

**API Endpoints:**
- `POST /api/projects` - Create project
- `GET /api/projects/{id}` - Get project
- `PUT /api/projects/{id}` - Update project
- `DELETE /api/projects/{id}` - Soft delete project
- `GET /api/projects` - List projects (paginated, filtered)
- `POST /api/projects/{id}/members` - Add team member
- `DELETE /api/projects/{id}/members/{userId}` - Remove team member

### 4.2 Tasks Module

**Entities:**
- `Task`: Id, ProjectId, Title, Description, Status, Priority, AssigneeId, DueDate, CreatedAt, UpdatedAt, IsDeleted, DeletedAt
- `TaskDependency`: TaskId, BlockedByTaskId
- `TimeTrackingEntry`: TaskId, UserId, Hours, Date

**Enums:**
- `TaskStatus.cs`: New, InProgress, Completed, Blocked, Cancelled
- `TaskPriority.cs`: Low, Medium, High, Critical

**DbContext:**
- `TasksDbContext`: Manages Task, TaskDependency, TimeTrackingEntry entities

**Specifications:**
- `TaskSpecifications.cs`: Complex query logic (filtering, sorting, pagination)

**Seeders:**
- `TaskSeeder.cs`: Seeds initial tasks
- `TaskDependencySeeder.cs`: Seeds task dependencies
- `TimeTrackingSeeder.cs`: Seeds time tracking entries

**API Endpoints:**
- `POST /api/tasks` - Create task
- `GET /api/tasks/{id}` - Get task
- `PUT /api/tasks/{id}` - Update task
- `DELETE /api/tasks/{id}` - Soft delete task
- `GET /api/projects/{projectId}/tasks` - List tasks (filtered, paginated)
- `PUT /api/tasks/{id}/status` - Update task status
- `PUT /api/tasks/{id}/assignee` - Assign task
- `POST /api/tasks/{id}/time-entries` - Add time tracking

### 4.3 Users Module

**Entities:**
- `ApplicationUser`: Extends ASP.NET Core Identity IdentityUser
- `UserRole`: Custom role entity
- `Permission`: Permission entity

**Enums:**
- `UserRole.cs`: Admin, Manager, Developer, Viewer
- `Permission.cs`: CreateProject, DeleteProject, CreateTask, DeleteTask, etc.

**DbContext:**
- `UsersDbContext`: Manages ApplicationUser, UserRole, Permission entities
- Integrates with ASP.NET Core Identity

**Seeders:**
- `UserSeeder.cs`: Seeds initial users
- `UserRoleSeeder.cs`: Seeds roles and permissions

**API Endpoints:**
- `POST /api/auth/login` - Authenticate and get JWT token
- `POST /api/auth/register` - Register new user
- `GET /api/users/{id}` - Get user profile
- `PUT /api/users/{id}` - Update user profile
- `PUT /api/users/{id}/password` - Change password
- `POST /api/users/{id}/roles` - Assign role (admin only)
- `GET /api/users` - List users (admin only)

### 4.4 Notifications Module

**Entities:**
- `Notification`: Id, UserId, Type, Message, IsRead, CreatedAt, UpdatedAt, IsDeleted, DeletedAt

**Enums:**
- `NotificationType.cs`: TaskAssigned, TaskCompleted, TaskMentioned, DueDateApproaching, CriticalPriority
- `NotificationStatus.cs`: Unread, Read, Archived

**DbContext:**
- `NotificationsDbContext`: Manages Notification entities

**SignalR Hub:**
- `TaskUpdatesHub`: Handles real-time connections and broadcasts
  - Groups: `project-{projectId}`, `user-{userId}`
  - Methods: `SendTaskUpdate()`, `SendProjectUpdate()`, `SendNotification()`

**Seeders:**
- `NotificationSeeder.cs`: Seeds initial notifications

**API Endpoints:**
- `GET /api/notifications` - Get user notifications (paginated)
- `PUT /api/notifications/{id}/read` - Mark as read
- `DELETE /api/notifications/{id}` - Soft delete notification
- `GET /api/notifications/preferences` - Get notification preferences
- `PUT /api/notifications/preferences` - Update preferences

---

## 5. Configuration Management

### 5.1 Program.cs (Minimal)
```csharp
var builder = WebApplicationBuilder.CreateBuilder(args);

// Load base configuration
builder.Configuration
    .AddJsonFile("appsettings.json")
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

// Load module-specific configurations
var modulesPath = Path.Combine(AppContext.BaseDirectory, "Configuration");
foreach (var moduleConfigFile in Directory.GetFiles(modulesPath, "*-config.json"))
{
    builder.Configuration.AddJsonFile(moduleConfigFile, optional: false, reloadOnChange: true);
}

// Register modules
builder.Services.AddProjectsModule(builder.Configuration);
builder.Services.AddTasksModule(builder.Configuration);
builder.Services.AddUsersModule(builder.Configuration);
builder.Services.AddNotificationsModule(builder.Configuration);

// Add shared services
builder.Services.AddSharedServices(builder.Configuration);

var app = builder.Build();

// Middleware
app.UseExceptionHandlingMiddleware();
app.UseLoggingMiddleware();
app.UseAuthentication();
app.UseAuthorization();

// Map endpoints
app.MapProjectsEndpoints();
app.MapTasksEndpoints();
app.MapUsersEndpoints();
app.MapNotificationsEndpoints();

// SignalR
app.MapHub<TaskUpdatesHub>("/hubs/task-updates");

app.Run();
```

### 5.2 Module Configuration Files
Each module has a configuration file (e.g., `projects-config.json`):
```json
{
  "Projects": {
    "ConnectionString": "Server=localhost;Database=TaskManagementAPI;...",
    "MaxDescriptionLength": 2000,
    "EnableSoftDelete": true,
    "SoftDeleteRetentionDays": 90
  }
}
```

### 5.3 Environment-Specific Configuration
- `appsettings.Development.json`: Local development settings
- `appsettings.Production.json`: Production settings
- Environment variables override all settings

---

## 6. Database Design

### 6.1 Separate DbContext Per Module
Each module has its own DbContext:
- `ProjectsDbContext`: Projects, ProjectMembers
- `TasksDbContext`: Tasks, TaskDependencies, TimeTrackingEntries
- `UsersDbContext`: ApplicationUser, UserRoles, Permissions
- `NotificationsDbContext`: Notifications

**Benefits:**
- Module independence and testability
- Clear data ownership
- Easier migrations per module
- Simpler to mock in tests

### 6.2 Entity Configurations
Each entity has a Fluent API configuration:
```csharp
public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).IsRequired().HasMaxLength(255);
        builder.Property(p => p.Description).HasMaxLength(2000);
        builder.HasQueryFilter(p => !p.IsDeleted);
        builder.HasIndex(p => p.CreatedAt);
    }
}
```

### 6.3 Migrations Strategy
- Migrations per module in separate folders
- `Migrations/Projects/`, `Migrations/Tasks/`, etc.
- Applied during application startup
- Rollback capability per module

### 6.4 Seed Data Loading
```csharp
public interface ISeeder
{
    Task SeedAsync(DbContext context);
}

public class ProjectSeeder : ISeeder
{
    public async Task SeedAsync(DbContext context)
    {
        if (await context.Set<Project>().AnyAsync())
            return; // Already seeded
        
        var projects = new[] { /* seed data */ };
        await context.Set<Project>().AddRangeAsync(projects);
        await context.SaveChangesAsync();
    }
}
```

---

## 7. Authentication & Authorization

### 7.1 ASP.NET Core Identity Integration
- User management via IdentityUser
- Password hashing and validation
- Role management
- Account lockout after 5 failed attempts (30-minute lockout)

### 7.2 JWT Token Generation
- Token expiration: 1 hour
- Claims: UserId, Email, Roles
- Refresh token support (optional)
- Signature validation on each request

### 7.3 Authorization
- Role-based access control (RBAC)
- Policy-based authorization for fine-grained control
- Attribute-based authorization on controllers/actions

```csharp
[Authorize(Roles = "Admin,Manager")]
[HttpPost("projects")]
public async Task<IActionResult> CreateProject(CreateProjectRequest request)
{
    // Implementation
}
```

---

## 8. Real-time Updates (SignalR)

### 8.1 Hub Design
```csharp
public class TaskUpdatesHub : Hub
{
    public async Task JoinProjectGroup(string projectId)
    {
        await Groups.AddToGroupAsync(Connection.ConnectionId, $"project-{projectId}");
    }
    
    public async Task SendTaskUpdate(string projectId, TaskUpdateDto update)
    {
        await Clients.Group($"project-{projectId}").SendAsync("TaskUpdated", update);
    }
}
```

### 8.2 Group Management
- Groups by project: `project-{projectId}`
- Groups by user: `user-{userId}`
- Automatic cleanup on disconnect

### 8.3 Broadcasting Strategy
- Task created/updated/deleted → broadcast to project group
- Project updated → broadcast to project group
- User assigned → send to user group
- Critical priority → notify project manager

---

## 9. Error Handling & Validation

### 9.1 Global Exception Middleware
```csharp
public class ExceptionHandlingMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }
}
```

### 9.2 Error Response Format
```json
{
    "errorCode": "VALIDATION_ERROR",
    "message": "Validation failed",
    "errors": [
        {
            "field": "title",
            "message": "Title is required"
        }
    ],
    "timestamp": "2024-01-15T10:30:00Z",
    "traceId": "0HN1GBDV5SBLF:00000001"
}
```

### 9.3 HTTP Status Codes
- 400: Bad Request (validation errors)
- 401: Unauthorized (invalid/expired token)
- 403: Forbidden (insufficient permissions)
- 404: Not Found (resource doesn't exist)
- 429: Too Many Requests (rate limit exceeded)
- 500: Internal Server Error (unexpected error)

---

## 10. Testing Architecture

### 10.1 Unit Tests
- Test business logic, validators, specifications
- Mock external dependencies
- Target: >85% coverage per module
- Tools: xUnit, Moq, FluentAssertions

### 10.2 Integration Tests
- Test DbContext, repositories, services
- Use Testcontainers for SQL Server
- Test data builders for setup
- Target: Happy path and error scenarios

### 10.3 Test Fixtures
```csharp
public class DatabaseFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _container = new MsSqlBuilder().Build();
    
    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        // Run migrations
    }
    
    public async Task DisposeAsync()
    {
        await _container.StopAsync();
    }
}
```

### 10.4 Coverage Measurement
- Use OpenCover or Coverlet
- Generate coverage reports
- CI/CD pipeline enforces >80% coverage
- Coverage badges in README

---

## 11. Cross-Cutting Concerns

### 11.1 Logging
- Structured logging with Serilog
- Log levels: Debug, Information, Warning, Error, Fatal
- Context: UserId, RequestId, Timestamp
- Sinks: Console, File, Application Insights

### 11.2 Audit Logging
- Log all create/update/delete operations
- Store: UserId, EntityType, EntityId, Action, Changes, Timestamp
- Immutable audit log table
- Retention: Minimum 1 year

### 11.3 Rate Limiting
- Per-user rate limits
- Per-IP rate limits
- Configurable limits per endpoint
- Return 429 Too Many Requests when exceeded

### 11.4 CORS Configuration
- Allow specific origins
- Allow specific methods (GET, POST, PUT, DELETE)
- Allow specific headers
- Support credentials

---

## 12. API Design

### 12.1 RESTful Conventions
- Resource-based URLs: `/api/projects`, `/api/tasks`
- HTTP methods: GET (read), POST (create), PUT (update), DELETE (delete)
- Consistent naming: camelCase for JSON, PascalCase for C#

### 12.2 Pagination
- Query parameters: `pageNumber`, `pageSize`
- Default page size: 20
- Maximum page size: 100
- Response includes: `data`, `totalCount`, `pageNumber`, `pageSize`

### 12.3 Filtering & Sorting
- Query parameters for filtering: `status=InProgress&priority=High`
- Query parameters for sorting: `sortBy=createdAt&sortOrder=desc`
- Support multiple sort fields

### 12.4 DTOs
- Request DTOs: Input validation
- Response DTOs: Output formatting
- Mapping via AutoMapper
- Exclude sensitive data (passwords, tokens)

---

## 13. Implementation Patterns

### 13.1 Repository Pattern
```csharp
public class GenericRepository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly DbContext _context;
    
    public async Task<T> GetByIdAsync(Guid id)
    {
        return await _context.Set<T>().FirstOrDefaultAsync(e => e.Id == id);
    }
    
    public async Task<T> AddAsync(T entity)
    {
        await _context.Set<T>().AddAsync(entity);
        return entity;
    }
}
```

### 13.2 Specification Pattern
```csharp
public class TaskSpecifications
{
    public static IQueryable<Task> GetByProjectAndStatus(
        IQueryable<Task> query, Guid projectId, TaskStatus status)
    {
        return query
            .Where(t => t.ProjectId == projectId && t.Status == status)
            .OrderByDescending(t => t.CreatedAt);
    }
}
```

### 13.3 Service Layer
```csharp
public class ProjectService
{
    private readonly IRepository<Project> _repository;
    private readonly INotificationService _notificationService;
    
    public async Task<ProjectDto> CreateProjectAsync(CreateProjectRequest request)
    {
        var project = new Project { Name = request.Name, Description = request.Description };
        await _repository.AddAsync(project);
        await _notificationService.BroadcastAsync("projects", $"New project: {project.Name}");
        return _mapper.Map<ProjectDto>(project);
    }
}
```

### 13.4 Dependency Injection
```csharp
public static class ProjectsModuleExtensions
{
    public static IServiceCollection AddProjectsModule(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IRepository<Project>, GenericRepository<Project>>();
        services.AddScoped<ProjectService>();
        services.AddScoped<ProjectsDbContext>();
        return services;
    }
}
```

---

## 14. Correctness Properties

This specification focuses on infrastructure, configuration, and CRUD operations. Property-based testing is not applicable. Instead, correctness is ensured through:

1. **Unit Tests**: Specific examples and edge cases
2. **Integration Tests**: End-to-end workflows with real database
3. **Schema Validation**: Configuration validation at startup
4. **Mock-Based Tests**: External dependency behavior
5. **Code Reviews**: SOLID principles and best practices
6. **Static Analysis**: Code quality tools (SonarQube, Roslyn analyzers)

---

## 15. Deployment & DevOps

### 15.1 CI/CD Pipeline (GitHub Actions)
- Run unit tests on every commit
- Run integration tests with Testcontainers
- Generate coverage reports
- Build Docker image
- Deploy to staging/production

### 15.2 Docker Deployment
- Multi-stage Dockerfile
- SQL Server container for database
- Environment-specific configuration
- Health checks

### 15.3 Monitoring & Observability
- Application Insights for monitoring
- Structured logging with Serilog
- Performance metrics
- Error tracking and alerting

---

## 16. Documentation

### 16.1 Architecture Decision Records (ADRs)
- ADR-001: Modular Monolith Architecture
- ADR-002: Separate DbContext Per Module
- ADR-003: Soft Delete Implementation
- ADR-004: SignalR Real-time Updates

### 16.2 API Documentation
- OpenAPI/Swagger specification
- Postman collection
- Endpoint documentation with examples
- Authentication guide

### 16.3 Developer Guide
- Setup instructions
- Running tests
- Code style guide
- Contributing guidelines

---

## 17. Success Criteria

- [ ] All modules follow consistent structure
- [ ] >80% test coverage achieved
- [ ] Zero code duplication
- [ ] All configurations externalized
- [ ] Real-time updates working
- [ ] Documentation complete
- [ ] CI/CD pipeline functional
- [ ] Code review checklist in place
- [ ] Performance benchmarks met
- [ ] Security best practices implemented
