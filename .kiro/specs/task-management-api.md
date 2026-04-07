# Task Management API - Modular Monolith Specification

## Project Overview
A collaborative task management system (simplified Jira) built as a modular monolith using .NET 8, ASP.NET Core, and EF Core with real-time updates via SignalR.

**Key Characteristics:**
- Modular architecture with bounded contexts
- SOLID principles throughout
- >80% test coverage (xUnit + Moq + Testcontainers)
- Real-time updates with SignalR
- JWT + ASP.NET Core Identity authentication
- SQL Server with EF Core
- CI/CD via GitHub Actions
- Comprehensive documentation (ADR, OpenAPI, README)

---

## Architecture Overview

### Modular Structure
```
src/
├── TaskManagementAPI/
│   ├── Program.cs (minimal, loads configs from modules)
│   ├── Shared/
│   │   ├── Domain/
│   │   │   ├── BaseEntity.cs (GUID ID, soft delete)
│   │   │   └── Enums/
│   │   ├── Infrastructure/
│   │   │   └── Persistence/
│   │   └── Middleware/
│   │
│   ├── Modules/
│   │   ├── Projects/
│   │   ├── Tasks/
│   │   ├── Users/
│   │   └── Notifications/
│   │
│   └── Configuration/
│       ├── projects-config.json
│       ├── tasks-config.json
│       ├── users-config.json
│       └── notifications-config.json
│
├── Tests/
│   ├── TaskManagementAPI.Tests.Unit/
│   ├── TaskManagementAPI.Tests.Integration/
│   └── TaskManagementAPI.Tests.Common/
│
└── Documentation/
    ├── ADR/
    ├── OpenAPI/
    └── README.md
```

---

## Core Requirements

### 1. Shared Domain Layer
- **BaseEntity**: GUID ID, CreatedAt, UpdatedAt, IsDeleted (soft delete)
- **Enums**: Separate files per enum (TaskStatus, Priority, UserRole, etc.)
- **Interfaces**: IRepository, IUnitOfWork, INotificationService

### 2. Module Structure (Each Module)
Each module follows this structure:
```
Modules/{ModuleName}/
├── Domain/
│   ├── Entities/
│   ├── ValueObjects/
│   ├── Enums/
│   │   ├── {EnumName}.cs (one per enum)
│   │   └── {EnumName}.cs
│   └── Specifications/
├── Application/
│   ├── Commands/
│   ├── Queries/
│   ├── DTOs/
│   └── Validators/
├── Infrastructure/
│   ├── Persistence/
│   │   ├── {ModuleName}DbContext.cs
│   │   ├── Configurations/
│   │   │   ├── {EntityName}Configuration.cs
│   │   │   └── {EntityName}Configuration.cs
│   │   └── Seeds/
│   │       ├── {EntityName}Seeder.cs
│   │       └── {EntityName}Seeder.cs
│   └── Services/
├── Presentation/
│   └── Controllers/
└── DependencyInjection.cs
```

### 3. Configuration Management
- **Program.cs**: Minimal, loads module configs from `Configuration/` folder
- **Module Config Files**: JSON files per module with:
  - DbContext connection strings
  - Service registrations
  - Feature flags
  - Logging settings

### 4. Database Context Strategy
- **Separate DbContext per module** (ProjectsDbContext, TasksDbContext, etc.)
- **Shared DbContext** for cross-cutting concerns (Users, Audit)
- **Fluent API configurations** in dedicated Configuration classes
- **Migrations per module** in separate folders

### 5. Entity Design
- **BaseEntity**: 
  ```csharp
  public abstract class BaseEntity
  {
      public Guid Id { get; set; } = Guid.NewGuid();
      public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
      public DateTime? UpdatedAt { get; set; }
      public bool IsDeleted { get; set; } = false;
  }
  ```
- **Soft Delete**: All queries filter `IsDeleted == false` by default
- **Audit Trail**: CreatedAt, UpdatedAt tracked automatically

### 6. Enums Organization
- **One file per enum** (e.g., `TaskStatus.cs`, `Priority.cs`, `UserRole.cs`)
- **Located in**: `Modules/{ModuleName}/Domain/Enums/`
- **Shared enums**: `Shared/Domain/Enums/`

