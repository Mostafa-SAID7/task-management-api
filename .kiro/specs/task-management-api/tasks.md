# Implementation Plan: Task Management API

## Overview

This implementation plan breaks down the Task Management API modular monolith into discrete, incremental coding tasks organized by phase. Each task builds on previous work, with testing integrated throughout. The implementation uses C# with .NET 8 and ASP.NET Core, following the architecture defined in the design document.

---

## Phase 1: Foundation & Infrastructure

- [ ] 1.1 Create project structure and solution setup
  - Create solution file and project structure as defined in design
  - Create src/TaskManagementAPI, tests/TaskManagementAPI.Tests.Unit, tests/TaskManagementAPI.Tests.Integration projects
  - Create Shared, Modules, and Configuration directories
  - _Requirements: 8.1, 8.2, 8.3_

- [ ] 1.2 Implement BaseEntity and shared domain interfaces
  - Create BaseEntity abstract class with GUID Id, CreatedAt, UpdatedAt, IsDeleted, DeletedAt properties
  - Create IRepository<T> generic interface with CRUD operations
  - Create IUnitOfWork interface for transaction management
  - Create INotificationService interface for real-time updates
  - _Requirements: 8.3, 8.4, 13.1_

- [ ] 1.3 Create BaseDbContext and generic repository implementation
  - Create BaseDbContext abstract class with soft delete query filters
  - Implement GenericRepository<T> with CRUD operations
  - Configure EF Core to exclude soft-deleted entities by default
  - _Requirements: 8.3, 13.1, 13.2_

- [ ] 1.4 Set up configuration loading mechanism
  - Implement configuration loader in Program.cs to load appsettings.json and environment-specific files
  - Create module-specific configuration file loader
  - Implement configuration validation at startup
  - _Requirements: 15.1, 15.2, 15.3_

- [ ] 1.5 Implement dependency injection setup
  - Create extension methods for registering shared services
  - Set up DI container in Program.cs
  - Create module registration pattern for modular DI
  - _Requirements: 8.10_

- [ ] 1.6 Implement global exception handling middleware
  - Create ExceptionHandlingMiddleware to catch and format exceptions
  - Implement error response format with error codes and validation errors
  - Add request ID tracking for error correlation
  - _Requirements: 12.1, 12.2, 12.3_

- [ ] 1.7 Set up structured logging with Serilog
  - Configure Serilog for console and file output
  - Implement LoggingMiddleware to log HTTP requests/responses
  - Add context information (UserId, RequestId, Timestamp)
  - _Requirements: 11.1, 11.2_

- [ ]* 1.8 Write unit tests for shared infrastructure
  - Test BaseEntity functionality
  - Test GenericRepository CRUD operations with mocked DbContext
  - Test configuration loading and validation
  - _Requirements: 6.1, 6.7_

---

## Phase 2: Projects Module

- [ ] 2.1 Create Project entity and enums
  - Create Project entity inheriting from BaseEntity with Name, Description properties
  - Create ProjectStatus enum (Active, Archived, Deleted)
  - Create ProjectMemberRole enum (Owner, Manager, Developer, Viewer)
  - Create ProjectMember entity with ProjectId, UserId, Role, JoinedAt
  - _Requirements: 1.1, 1.6, 8.3_

- [ ] 2.2 Set up ProjectsDbContext and entity configurations
  - Create ProjectsDbContext inheriting from BaseDbContext
  - Create ProjectConfiguration with Fluent API (key, required fields, max lengths, indexes)
  - Create ProjectMemberConfiguration with relationships
  - Configure soft delete query filters
  - _Requirements: 8.2, 8.3, 13.1_

- [ ] 2.3 Create project repository and service layer
  - Create IProjectRepository interface extending IRepository<Project>
  - Implement ProjectRepository with custom query methods (GetByIdWithMembers, GetUserProjects)
  - Create ProjectService with CreateProject, UpdateProject, DeleteProject, AddMember, RemoveMember methods
  - _Requirements: 1.1, 1.2, 1.3, 1.4_

