# 🚀 Task Management API - Setup Guide

## Local Development Setup

### Prerequisites
- **.NET 9.0 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/9.0)
- **PostgreSQL 16+** - [Download](https://www.postgresql.org/download/) or use Docker
- **Git** - Version control

### Quick Start (Development)

#### 1. Clone Repository
```bash
git clone https://github.com/Mostafa-SAID7/task-management-api.git
cd task-management-api
```

#### 2. Configure Environment
Copy `.env.example` to `.env` and update values:
```bash
cp .env.example .env
```

Edit `.env` with your local database credentials:
```
ConnectionStrings__DefaultConnection=Host=localhost;Port=5432;Database=taskmanagementdb;Username=postgres;Password=postgres;SSL Mode=Disable
Jwt__Key=your-super-secret-key-minimum-32-characters
```

#### 3. Restore Dependencies
```bash
cd TaskManagementAPI
dotnet restore
```

#### 4. Build Project
```bash
dotnet build
```

#### 5. Run Database Migrations
The app automatically runs migrations on startup. Ensure PostgreSQL is running.

#### 6. Start API
```bash
dotnet run --launch-profile http
```

The API will be available at: **http://localhost:5000**
Swagger UI: **http://localhost:5000/swagger**

---

## Docker Setup (Recommended for Development)

### Prerequisites
- **Docker** - [Download](https://www.docker.com/products/docker-desktop)
- **Docker Compose** - Included with Docker Desktop

### Run with Docker Compose (Development)
```bash
docker-compose up -d
```

This starts:
- **PostgreSQL** on port `5432`
- **Task Management API** on port `5000`
- **pgAdmin** (optional) on port `5050`

### Access Services
- API: http://localhost:5000
- Swagger: http://localhost:5000/swagger
- pgAdmin: http://localhost:5050 (Email: admin@taskmanagement.local, Password: admin)

### Stop Services
```bash
docker-compose down
```

### Remove Volumes (Reset Database)
```bash
docker-compose down -v
```

---

## Production Deployment

### Using Docker Compose (Production)

#### 1. Create `.env.prod` file
```bash
cp .env.example .env.prod
```

Edit `.env.prod` with production values:
```
ASPNETCORE_ENVIRONMENT=Production
DB_USER=prod_user
DB_PASSWORD=your-secure-password-32-chars-min
JWT_KEY=your-production-jwt-key-32-chars-min
```

#### 2. Deploy with Production Compose
```bash
docker-compose -f docker-compose.prod.yml --env-file .env.prod up -d
```

### Production Configuration

**appsettings.Production.json** includes:
- SSL Mode: **Required** for database connections
- Logging Level: **Warning** (reduced verbosity)
- Swagger: **Disabled** (security best practice)
- JWT expiration: Configurable
- Connection pooling: Optimized for high load

---

## Configuration Files

### appsettings.json (Base)
Default configuration applied to all environments.

### appsettings.Development.json
Development-specific settings (Debug logging, Swagger enabled).

### appsettings.Production.json
Production-specific settings (Warning logging, Swagger disabled, SSL required).

---

## Environment Variables (Docker)

### Development
```
ASPNETCORE_ENVIRONMENT=Development
ConnectionStrings__DefaultConnection=Host=postgres;Port=5432;Database=taskmanagementdb;Username=postgres;Password=postgres;SSL Mode=Disable
Jwt__Key=dev-key-32-characters
```

### Production
```
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=Host=postgres;Port=5432;Database=taskmanagementdb;Username=PROD_USER;Password=PROD_PASSWORD;SSL Mode=Require
Jwt__Key=prod-key-32-characters-minimum
```

---

## Common Issues & Solutions

### Issue: Database Connection Failed
**Solution:**
1. Verify PostgreSQL is running: `psql -U postgres`
2. Check connection string in `appsettings.json`
3. For Docker, ensure postgres service is healthy: `docker-compose ps`

### Issue: Port 5000 Already in Use
**Solution:**
```bash
# Change port in docker-compose.yml:
# ports:
#   - "5001:80"  # Changed from 5000:80
```

### Issue: Swagger Not Loading
**Solution:**
1. Verify app is running: `curl http://localhost:5000/health`
2. Check ASPNETCORE_ENVIRONMENT is not Production
3. Clear browser cache and hard refresh

### Issue: JWT Authentication Fails
**Solution:**
1. Ensure Jwt__Key is set (minimum 32 characters)
2. Check token expiration time
3. Verify Authorization header format: `Bearer <token>`

---

## Replit Deployment

### 1. Create Replit Fork
- Go to [GitHub repo](https://github.com/Mostafa-SAID7/task-management-api)
- Click "Fork" and select Replit

### 2. Configure .replit (Already Setup)
The `.replit` file includes:
- PostgreSQL module
- .NET 9.0 support
- Automatic restore → build → run workflow
- Port mapping (5000→80)

### 3. Click "Run" Button
The Replit environment will:
1. Restore dependencies
2. Build the project
3. Start the API on http://0.0.0.0:5000

### 4. Access API
- **Public URL**: Provided by Replit (e.g., https://task-api-prod.replit.dev)
- **Swagger**: Append `/swagger` to the URL

---

## Testing

### Run Unit Tests
```bash
dotnet test
```

### Run Integration Tests
```bash
dotnet test --filter "Category=Integration"
```

---

## Database Migrations

### Create New Migration
```bash
dotnet ef migrations add MigrationName --project TaskManagementAPI
```

### Apply Migrations Manually
```bash
dotnet ef database update --project TaskManagementAPI
```

### View Migration Status
```bash
dotnet ef migrations list --project TaskManagementAPI
```

---

## Performance Tuning

### Enable Connection Pooling
Connection pooling is configured in `Program.cs`. For production:
```csharp
optionsBuilder.UseNpgsql(connectionString, 
    opts => opts.CommandTimeout(30));
```

### Log File Rotation
Logs are automatically rotated daily (14 days retention for app logs, 90 days for audit).

---

## Security Checklist

- [ ] Change JWT Key in production
- [ ] Use SSL Mode=Require for production databases
- [ ] Disable Swagger in production
- [ ] Set strong database passwords
- [ ] Use HTTPS in production
- [ ] Implement rate limiting (if needed)
- [ ] Keep .env files out of version control
- [ ] Rotate secrets regularly

---

## Troubleshooting

### Enable Debug Logging
Set in `appsettings.Development.json`:
```json
"Logging": {
  "LogLevel": {
    "Default": "Debug"
  }
}
```

### Check Health Endpoint
```bash
curl http://localhost:5000/health
```

### View Docker Logs
```bash
docker-compose logs -f api
```

### Database Connection String Format
```
Host=<hostname>;Port=5432;Database=<dbname>;Username=<user>;Password=<pass>;SSL Mode=Disable
```

---

## Support & Documentation

- **README**: Project overview and features
- **DEVELOPMENT.md**: Development standards and guidelines
- **TESTING.md**: Testing strategy and examples
- **PERFORMANCE.md**: Performance optimization tips
- **GitHub Issues**: Report bugs and request features

---

## Quick Commands Reference

```bash
# Development
dotnet run --launch-profile http
dotnet watch run

# Docker
docker-compose up -d
docker-compose down
docker-compose logs -f api

# Testing
dotnet test
dotnet test --watch

# Database
dotnet ef database update
dotnet ef migrations add <name>
dotnet ef migrations remove
```

---

**Last Updated**: June 2026  
**Framework**: ASP.NET Core 9.0  
**Database**: PostgreSQL 16+
