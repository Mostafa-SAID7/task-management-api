# 🎯 Task Management API

[![CI - Build and Test](https://github.com/Mostafa-SAID7/task-management-api/actions/workflows/ci.yml/badge.svg)](https://github.com/Mostafa-SAID7/task-management-api/actions/workflows/ci.yml)
[![Docker - Build and Push](https://github.com/Mostafa-SAID7/task-management-api/actions/workflows/docker.yml/badge.svg)](https://github.com/Mostafa-SAID7/task-management-api/actions/workflows/docker.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

A high-performance **Modular Monolith** API engine for professional project orchestration. Built with **.NET 9** and architected for extreme scalability and maintainability.

---

## 🚀 The Vision

The **Task Management API** is a state-of-the-art solution for complex project tracking. It bridges the gap between simple Todo apps and enterprise-grade Jira-like complexity, offering a "vertical slice" architecture that ensures each module remains isolated, testable, and reliable.

> [!IMPORTANT]
> **Author**: [M.Said](https://m-said-portfolio.netlify.app)  
> **Repository**: [Mostafa-SAID7/task-management-api](https://github.com/Mostafa-SAID7/task-management-api)

---

## ✨ Cinematic Features

| Feature | Description | Status |
| :--- | :--- | :--- |
| **🏗️ Modular Monolith** | Clean separation of Projects, Tasks, Users, and Notifications. | ✅ Ready |
| **✅ Task Blocking** | Advanced dependency logic prevents tasks from closing if blocked. | ✅ Ready |
| **🔐 Secure-by-Design** | JWT-powered Identity with RBAC and secure soft-delete patterns. | ✅ Ready |
| **⚡ Real-time Pulse** | Instant updates and notifications via SignalR integration. | ✅ Ready |
| **📦 Portability** | Dockerized environment with zero-dependency integration tests. | ✅ Ready |

---

## 🛠️ Technology Stack

- **Platform**: .NET 9.0 (Modular Monolith architecture)
- **Data Persistence**: SQL Server + Entity Framework Core 9.0
- **Security & Identity**: ASP.NET Core Identity + JWT Bearer
- **Communication**: SignalR (Real-time events)
- **Quality Assurance**: xUnit + Moq + In-Memory Test Server
- **Infrastructure**: Docker, Docker Compose, GitHub Actions

---

## 📚 Technical Catalog

- 🧱 **[Architecture Deep-Dive](docs/STRUCTURE.md)**: Explore the modular core and vertical slices.
- 📊 **[Relationship Schema](docs/ERD.md)**: Entity Relationship Diagram for the entire domain.
- 🚦 **[Testing Strategy](docs/TESTING.md)**: Our guide to code quality and 100% CI stability.
- 💻 **[Development Portal](docs/DEVELOPMENT.md)**: Standards, setup, and contribution guide.
- 🚀 **[Performance Tune-up](docs/PERFORMANCE.md)**: Optimization tricks for high-load scenarios.

---

## 🚦 Quick Start

### 1. Requirements
*   .NET 9.0 SDK
*   SQL Server (Local or Docker)

### 2. Launching the API
```bash
# Clone the repository
git clone https://github.com/Mostafa-SAID7/task-management-api.git

# Navigate to project
cd task-management-api

# Run the API
dotnet run --project TaskManagementAPI
```

---

## 🤝 Community & Support

*   **Contributing**: Check out our **[CONTRIBUTING.md](.github/CONTRIBUTING.md)**.
*   **Security**: See **[SECURITY.md](.github/SECURITY.md)** for reporting vulnerabilities.
*   **License**: MIT License.

---

## 👤 Credits
**M.Said**  
*Full Stack Developer & Software Architect*  
🌐 **[Portfolio](https://m-said-portfolio.netlify.app)**  
🐙 **[GitHub Profile](https://github.com/Mostafa-SAID7)**