- [ ] 2.4 Implement Projects API controllers and endpoints
  - Create ProjectsController with endpoints: POST /api/projects, GET /api/projects/{id}, PUT /api/projects/{id}, DELETE /api/projects/{id}, GET /api/projects
  - Implement pagination and filtering for list endpoint
  - Add authorization checks for Project_Manager role
  - Create request/response DTOs (CreateProjectRequest, UpdateProjectRequest, ProjectDto)
  - _Requirements: 1.1, 1.2, 1.3, 1.4, 1.5, 1.7_

- [ ] 2.5 Implement team member management endpoints
  - Create endpoints: POST /api/projects/{id}/members, DELETE /api/projects/{id}/members/{userId}
  - Implement member role assignment and removal
  - Add validation for member operations
  - _Requirements: 1.6_

- [ ] 2.6 Create project seeders
  - Create ProjectSeeder to seed initial projects
  - Create ProjectMemberSeeder to seed project memberships
  - Implement ISeeder interface for consistent seeding pattern
  - _Requirements: 8.3_

- [ ] 2.7 Integrate Projects module with DI and routing
  - Create ProjectsModuleExtensions with AddProjectsModule method
  - Register ProjectsDbContext, repositories, and services
  - Create MapProjectsEndpoints extension method
  - Register in Program.cs
  - _Requirements: 8.10_

- [ ]* 2.8 Write unit tests for Projects module
  - Test ProjectService methods (create, update, delete, add/remove members)
  - Test validation logic for project creation
  - Test authorization checks
  - Test soft delete functionality
  - _Requirements: 6.1, 6.7, 6.8_

- [ ]* 2.9 Write integration tests for Projects module
  - Test ProjectsController endpoints with real database
  - Test project creation, retrieval, update, deletion workflows
  - Test member management workflows
  - Test pagination and filtering
  - _Requirements: 6.3, 6.8_

---

## Phase 3: Tasks Module

- [ ] 3.1 Create Task entity and enums
  - Create Task entity with ProjectId, Title, Description, Status, Priority, AssigneeId, DueDate properties
  - Create TaskStatus enum (New, InProgress, Completed, Blocked, Cancelled)
  - Create TaskPriority enum (Low, Medium, High, Critical)
  - Create TaskDependency entity with TaskId, BlockedByTaskId
  - Create TimeTrackingEntry entity with TaskId, UserId, Hours, Date
  - _Requirements: 2.1, 2.2, 2.7, 2.11, 2.12_

- [ ] 3.2 Set up TasksDbContext and entity configurations
  - Create TasksDbContext inheriting from BaseDbContext
  - Create TaskConfiguration with Fluent API (relationships, indexes, constraints)
  - Create TaskDependencyConfiguration and TimeTrackingEntryConfiguration
  - Configure soft delete query filters
  - _Requirements: 8.2, 8.3, 13.1_

- [ ] 3.3 Create task specifications for complex queries
  - Create TaskSpecifications class with methods: GetByProjectAndStatus, GetByAssignee, GetBlocked, GetOverdue
  - Implement filtering, sorting, and pagination logic
  - _Requirements: 2.6_

- [ ] 3.4 Create task repository and service layer
  - Create ITaskRepository interface extending IRepository<Task>
  - Implement TaskRepository with custom query methods using specifications
  - Create TaskService with CreateTask, UpdateTask, DeleteTask, AssignTask, UpdateStatus, AddTimeEntry methods
  - Implement task dependency validation (blocking tasks must be complete)
  - _Requirements: 2.1, 2.2, 2.3, 2.4, 2.5, 2.7, 2.11, 2.12_

- [ ] 3.5 Implement Tasks API controllers and endpoints
  - Create TasksController with endpoints: POST /api/tasks, GET /api/tasks/{id}, PUT /api/tasks/{id}, DELETE /api/tasks/{id}
  - Create endpoint: GET /api/projects/{projectId}/tasks with filtering by status, priority, assignee
  - Create endpoints: PUT /api/tasks/{id}/status, PUT /api/tasks/{id}/assignee
  - Create endpoint: POST /api/tasks/{id}/time-entries
  - Implement pagination and sorting
  - Create request/response DTOs
  - _Requirements: 2.1, 2.2, 2.3, 2.4, 2.5, 2.6, 2.7, 2.8, 2.9, 2.10_

