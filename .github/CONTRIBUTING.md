# Contributing to Task Management API

Thank you for your interest in contributing! This document provides guidelines for contributing to the project.

## Code of Conduct

- Be respectful and inclusive
- Provide constructive feedback
- Focus on the code, not the person
- Help others learn and grow

## Getting Started

1. Fork the repository
2. Clone your fork: `git clone https://github.com/Mostafa-SAID7/task-management-api.git`
3. Create a feature branch: `git checkout -b feature/your-feature-name`
4. Make your changes
5. Commit your changes
6. Push to your fork
7. Create a Pull Request

## Development Workflow

### Branch Naming

- `feature/description` - New features
- `bugfix/description` - Bug fixes
- `docs/description` - Documentation updates
- `refactor/description` - Code refactoring

### Commit Messages

Follow the conventional commits format:

```
type(scope): subject

body

footer
```

**Types**:
- `feat` - New feature
- `fix` - Bug fix
- `docs` - Documentation
- `style` - Code style (formatting, missing semicolons, etc.)
- `refactor` - Code refactoring
- `perf` - Performance improvement
- `test` - Adding or updating tests
- `chore` - Build process, dependencies, etc.

**Examples**:
```
feat(projects): add project archiving functionality

Implement soft delete for projects with restoration capability.
Adds new endpoint PUT /api/projects/{id}/archive.

Closes #123
```

```
fix(tasks): resolve task dependency validation

Fix issue where task dependencies were not properly validated
when updating task status.

Fixes #456
```

## Code Standards

### C# Conventions

- Use PascalCase for class names and methods
- Use camelCase for private fields and local variables
- Use meaningful, descriptive names
- Keep methods focused and concise (< 20 lines)
- Use async/await for I/O operations

### Naming Conventions

```csharp
// Classes
public class ProjectService { }

// Methods
public async Task<Project> CreateProjectAsync(string name) { }

// Private fields
private readonly IProjectRepository _projectRepository;

// Local variables
var projectName = "My Project";

// Constants
private const int MaxDescriptionLength = 2000;
```

### Code Organization

```csharp
public class ProjectService
{
    // Fields
    private readonly IProjectRepository _projectRepository;
    
    // Constructor
    public ProjectService(IProjectRepository projectRepository) { }
    
    // Public methods
    public async Task<Project> CreateProjectAsync(string name) { }
    
    // Private methods
    private void ValidateProjectName(string name) { }
}
```

### Documentation

- Add XML comments to all public classes and methods
- Include parameter descriptions
- Include return value descriptions
- Include exception documentation

```csharp
/// <summary>
/// Creates a new project.
/// </summary>
/// <param name="name">The project name.</param>
/// <param name="description">The project description.</param>
/// <returns>The created project.</returns>
/// <exception cref="ArgumentException">Thrown when name is empty.</exception>
public async Task<Project> CreateProjectAsync(string name, string? description = null)
{
    if (string.IsNullOrWhiteSpace(name))
        throw new ArgumentException("Project name is required.", nameof(name));
    
    // Implementation
}
```

## Testing Requirements

### Unit Tests

- Test all public methods
- Test happy path and error scenarios
- Use meaningful test names
- Keep tests focused and isolated

```csharp
[Fact]
public async Task CreateProjectAsync_WithValidName_ReturnsProject()
{
    // Arrange
    var repository = new Mock<IProjectRepository>();
    var service = new ProjectService(repository.Object);
    
    // Act
    var result = await service.CreateProjectAsync("Test Project");
    
    // Assert
    Assert.NotNull(result);
    Assert.Equal("Test Project", result.Name);
}
```

### Integration Tests

- Test end-to-end workflows
- Use real database (Testcontainers)
- Test authorization and permissions
- Test error scenarios

### Coverage Requirements

- Minimum 80% overall coverage
- Minimum 85% per module
- Critical paths must have 100% coverage

## Pull Request Process

1. **Update documentation** - Update README, docs, or comments as needed
2. **Add tests** - Include unit and integration tests
3. **Run tests locally** - Ensure all tests pass
4. **Create PR** - Provide clear description of changes
5. **Address feedback** - Respond to code review comments
6. **Merge** - Maintainer will merge after approval

### PR Description Template

```markdown
## Description
Brief description of changes

## Type of Change
- [ ] Bug fix
- [ ] New feature
- [ ] Breaking change
- [ ] Documentation update

## Related Issues
Closes #123

## Testing
- [ ] Unit tests added
- [ ] Integration tests added
- [ ] Manual testing completed

## Checklist
- [ ] Code follows style guidelines
- [ ] Documentation updated
- [ ] Tests pass locally
- [ ] No new warnings generated
```

## Code Review Checklist

Reviewers will check:

- [ ] Code follows conventions
- [ ] Tests are included and pass
- [ ] Documentation is updated
- [ ] No code duplication
- [ ] SOLID principles followed
- [ ] Performance implications considered
- [ ] Security best practices applied
- [ ] Error handling is appropriate

## Architecture Guidelines

### Vertical Slice Pattern

Each module should follow:
```
Presentation → Application → Domain → Infrastructure
```

### Single Responsibility Principle

- Controllers: HTTP concerns only
- Services: Business logic only
- Repositories: Data access only
- DbContext: Entity configuration only

### Dependency Injection

- Use constructor injection
- Depend on abstractions (interfaces)
- Register in module extensions

## Performance Considerations

- Use async/await for I/O operations
- Implement pagination for large result sets
- Add database indexes for frequently queried columns
- Consider caching for frequently accessed data
- Profile before optimizing

## Security Best Practices

- Validate all input
- Use parameterized queries
- Hash passwords with Identity
- Implement authorization checks
- Log security events
- Don't expose sensitive data in errors

## Documentation

- Update README for new features
- Add/update API documentation
- Create ADR for architectural decisions
- Update CHANGELOG
- Add inline code comments for complex logic

## Release Process

1. Update version in `csproj` files
2. Update CHANGELOG.md
3. Create release branch: `release/v1.0.0`
4. Create PR and merge to main
5. Tag release: `git tag v1.0.0`
6. Push tag: `git push origin v1.0.0`
7. Create GitHub release with notes

## Questions?

- Check [GETTING_STARTED.md](GETTING_STARTED.md)
- Review [DEVELOPMENT.md](DEVELOPMENT.md)
- Open an issue for discussion

Thank you for contributing!

