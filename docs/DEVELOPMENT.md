# 🛠️ Development Portal - Task Management API

This guide defines our engineering standards, development workflows, and the modular blueprint for the **Task Management API**.

---

## 🏛️ Engineering Philosophy

We adhere to the **Modular Monolith** pattern, combining the simplicity of a single deployment unit with the scalability and isolation of independent modules.

### Core Pillars
- **Strict Module Isolation**: No direct database cross-talk between modules.
- **Service-Based Communication**: Cross-module interactions happen via well-defined infrastructure services.
- **Vertical Slice Architecture**: Features are organized by business capabilities, not technical layers.

---

## 🏗️ Feature Development Workflow

When adding a new feature or extending a module, follow this 7-step protocol:

### 1. Domain Modeling
Define your entity in the module's `Domain/Entities` folder. Inherit from `BaseEntity` for automatic timestamp and soft-delete features.

### 2. Contract Definition
Create **DTOs** (Data Transfer Objects) in the `Application/DTOs` folder to define the external API shape.

### 3. Repository Implementation
Implement the module-specific repository extending `GenericRepository<T>`. Register its interface in the module's DI configuration.

### 4. Service Logic
Encapsulate business rules in an application service. This layer orchestrates repositories, validation, and cross-module notifications.

### 5. API Presentation
Create a controller in the module's `Presentation/Controllers` folder. Use the `[Authorize]` attribute to enforce security by default.

### 6. Dependency Registration
Always add a `ModuleExtensions.cs` class to handle the module's wiring, ensuring a clean `Program.cs`.

### 7. Automated Verification
Every feature must include:
- **Unit Tests**: For isolated service logic.
- **Integration Tests**: For controller-to-database flows.

---

## 🚦 Growth Command Center

### Quality Checks
```powershell
# Format codebase to standards
dotnet format

# Run full project audit
dotnet build /p:EnforceCodeStyleInBuild=true
```

### Database Lifecycle
```powershell
# Create a new atomic migration
dotnet ef migrations add <Name> --project TaskManagementAPI

# Sync local database
dotnet ef database update --project TaskManagementAPI
```

---

## 🤝 Contribution Protocol

### Conventional Commits
We use a structured commit history to automate changelog generation:
- `feat:` (New feature)
- `fix:` (Bug fix)
- `docs:` (Documentation update)
- `refactor:` (Code structural change)
- `test:` (Test addition/update)

### Pull Request Standards
- Maximum 10 files per PR for high-quality reviews.
- 100% test pass rate required for merge.
- Documentation must be updated in sync with code changes.

---

## 👤 Author & Governance
**M.Said**  
*Lead Architect*  
[Portfolio](https://m-said-portfolio.netlify.app) | [GitHub](https://github.com/Mostafa-SAID7)
