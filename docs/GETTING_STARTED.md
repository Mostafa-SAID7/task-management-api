# 🚀 Getting Started - Task Management API

Welcome to the **Task Management API**! This guide will walk you through setting up your environment and making your first API calls in minutes.

---

## 🛠️ Prerequisites

Before you begin, ensure you have the following installed on your machine:

| Requirement | Version | Link |
| :--- | :--- | :--- |
| **.NET SDK** | 9.0+ | [Download](https://dotnet.microsoft.com/download/dotnet/9.0) |
| **SQL Server** | 2019+ | [Download](https://www.microsoft.com/sql-server) |
| **Docker** | Latest | [Download](https://www.docker.com/products/docker-desktop) |
| **Git** | Latest | [Download](https://git-scm.com/downloads) |

---

## 🏗️ Installation

### Option 1: Docker Compose (Recommended)
The fastest way to launch the full stack (API + Database).

```bash
# Clone and enter repository
git clone https://github.com/Mostafa-SAID7/task-management-api.git
cd task-management-api

# Spin up the infrastructure
docker-compose up -d --build
```

> [!TIP]
> Use `docker-compose ps` to verify all services are running and healthy.

### Option 2: Local .NET CLI
For active development and debugging.

1. **Clone the repo**: `git clone...`
2. **Setup Database**: Update the `DefaultConnection` string in `TaskManagementAPI/appsettings.Development.json`.
3. **Run Migrations**: 
   ```powershell
   dotnet ef database update --project TaskManagementAPI
   ```
4. **Launch API**:
   ```powershell
   dotnet run --project TaskManagementAPI
   ```

---

## 🚦 Your First Integration

Once the API is running at `http://localhost:5000`, follow these steps to test the end-to-end flow.

### 1. Account Creation
Register a new administrative user to receive your secure credentials.
```bash
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"email": "admin@taskapi.net", "password": "Password123!", "fullName": "Admin User"}'
```

### 2. Authentication
Retrieve your JWT bearer token.
```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email": "admin@taskapi.net", "password": "Password123!"}'
```

### 3. Explore with Swagger
Navigate to **`http://localhost:5000/swagger`** for a premium, interactive API explorer experience.

---

## 🧪 Documentation Vault
Deep-dive into our technical resources:
- 🧱 **[Architecture Guide](STRUCTURE.md)**
- 🚦 **[Testing Strategy](TESTING.md)**
- 📊 **[Relationship Diagram](ERD.md)**

---

## 👤 Support & Author
**M.Said**  
*Lead Architect*  
[Portfolio](https://m-said-portfolio.netlify.app) | [GitHub](https://github.com/Mostafa-SAID7)
