# Getting Started - Task Management API

This guide will help you set up and run the Task Management API locally.

## Prerequisites

- .NET 9.0 SDK or later
- SQL Server 2019 or later (or Docker)
- Visual Studio 2022 / VS Code
- Git

## Installation

### Option 1: Docker (Recommended)

The easiest way to get started is using Docker Compose:

```bash
# Clone the repository
git clone https://github.com/Mostafa-SAID7/task-management-api.git
cd task-management-api

# Start the application
docker-compose up -d

# Wait for services to be healthy
docker-compose ps

# View logs
docker-compose logs -f api
```

The API will be available at:
- **API**: http://localhost:5000
- **Swagger UI**: http://localhost:5000/swagger/index.html
- **Database**: localhost:1433 (SQL Server)

### Option 2: Local Development

1. **Clone the repository**
   ```bash
   git clone https://github.com/Mostafa-SAID7/task-management-api.git
   cd task-management-api
   ```

2. **Install SQL Server**
   - Download from [Microsoft SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)
   - Or use Docker: `docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourPassword123!" -p 1433:1433 mcr.microsoft.com/mssql/server:2022-latest`

3. **Configure connection string**
   ```bash
   # Edit TaskManagementAPI/appsettings.Development.json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=TaskManagementAPI;User Id=sa;Password=YourPassword123!;TrustServerCertificate=true;"
     }
   }
   ```

4. **Restore and build**
   ```bash
   cd TaskManagementAPI
   dotnet restore
   dotnet build
   ```

5. **Run migrations and seed data**
   ```bash
   dotnet ef database update
   ```

6. **Start the application**
   ```bash
   dotnet run
   ```

The API will be available at http://localhost:5000

## First Steps

### 1. Access Swagger UI

Open http://localhost:5000/swagger/index.html to explore the API.

### 2. Register a User

```bash
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "Password123!",
    "fullName": "John Doe"
  }'
```

### 3. Login

```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "Password123!"
  }'
```

Response:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresIn": 3600
}
```

### 4. Create a Project

```bash
curl -X POST http://localhost:5000/api/projects \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "My First Project",
    "description": "A test project"
  }'
```

### 5. Create a Task

```bash
curl -X POST http://localhost:5000/api/tasks \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "projectId": "PROJECT_ID",
    "title": "First Task",
    "description": "My first task",
    "priority": "High",
    "dueDate": "2026-12-31"
  }'
```

## Configuration

### Environment Variables

Create a `.env` file in the root directory:

```bash
# Database
ConnectionStrings__DefaultConnection=Server=localhost;Database=TaskManagementAPI;...

# JWT
Jwt__Secret=your-secret-key-here
Jwt__Issuer=TaskManagementAPI
Jwt__Audience=TaskManagementAPI
Jwt__ExpirationMinutes=60

# Logging
Logging__LogLevel__Default=Information
```

### appsettings Files

- `appsettings.json` - Default settings
- `appsettings.Development.json` - Development overrides
- `appsettings.Production.json` - Production settings

## Running Tests

### Unit Tests
```bash
dotnet test tests/TaskManagementAPI.Tests.Unit
```

### Integration Tests
```bash
dotnet test tests/TaskManagementAPI.Tests.Integration
```

### All Tests with Coverage
```bash
dotnet test /p:CollectCoverage=true /p:CoverageFormat=opencover
```

## Troubleshooting

### Database Connection Issues

**Error**: "Cannot connect to database"

**Solution**:
1. Verify SQL Server is running: `docker ps` (if using Docker)
2. Check connection string in `appsettings.Development.json`
3. Ensure database exists: `CREATE DATABASE TaskManagementAPI`

### Port Already in Use

**Error**: "Address already in use"

**Solution**:
```bash
# Change port in appsettings.json
"Kestrel": {
  "Endpoints": {
    "Http": {
      "Url": "http://localhost:5001"
    }
  }
}
```

### Docker Issues

**Error**: "Cannot connect to Docker daemon"

**Solution**:
1. Ensure Docker is running
2. On Windows: Start Docker Desktop
3. On Linux: `sudo systemctl start docker`

## Next Steps

- Check [Development Guide](DEVELOPMENT.md)
- Review [Contributing Guidelines](CONTRIBUTING.md)
- Explore [Architecture Decisions](ADR/)

## Support

- **Issues**: [GitHub Issues](https://github.com/Mostafa-SAID7/task-management-api/issues)
- **Documentation**: [docs/](.)