- [ ] 3.6 Create task seeders
  - Create TaskSeeder to seed initial tasks
  - Create TaskDependencySeeder to seed task dependencies
  - Create TimeTrackingSeeder to seed time tracking entries
  - _Requirements: 8.3_

- [ ] 3.7 Integrate Tasks module with DI and routing
  - Create TasksModuleExtensions with AddTasksModule method
  - Register TasksDbContext, repositories, and services
  - Create MapTasksEndpoints extension method
  - Register in Program.cs
  - _Requirements: 8.10_

- [ ]* 3.8 Write unit tests for Tasks module
  - Test TaskService methods (create, update, delete, assign, status change)
  - Test task dependency validation
  - Test due date validation (no past dates)
  - Test time tracking entry addition
  - Test soft delete functionality
  - _Requirements: 6.1, 6.7, 6.8_

- [ ]* 3.9 Write integration tests for Tasks module
  - Test TasksController endpoints with real database
  - Test task creation, retrieval, update, deletion workflows
  - Test task assignment and status updates
  - Test filtering, sorting, and pagination
  - Test task dependency blocking logic
  - _Requirements: 6.3, 6.8_

---

## Phase 4: Users Module & Authentication

- [ ] 4.1 Create ApplicationUser entity and enums
  - Create ApplicationUser extending IdentityUser with additional properties
  - Create UserRole enum (Admin, Manager, Developer, Viewer)
  - Create Permission enum with all system permissions
  - Create UserRole entity for role-permission mapping
  - _Requirements: 3.1, 3.4, 3.5, 3.10_

- [ ] 4.2 Set up UsersDbContext with ASP.NET Core Identity
  - Create UsersDbContext extending IdentityDbContext<ApplicationUser>
  - Configure ApplicationUser, IdentityRole, and custom role entities
  - Set up Identity options (password complexity, lockout policy)
  - Configure 5 failed attempts lockout for 30 minutes
  - _Requirements: 3.1, 3.4, 3.8, 3.9_

- [ ] 4.3 Implement JWT authentication
  - Create JwtTokenGenerator service to generate JWT tokens with 1-hour expiration
  - Implement token validation middleware
  - Add JWT bearer authentication to Program.cs
  - Include UserId, Email, Roles in token claims
  - _Requirements: 3.1, 3.2, 3.3_

- [ ] 4.4 Create user service layer
  - Create UserService with CreateUser, UpdateUser, ChangePassword, AssignRole methods
  - Implement password complexity validation (8+ chars, uppercase, lowercase, number, special char)
  - Implement user profile retrieval with roles and assigned projects
  - _Requirements: 3.4, 3.5, 3.6, 3.7_

- [ ] 4.5 Implement Users API controllers and endpoints
  - Create AuthController with endpoints: POST /api/auth/login, POST /api/auth/register
  - Create UsersController with endpoints: GET /api/users/{id}, PUT /api/users/{id}, PUT /api/users/{id}/password
  - Create admin endpoints: POST /api/users/{id}/roles, GET /api/users
  - Implement request/response DTOs
  - Add authorization checks for admin operations
  - _Requirements: 3.1, 3.4, 3.5, 3.6, 3.7, 3.10_

- [ ] 4.6 Create authorization policies
  - Create policy-based authorization for fine-grained access control
  - Implement role-based policies (Admin, Manager, Developer, Viewer)
  - Create custom authorization handlers for resource-level permissions
  - _Requirements: 3.5, 3.10_

- [ ] 4.7 Create user seeders
  - Create UserSeeder to seed initial users with different roles
  - Create UserRoleSeeder to seed roles and permissions
  - _Requirements: 8.3_

- [ ] 4.8 Integrate Users module with DI and routing
  - Create UsersModuleExtensions with AddUsersModule method
  - Register UsersDbContext, Identity, JWT, services
  - Create MapUsersEndpoints extension method
  - Register in Program.cs
  - _Requirements: 8.10_