### 7. Seeding Strategy
- **One seeder class per entity** (e.g., `ProjectSeeder.cs`, `TaskSeeder.cs`)
- **Located in**: `Modules/{ModuleName}/Infrastructure/Persistence/Seeds/`
- **Interface**: `ISeeder` with `SeedAsync()` method
- **Orchestrated by**: `DatabaseSeeder` in Shared layer
- **Idempotent**: Check existence before seeding

### 8. Real-Time Updates (SignalR)
- **Hub**: `TaskUpdatesHub` for task notifications
- **Events**: Task created, updated, assigned, completed
- **Groups**: By project, by user, by task

### 9. Authentication & Authorization
- **ASP.NET Core Identity** for user management
- **JWT tokens** for API authentication
- **Role-based access control** (RBAC)
- **Policy-based authorization** for fine-grained control

### 10. Testing Strategy
- **Unit Tests**: Business logic, validators, specifications
- **Integration Tests**: DbContext, repositories, services
- **Testcontainers**: SQL Server for integration tests
- **Target**: >80% code coverage
- **Tools**: xUnit, Moq, Testcontainers, FluentAssertions

---

## Implementation Tasks

### Phase 1: Foundation
- [ ] Create base project structure
- [ ] Implement BaseEntity with soft delete
- [ ] Set up Shared domain layer
- [ ] Configure Program.cs for modular loading
- [ ] Create configuration loading mechanism

### Phase 2: Projects Module
- [ ] Define Project entity and enums
- [ ] Create ProjectsDbContext
- [ ] Implement repository pattern
- [ ] Add CRUD operations
- [ ] Create unit & integration tests

### Phase 3: Tasks Module
- [ ] Define Task entity and enums
- [ ] Create TasksDbContext
- [ ] Implement task specifications
- [ ] Add task assignment logic
- [ ] Create unit & integration tests

### Phase 4: Users Module
- [ ] Integrate ASP.NET Core Identity
- [ ] Create User entity extensions
- [ ] Implement JWT authentication
- [ ] Add role management
- [ ] Create unit & integration tests

### Phase 5: Notifications Module
- [ ] Implement SignalR hub
- [ ] Create notification service
- [ ] Add real-time task updates
- [ ] Implement notification persistence
- [ ] Create unit & integration tests

### Phase 6: Cross-Cutting Concerns
- [ ] Global exception handling middleware
- [ ] Logging & monitoring
- [ ] API versioning
- [ ] Rate limiting
- [ ] CORS configuration

### Phase 7: Documentation & DevOps
- [ ] Write ADRs (Architecture Decision Records)
- [ ] Generate OpenAPI/Swagger specs
- [ ] Create Postman collections
- [ ] Write comprehensive README
- [ ] Set up GitHub Actions CI/CD

### Phase 8: Code Quality & Mentoring
- [ ] Code review checklist
- [ ] Internal guidelines document
- [ ] Performance optimization guide
- [ ] SOLID principles examples
- [ ] Testing best practices guide

---

## Key Design Decisions

### Why Modular Monolith?
- Easier to start than microservices
- Shared database for consistency
- Clear module boundaries for future migration
- Better for team collaboration

### Why Separate DbContexts?
- Module independence
- Easier testing (mock per context)
- Clear data ownership
- Simpler migrations per module

### Why One Enum Per File?
- Single Responsibility Principle
- Easier to find and maintain
- Clear versioning if needed
- Better for code reviews

### Why Separate Seeders?
- Each seeder has one responsibility
- Easier to test seeding logic
- Clear dependency injection
- Reusable across environments

---

## Success Criteria
- [ ] All modules follow consistent structure
- [ ] >80% test coverage achieved
- [ ] Zero code duplication
- [ ] All configurations externalized
- [ ] Real-time updates working
- [ ] Documentation complete
- [ ] CI/CD pipeline functional
- [ ] Code review checklist in place

---

## Next Steps
1. Review and approve this specification
2. Create base project structure
3. Implement Phase 1 (Foundation)
4. Proceed with module implementation
