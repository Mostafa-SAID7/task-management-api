# 🚀 Quick Start - Task Management API

## Start API in 2 Minutes

### Option 1: Docker (Recommended)
```bash
# Navigate to project root
cd task-management-api

# Start services
docker-compose up -d

# Open Swagger UI
# http://localhost:5000/swagger
```

### Option 2: Direct .NET (Fastest)
```bash
cd TaskManagementAPI
dotnet run --launch-profile http
```

Both start the API on **http://0.0.0.0:5000**

---

## 📚 Access Swagger UI

Once running, open your browser:
```
http://localhost:5000/swagger
```

### What You'll See
- ✅ API endpoints for Projects, Tasks, Users, Notifications
- ✅ Request/Response schemas
- ✅ Try it out buttons for each endpoint
- ✅ JWT authentication configuration

---

## 🔑 Authenticate with JWT

### 1. Get Token (Login or Register)
Find the Users endpoint → `/api/users/auth` → Try it out

### 2. Copy Token
Response includes: `"token": "eyJ..."`

### 3. Use Token in Swagger
Click "Authorize" button (top right) → Paste token → Click Authorize

### 4. Call Protected Endpoints
All endpoints now include your token automatically

---

## 📊 Database

### Local Development
PostgreSQL container starts automatically with Docker Compose

**Connection String**:
```
Host=localhost;Port=5432;Database=taskmanagementdb;Username=postgres;Password=postgres
```

### Database Management (pgAdmin)
```
URL: http://localhost:5050
Email: admin@taskmanagement.local
Password: admin
```

Add Server:
- Host: `postgres`
- Username: `postgres`
- Password: `postgres`

---

## ⚙️ Configuration

### Development Defaults
- **Environment**: Development
- **Database**: PostgreSQL (localhost:5432)
- **JWT Key**: `your-super-secret-key-...` (dev placeholder)
- **Logging**: Debug level
- **Swagger**: Enabled

### Change Configuration
Edit `.env` file or `appsettings.Development.json`

---

## 🐛 Troubleshooting

### Swagger Not Loading
```bash
# Check app is running
curl http://localhost:5000/health

# Verify environment is Development (not Production)
# Production disables Swagger for security
```

### Database Connection Failed
```bash
# Verify PostgreSQL is running
docker ps | grep postgres

# Check logs
docker-compose logs postgres
```

### Port 5000 Already in Use
```bash
# Change port in docker-compose.yml
# ports:
#   - "5001:80"  # Changed from 5000:80
```

---

## 📝 Common API Workflows

### 1. Create Project
```
POST /api/projects
{
  "name": "My Project",
  "description": "Project description"
}
```

### 2. Create Task
```
POST /api/tasks
{
  "title": "Task title",
  "description": "Task description",
  "projectId": "project-id-here",
  "status": "Todo"
}
```

### 3. Update Task Status
```
PATCH /api/tasks/{taskId}/status
{
  "status": "InProgress"
}
```

### 4. List Tasks
```
GET /api/tasks?projectId=project-id-here&status=Todo
```

---

## 🔐 Production Deployment

### Quick Production Deploy
```bash
# 1. Set production environment
docker-compose -f docker-compose.prod.yml --env-file .env.prod up -d

# 2. Verify deployment
curl http://your-domain.com/health

# 3. Access API (No Swagger in production for security)
curl http://your-domain.com/api/projects
```

---

## 📚 Full Documentation

- **SETUP.md** — Complete setup guide for all platforms
- **REVIEW_SUMMARY.md** — Configuration review & optimization
- **docs/DEVELOPMENT.md** — Development standards
- **docs/TESTING.md** — Testing strategy
- **docs/PERFORMANCE.md** — Performance tips

---

## ✅ Health Check

Test API is running:
```bash
curl http://localhost:5000/health
```

Expected response:
```json
{
  "status": "healthy",
  "timestamp": "2024-06-20T10:30:00Z",
  "version": "1.0.0"
}
```

---

## 🛑 Stop Services

```bash
# Stop all containers
docker-compose down

# Stop and remove volumes (reset database)
docker-compose down -v
```

---

## 🎯 Next Steps

1. **Explore API** → Open Swagger UI and try endpoints
2. **Create Project** → See modular architecture in action
3. **Run Tests** → `dotnet test`
4. **Read Full Docs** → SETUP.md for detailed guide
5. **Deploy to Replit** → Fork repo and click Run

---

**Ready to go!** 🎉
