# 📊 Entity Relationship Diagram (ERD)

This document provides a visual representation of the Domain Models and their relationships within the **Task Management API**.

---

## 🏗️ Domain Model Map

```mermaid
erDiagram
    APPLICATION_USER ||--o{ PROJECT_MEMBER : "is member of"
    APPLICATION_USER ||--o{ WORK_TASK : "is assigned to"
    APPLICATION_USER ||--o{ TIME_TRACKING_ENTRY : "logs time"
    APPLICATION_USER ||--o{ NOTIFICATION : "receives"

    PROJECT ||--o{ PROJECT_MEMBER : "contains"
    PROJECT ||--o{ WORK_TASK : "contains"

    WORK_TASK ||--o{ TASK_DEPENDENCY : "is blocked by"
    WORK_TASK ||--o{ TASK_DEPENDENCY : "blocks"
    WORK_TASK ||--o{ TIME_TRACKING_ENTRY : "tracked by"

    PROJECT {
        Guid Id
        string Name
        string Slug
        string Description
        int Status
    }

    PROJECT_MEMBER {
        Guid Id
        Guid ProjectId
        string UserId
        int Role
        DateTime JoinedAt
    }

    WORK_TASK {
        Guid Id
        Guid ProjectId
        string Title
        string Slug
        string Description
        int Status
        int Priority
        string AssigneeId
        DateTime DueDate
    }

    TASK_DEPENDENCY {
        Guid Id
        Guid TaskId
        Guid BlockedByTaskId
    }

    TIME_TRACKING_ENTRY {
        Guid Id
        Guid TaskId
        string UserId
        decimal Hours
        DateTime Date
    }

    NOTIFICATION {
        Guid Id
        string UserId
        string Message
        int Type
        bool IsRead
        DateTime CreatedAt
    }

    APPLICATION_USER {
        string Id
        string UserName
        string Email
        string FullName
    }
```

---

## 🧩 Relationship Descriptions

### 1. Project & Members
A **Project** has a one-to-many relationship with **ProjectMember**. This acts as a junction between the identity system and our domain modules, allowing us to assign roles (Owner, Manager, Developer) per project.

### 2. Projects & Tasks
A **Project** contains multiple **WorkTask** entities. Tasks are strictly scoped to a single project and cannot exist independently.

### 3. Task Dependencies (Self-Referencing)
A **WorkTask** can have multiple **TaskDependency** entries. This enables complex blocking logic:
- **BlockedBy**: This task waits for another task.
- **Blocking**: This task prevents another task from starting.

### 4. Time Tracking
Users can log time against specific tasks. A **TimeTrackingEntry** connects a **WorkTask** with an **ApplicationUser** via their `UserId`.

### 5. Identity Integration
The **ApplicationUser** entity (extending `IdentityUser`) is the central anchor for all actor-based relationships across the system.
