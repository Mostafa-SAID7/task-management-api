# 🏗️ Architectural Blueprint - Task Management API

This document provides a high-level overview of the **Modular Monolith** architecture and the physical file structure of the **Task Management API**.

---

## 🏛️ Architectural Pattern: Modular Monolith

We have transitioned from a layered monolith to a **Modular Monolith** pattern. This approach allows us to maintain a clean separation of concerns while keeping the deployment simple.

### Core Principles
1. **Vertical/Modular Isolation**: Each business capability (Projects, Tasks, Users, Notifications) has its own bounded context.
2. **Shared Infrastructure**: Common cross-cutting concerns (Database, Auth, Logging) are located in the `Shared` layer.
3. **Internal API Contracts**: Modules communicate via domain interfaces or shared services, minimizing direct dependencies.

---

## 📁 System Topology

### 1. Root & Configuration
| File | Purpose |
| :--- | :--- |
| `README.md` | Entry point & Vision |
| `TaskManagementAPI.sln` | Main solution file |
| `Dockerfile` | Production container definition |
| `docker-compose.yml` | Multi-container environment orchestration |

### 2. The API Core (`TaskManagementAPI/`)
The main application is divided into **Shared** infrastructure and **Feature Modules**.

#### 🧪 Shared Infrastructure
- `Domain/`: Base entities (`BaseEntity.cs`) and core interfaces.
- `Infrastructure/`: 
    - `BaseDbContext.cs`: Reusable EF Core logic (Soft Delete, Auto-Timestamps).
    - `Middleware/`: Custom error handling and rate limiting.
    - `DependencyInjection/`: Global service registration.

#### 📦 Feature Modules
Each module contains its own **Application**, **Domain**, **Infrastructure**, and **Presentation** layers.
- **Projects Module**: Portfolio and lifecycle management.
- **Tasks Module**: Work tracking, dependencies, and time logging.
- **Users Module**: Identity integration and user profiles.
- **Notifications Module**: Real-time event bus and SignalR hubs.

### 3. Testing Infrastructure (`tests/`)
- **Unit Tests**: Isolated logic verification for services and domain models.
- **Integration Tests**: Horizontal verification using `TestWebApplicationFactory` and in-memory providers.
- **Common**: Shared fixtures and test data builders.

---

## 📊 Summary Statistics
- **Total Modules**: 4
- **Total Source Files**: 90+
- **Documentation Coverage**: 100% of core flows
- **Test Strategy**: Unit + Portable Integration

---

## 🛡️ Structural Integrity
Every file in this project has been organized to prevent **Architectural Decay**. We use `.editorconfig` to enforce strict coding standards and CI/CD pipelines to verify structural correctness on every push.

> [!IMPORTANT]
> **Owner**: [M.Said](https://m-said-portfolio.netlify.app)  
> **Repository Maintenance**: [Mostafa-SAID7](https://github.com/Mostafa-SAID7)
