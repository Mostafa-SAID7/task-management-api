# 🎯 Task Management API

[![CI - Build and Test](https://github.com/Mostafa-SAID7/task-management-api/actions/workflows/ci.yml/badge.svg)](https://github.com/Mostafa-SAID7/task-management-api/actions/workflows/ci.yml)
[![Docker - Build and Push](https://github.com/Mostafa-SAID7/task-management-api/actions/workflows/docker.yml/badge.svg)](https://github.com/Mostafa-SAID7/task-management-api/actions/workflows/docker.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

A power-packed modular monolith API for efficient project and task management, meticulously crafted with **.NET 9** and modern architectural principles by **[M.Said](https://m-said-portfolio.netlify.app)**.

---

## 🚀 Vision
The Task Management API is engineered for professional-grade project tracking, featuring high scalability, real-time collaboration, and a robust security model. By leveraging a **Modular Monolith** approach with **Vertical Slice Architecture**, we ensure that each feature is highly cohesive yet loosely coupled.

## ✨ Key Features
- **🏗️ Project Management**: Complete lifecycle control for project portfolios.
- **✅ Task Intelligence**: Smart task tracking with complex dependencies and blocking logic.
- **🔐 Enterprise Security**: Identity-integrated JWT authentication with RBAC.
- **⚡ Real-time Pulse**: Instant notifications and updates powered by SignalR.
- **♻️ Data Resilience**: Advanced soft-deletion patterns with query-level filtering.
- **📦 Clean Architecture**: Modular separation to prevent architectural decay.

## 🛠️ Technology Stack
- **Core**: ASP.NET Core 9.0 (Modular Monolith)
- **Database**: SQL Server + Entity Framework Core 9.0
- **Auth**: ASP.NET Core Identity & JWT Bearer
- **UX**: SignalR (Real-time), Swagger (API Docs)
- **DevOps**: GitHub Actions, Docker, Docker Compose

## 📚 Technical Documentation
Explore our deep-dive documentation in the `docs/` directory:

- 🏗️ **[System Architecture](docs/STRUCTURE.md)**: Modular design and structural integrity.
- 💻 **[Development Portal](docs/DEVELOPMENT.md)**: Standards, setup, and contribution workflows.
- 🚦 **[Branch & CI Strategy](docs/BRANCH_PROTECTION.md)**: Repository management and quality gates.
- 📊 **[Data Architecture](docs/ERD.md)**: Entity Relationship Diagram.
- 🛡️ **[Security Policy](.github/SECURITY.md)**: Safety and reporting standards.
- 🚀 **[Performance Optimization](docs/PERFORMANCE.md)**: Speed and efficiency guide.
- 📜 **[Release Notes](docs/CHANGELOG.md)**: Project history and semver.

## 🚦 Getting Started

### Prerequisites
- .NET 9.0 SDK
- Docker & Docker Compose (Optional)
- SQL Server (Local or Containerized)

### Quick Setup
1. **Clone the repository**:
   ```bash
   git clone https://github.com/Mostafa-SAID7/task-management-api.git
   ```
2. **Configure Database**: Update strings in `TaskManagementAPI/appsettings.json`.
3. **Run the API**:
   ```bash
   dotnet run --project TaskManagementAPI
   ```

## 🤝 Contributing
Contributions are what make the open-source community an amazing place! Please see our **[CONTRIBUTING.md](.github/CONTRIBUTING.md)** for details on our code of conduct and the process for submitting pull requests.

## 👤 Author
**M.Said**
- Portfolio: [m-said-portfolio.netlify.app](https://m-said-portfolio.netlify.app)
- GitHub: [@Mostafa-SAID7](https://github.com/Mostafa-SAID7)

## 📜 License
Licensed under the [MIT License](LICENSE).