- [ ]* 4.9 Write unit tests for Users module
  - Test UserService methods (create, update, change password, assign role)
  - Test password complexity validation
  - Test JWT token generation and validation
  - Test authorization policies
  - _Requirements: 6.1, 6.7, 6.8_

- [ ]* 4.10 Write integration tests for Users module
  - Test authentication flow (login, token generation)
  - Test user creation and profile retrieval
  - Test password change workflow
  - Test role assignment and authorization
  - Test account lockout after failed attempts
  - _Requirements: 6.3, 6.8_

---

## Phase 5: Notifications Module

- [ ] 5.1 Create Notification entity and enums
  - Create Notification entity with UserId, Type, Message, IsRead, Status properties
  - Create NotificationType enum (TaskAssigned, TaskCompleted, TaskMentioned, DueDateApproaching, CriticalPriority)
  - Create NotificationStatus enum (Unread, Read, Archived)
  - _Requirements: 5.1, 5.2, 5.3, 5.4, 5.5_

- [ ] 5.2 Set up NotificationsDbContext
  - Create NotificationsDbContext inheriting from BaseDbContext
  - Create NotificationConfiguration with Fluent API
  - Configure soft delete query filters
  - _Requirements: 8.2, 8.3, 13.1_

- [ ] 5.3 Create notification repository and service layer
  - Create INotificationRepository interface extending IRepository<Notification>
  - Implement NotificationRepository with custom query methods (GetUserNotifications, GetUnread)
  - Create NotificationService with CreateNotification, MarkAsRead, DeleteNotification methods
  - _Requirements: 5.1, 5.2, 5.5, 5.6, 5.7_

- [ ] 5.4 Implement Notifications API controllers and endpoints
  - Create NotificationsController with endpoints: GET /api/notifications, PUT /api/notifications/{id}/read, DELETE /api/notifications/{id}
  - Create endpoints: GET /api/notifications/preferences, PUT /api/notifications/preferences
  - Implement pagination for notifications list
  - Create request/response DTOs
  - _Requirements: 5.1, 5.5, 5.6, 5.7, 5.8_

- [ ] 5.5 Implement SignalR hub for real-time updates
  - Create TaskUpdatesHub extending Hub
  - Implement JoinProjectGroup and LeaveProjectGroup methods
  - Implement SendTaskUpdate, SendProjectUpdate, SendNotification methods
  - Implement group management for project-{projectId} and user-{userId} groups
  - Implement connection lifecycle management (OnConnectedAsync, OnDisconnectedAsync)
  - _Requirements: 4.1, 4.2, 4.3, 4.4, 4.5, 4.6_

- [ ] 5.6 Create notification seeders
  - Create NotificationSeeder to seed initial notifications
  - _Requirements: 8.3_

- [ ] 5.7 Integrate Notifications module with DI and routing
  - Create NotificationsModuleExtensions with AddNotificationsModule method
  - Register NotificationsDbContext, repositories, and services
  - Create MapNotificationsEndpoints extension method
  - Register SignalR in Program.cs with MapHub
  - Register in Program.cs
  - _Requirements: 8.10_

- [ ]* 5.8 Write unit tests for Notifications module
  - Test NotificationService methods (create, mark as read, delete)
  - Test notification filtering and pagination
  - Test soft delete functionality
  - _Requirements: 6.1, 6.7, 6.8_

- [ ]* 5.9 Write integration tests for Notifications module
  - Test NotificationsController endpoints with real database
  - Test notification creation, retrieval, and status updates
  - Test SignalR hub connections and group management
  - Test real-time message broadcasting
  - _Requirements: 6.3, 6.8_

---

## Phase 6: Real-time Integration & Broadcasting

- [ ] 6.1 Integrate SignalR broadcasting with Projects module
  - Modify ProjectService to broadcast project updates via SignalR
  - Broadcast project creation, update, and deletion to project group
  - Broadcast team member additions and removals
  - _Requirements: 1.3, 1.4, 4.2, 4.3_

- [ ] 6.2 Integrate SignalR broadcasting with Tasks module
  - Modify TaskService to broadcast task updates via SignalR
  - Broadcast task creation, update, and deletion to project group
  - Broadcast task status changes and assignee changes
  - Send notification to assigned user when task is assigned
  - Notify project manager when task priority is Critical
  - _Requirements: 2.3, 2.4, 2.5, 2.10, 4.2, 4.3, 4.4_

- [ ] 6.3 Implement notification creation on task events
  - Create notifications when user is assigned to task
  - Create notifications when task status changes to Completed
  - Create notifications for due date approaching (within 24 hours)
  - Create notifications for critical priority tasks
  - _Requirements: 5.1, 5.2, 5.3, 5.4_

- [ ] 6.4 Implement connection authorization and filtering
  - Add authorization checks in SignalR hub to verify user access to project groups
  - Prevent unauthorized users from receiving project updates
  - Implement permission-based message filtering
  - _Requirements: 4.6_

- [ ]* 6.5 Write integration tests for real-time broadcasting
  - Test task update broadcasting to project group
  - Test project update broadcasting
  - Test notification delivery to assigned user
  - Test critical priority notification to project manager
  - Test connection authorization and filtering
  - _Requirements: 6.3, 6.8_

---

## Phase 7: Cross-Cutting Concerns

- [ ] 7.1 Implement audit logging
  - Create AuditLog entity with UserId, EntityType, EntityId, Action, Changes, Timestamp
  - Create AuditDbContext for audit logs
  - Implement AuditService to log create/update/delete operations
  - Integrate audit logging into Project, Task, and User services
  - _Requirements: 14.1, 14.2, 14.3, 14.4, 14.5, 14.6_

- [ ] 7.2 Implement rate limiting
  - Create RateLimitingMiddleware to enforce per-user and per-IP limits
  - Configure rate limits per endpoint
  - Return 429 Too Many Requests when limits exceeded
  - _Requirements: 10.3, 12.5_

- [ ] 7.3 Configure CORS
  - Set up CORS middleware in Program.cs
  - Configure allowed origins, methods, and headers
  - Support credentials for SignalR connections
  - _Requirements: 11.4_

- [ ] 7.4 Implement soft delete query filters
  - Ensure all DbContexts apply soft delete filters by default
  - Create helper method to retrieve soft-deleted entities when needed
  - Implement restore functionality for soft-deleted entities
  - _Requirements: 13.1, 13.2, 13.3, 13.4, 13.5, 13.6_

- [ ] 7.5 Implement performance optimization
  - Add database indexes for frequently queried columns
  - Implement query optimization in repositories
  - Add caching for frequently accessed data (projects, user roles)
  - Implement cache invalidation on updates
  - _Requirements: 10.1, 10.2, 10.5, 10.6, 10.7, 10.8_

- [ ]* 7.6 Write unit tests for cross-cutting concerns
  - Test audit logging functionality
  - Test rate limiting middleware
  - Test CORS configuration
  - Test soft delete filters
  - _Requirements: 6.1, 6.7_

---

## Phase 8: Testing & Quality Assurance

- [ ] 8.1 Set up test infrastructure and fixtures
  - Create DatabaseFixture using Testcontainers for SQL Server
  - Create TestDataBuilder for consistent test data creation
  - Create test extensions and helpers
  - _Requirements: 6.3, 6.4_

- [ ] 8.2 Set up code coverage measurement
  - Configure Coverlet for coverage reporting
  - Set up coverage thresholds (>80% overall, >85% per module)
  - Create coverage report generation in CI/CD
  - _Requirements: 6.1, 6.2_

- [ ] 8.3 Create comprehensive integration test suite
  - Write integration tests for all module workflows
  - Test happy path and error scenarios
  - Test authorization and permission checks
  - Test soft delete and restoration
  - _Requirements: 6.3, 6.4, 6.5, 6.6, 6.8_

- [ ] 8.4 Create performance tests
  - Test response times for list endpoints with large datasets
  - Test concurrent SignalR connections
  - Test database query performance
  - _Requirements: 10.1, 10.2, 10.3, 10.4_

- [ ] 8.5 Checkpoint - Ensure all tests pass and coverage meets targets
  - Run full test suite (unit + integration)
  - Verify code coverage >80%
  - Verify all tests pass
  - Ask the user if questions arise.

---

## Phase 9: Documentation & DevOps

- [ ] 9.1 Create Architecture Decision Records (ADRs)
  - Create ADR-001: Modular Monolith Architecture
  - Create ADR-002: Separate DbContext Per Module
  - Create ADR-003: Soft Delete Implementation
  - Create ADR-004: SignalR Real-time Updates
  - _Requirements: 7.1_

- [ ] 9.2 Configure OpenAPI/Swagger
  - Add Swagger/OpenAPI configuration to Program.cs
  - Configure Swagger UI at /swagger/index.html
  - Document all endpoints with request/response schemas
  - Document authentication requirements
  - _Requirements: 7.3_

- [ ] 9.3 Create Postman collection
  - Create Postman collection with all API endpoints
  - Include authentication flow
  - Include example requests and responses
  - _Requirements: 7.3_

- [ ] 9.4 Create comprehensive README
  - Document project overview and architecture
  - Include architecture diagram
  - Document setup instructions
  - Document running tests and development workflow
  - _Requirements: 7.2_

- [ ] 9.5 Create CONTRIBUTING.md
  - Document code style and conventions
  - Document commit message format
  - Document pull request process
  - Document code review checklist
  - _Requirements: 7.5, 11.1_

- [ ] 9.6 Create CHANGELOG.md
  - Document releases, features, bug fixes, and breaking changes
  - _Requirements: 7.7_

- [ ] 9.7 Set up GitHub Actions CI/CD pipeline
  - Create workflow to run unit tests on every commit
  - Create workflow to run integration tests with Testcontainers
  - Generate and report coverage metrics
  - Build Docker image
  - _Requirements: 6.2, 6.3, 6.4, 6.5, 6.6_

- [ ] 9.8 Create Docker setup
  - Create Dockerfile with multi-stage build
  - Create docker-compose.yml with API and SQL Server services
  - Configure environment-specific settings
  - _Requirements: 15.2, 15.3_

---

## Phase 10: Code Review & Mentoring

- [ ] 10.1 Create code review checklist
  - Document SOLID principles checklist
  - Document clean architecture checklist
  - Document testing requirements checklist
  - Document security best practices checklist
  - _Requirements: 11.1, 11.2, 11.3, 11.4, 11.5, 11.6, 11.7_

- [ ] 10.2 Create internal guidelines document
  - Document naming conventions
  - Document folder structure conventions
  - Document DI registration patterns
  - Document error handling patterns
  - _Requirements: 11.1_

- [ ] 10.3 Create SOLID principles examples document
  - Document Single Responsibility Principle with examples
  - Document Open/Closed Principle with examples
  - Document Liskov Substitution Principle with examples
  - Document Interface Segregation Principle with examples
  - Document Dependency Inversion Principle with examples
  - _Requirements: 8.6, 8.7, 8.8, 8.9_

- [ ] 10.4 Create performance optimization guide
  - Document query optimization techniques
  - Document caching strategies
  - Document indexing best practices
  - Document pagination implementation
  - _Requirements: 10.1, 10.2, 10.5, 10.6, 10.7, 10.8_

- [ ] 10.5 Create testing best practices guide
  - Document unit testing patterns
  - Document integration testing patterns
  - Document test data builders
  - Document mocking strategies
  - _Requirements: 6.7, 6.8, 6.9_

- [ ] 10.6 Final checkpoint - Ensure all documentation is complete
  - Verify all ADRs are documented
  - Verify README is comprehensive
  - Verify CONTRIBUTING.md is clear
  - Verify code review checklist is in place
  - Ask the user if questions arise.

---

## Notes

- Tasks marked with `*` are optional and can be skipped for faster MVP delivery
- Each task references specific requirements for traceability
- Checkpoints ensure incremental validation and team alignment
- All code follows C# and .NET 8 conventions
- All tests use xUnit framework with Moq for mocking
- Integration tests use Testcontainers for SQL Server
- All modules follow the vertical slice architecture pattern
- Configuration is externalized and environment-specific
- Real-time updates use SignalR with group-based broadcasting
