# Task Management API - Production Deployment Guide

## Overview

This guide covers deploying the Task Management API to production environments. The application is optimized for:
- ✅ Containerized deployments (Docker)
- ✅ Cloud platforms (Azure, AWS, Google Cloud)
- ✅ Kubernetes orchestration
- ✅ On-premises servers

## Prerequisites

- .NET 9 SDK or .NET 9 Runtime
- PostgreSQL 14+ (Supabase recommended)
- Docker (for containerized deployments)
- Git for version control

## Environment Configuration

### Available Environments

```
Development  → Local development with full logging and Swagger
Staging      → Pre-production with Swagger enabled for testing
Production   → Production with minimal logging, no Swagger
```

### Environment Variables

Set these environment variables on your deployment platform:

```bash
# ASP.NET Core
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=https://0.0.0.0:5000
ASPNETCORE_HTTPS_PORT=5000

# Database Connection
ConnectionStrings__DefaultConnection=Server=db.example.com;Port=5432;Database=taskdb;User Id=postgres;Password=SecurePassword;SSL Mode=Require;

# JWT Configuration
Jwt__Key=<64-character-base64-encoded-secret-key>
Jwt__Issuer=TaskManagementAPI
Jwt__Audience=TaskManagementAPI
Jwt__ExpirationMinutes=60

# Logging
Serilog__MinimumLevel__Default=Warning
```

## Database Setup

### Supabase (Recommended)

1. Create a new PostgreSQL database on Supabase
2. Copy the connection string from Project Settings > Database > URI
3. Apply migrations:

```bash
# Using Supabase CLI
supabase link --project-ref <project-ref>
supabase db push

# Using Entity Framework CLI
dotnet ef database update -p TaskManagementAPI/TaskManagementAPI.csproj
```

### Local PostgreSQL

```bash
# Create database
createdb taskdb

# Apply migrations
cd TaskManagementAPI
dotnet ef database update
```

## Deployment Options

### Option 1: Docker Container

#### Build Image

```bash
docker build -f Dockerfile -t task-management-api:latest .
```

#### Run Container

```bash
docker run -d \
  --name task-api \
  -p 5000:5000 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e ConnectionStrings__DefaultConnection="Server=db.example.com;..." \
  -e Jwt__Key="<your-jwt-key>" \
  -v /var/log/task-api:/app/logs \
  task-management-api:latest
```

#### Using Docker Compose

```yaml
version: '3.9'
services:
  api:
    image: task-management-api:latest
    ports:
      - "5000:5000"
    environment:
      ASPNETCORE_ENVIRONMENT: Production
      ConnectionStrings__DefaultConnection: "postgresql://..."
    volumes:
      - ./logs:/app/logs
    restart: unless-stopped
```

### Option 2: Kubernetes

#### Create Namespace

```bash
kubectl create namespace task-management
```

#### Create ConfigMap

```bash
kubectl create configmap app-config \
  --from-literal=ASPNETCORE_ENVIRONMENT=Production \
  -n task-management
```

#### Create Secret

```bash
kubectl create secret generic app-secrets \
  --from-literal=jwt-key="<your-jwt-key>" \
  --from-literal=db-connection="<connection-string>" \
  -n task-management
```

#### Deploy

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: task-api
  namespace: task-management
spec:
  replicas: 3
  selector:
    matchLabels:
      app: task-api
  template:
    metadata:
      labels:
        app: task-api
    spec:
      containers:
      - name: api
        image: task-management-api:latest
        ports:
        - containerPort: 5000
        envFrom:
        - configMapRef:
            name: app-config
        env:
        - name: Jwt__Key
          valueFrom:
            secretKeyRef:
              name: app-secrets
              key: jwt-key
        - name: ConnectionStrings__DefaultConnection
          valueFrom:
            secretKeyRef:
              name: app-secrets
              key: db-connection
        livenessProbe:
          httpGet:
            path: /health
            port: 5000
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /ready
            port: 5000
          initialDelaySeconds: 10
          periodSeconds: 5
        resources:
          requests:
            memory: "256Mi"
            cpu: "250m"
          limits:
            memory: "512Mi"
            cpu: "500m"
---
apiVersion: v1
kind: Service
metadata:
  name: task-api-service
  namespace: task-management
spec:
  selector:
    app: task-api
  type: LoadBalancer
  ports:
  - port: 80
    targetPort: 5000
    protocol: TCP
```

### Option 3: Azure App Service

1. Create App Service on Azure Portal
2. Configure deployment:

```bash
# Deploy using Azure CLI
az webapp deployment source config-zip \
  --resource-group myResourceGroup \
  --name myAppName \
  --src <path-to-publish-package>

# Or deploy from GitHub
az webapp deployment source config \
  --resource-group myResourceGroup \
  --name myAppName \
  --repo-url https://github.com/yourusername/task-management-api \
  --branch main \
  --manual-integration
```

3. Configure environment variables in App Service → Configuration → Application settings

## Health Checks & Monitoring

### Health Endpoints

```bash
# Check application health
curl http://localhost:5000/health

# Response:
{
  "status": "healthy",
  "timestamp": "2026-06-21T14:30:00Z",
  "environment": "Production",
  "version": "1.0.0",
  "uptime": 3600.5
}

# Check readiness (for Kubernetes)
curl http://localhost:5000/ready

# Response:
{
  "ready": true
}
```

### Logging

Application logs are written to:
- **Console**: Real-time log output
- **File**: Daily rolling logs in `logs/` directory
  - `app-*.log`: Application logs (30-day retention)
  - `audit-*.log`: Audit events (90-day retention)

View logs:

```bash
# Docker
docker logs task-api

# Kubernetes
kubectl logs -n task-management deployment/task-api

# File system
tail -f logs/app-*.log
```

### Monitoring & Observability

#### Application Insights (Azure)

```json
{
  "ApplicationInsights": {
    "InstrumentationKey": "<your-key>"
  }
}
```

#### Prometheus Metrics

Health endpoints provide basic metrics. For detailed metrics, integrate Prometheus:

```csharp
services.AddPrometheusMetrics();
```

## Security Best Practices

### ✅ Implemented

1. **HTTPS/TLS Enforcement**: HSTS header in production
2. **JWT Authentication**: Secure token-based authentication
3. **CORS Protection**: Controlled cross-origin requests
4. **Security Headers**: X-Frame-Options, X-Content-Type-Options, X-XSS-Protection
5. **Rate Limiting**: Fixed-window rate limiting on endpoints
6. **Antiforgery**: CSRF protection on state-changing operations
7. **Data Validation**: Input validation on all endpoints

### ⚠️ Additional Recommendations

1. **Secrets Management**:
   - Use Azure Key Vault, AWS Secrets Manager, or HashiCorp Vault
   - Never commit secrets to version control

2. **API Gateway**:
   - Use Kong, AWS API Gateway, or Azure API Management
   - Implement authentication at gateway level
   - Add request throttling and caching

3. **SSL/TLS Certificates**:
   - Use Let's Encrypt for free certificates
   - Auto-renew via certbot or similar tools
   - For production: use wildcard or multi-SAN certificates

4. **Database Security**:
   - Enable database encryption (Supabase does this by default)
   - Use SSL connections to PostgreSQL
   - Implement database backups with point-in-time recovery
   - Restrict IP access to database

5. **Audit Logging**:
   - Enable database audit trail
   - Monitor API logs for suspicious activity
   - Set up alerts for authentication failures

## Performance Tuning

### Database Optimization

```sql
-- Enable query optimization
ANALYZE;

-- Create indexes on frequently queried columns
CREATE INDEX idx_users_email ON users(email);
CREATE INDEX idx_projects_userId ON projects("userId");
CREATE INDEX idx_tasks_projectId ON tasks("projectId");
```

### Application Optimization

1. **Connection Pooling**:
   - Configured automatically in connection string
   - Default pool size: 25 connections
   - Adjust with `MaxPoolSize=` in connection string

2. **Caching**:
   - In-memory cache for frequently accessed data
   - Consider Redis for distributed caching

3. **Compression**:
   - Enable gzip compression on API responses
   - Configured in Kestrel settings

## Backup & Disaster Recovery

### Database Backups

For Supabase:
```bash
# Daily automated backups (retention: 7 days for free tier)
# Access from Project Settings > Database > Backups

# Manual backup
pg_dump postgres://user:password@db.supabase.co/postgres > backup.sql
```

For on-premises PostgreSQL:
```bash
# Daily backup script
#!/bin/bash
backup_dir="/backups/postgres"
timestamp=$(date +"%Y%m%d_%H%M%S")
pg_dump taskdb | gzip > "$backup_dir/taskdb_$timestamp.sql.gz"

# Keep last 30 days
find $backup_dir -mtime +30 -delete
```

### Disaster Recovery Plan

1. **RTO (Recovery Time Objective)**: < 1 hour
2. **RPO (Recovery Point Objective)**: < 15 minutes
3. **Procedures**:
   - Maintain database backups in separate location
   - Document application deployment process
   - Test recovery procedures monthly

## Scaling & Load Balancing

### Horizontal Scaling (Multiple Instances)

1. **Use stateless sessions**: Avoid in-memory session storage
2. **Shared state**: Use database for shared state
3. **Load Balancer Configuration**:

```nginx
# Nginx example
upstream task_api {
    server api1:5000;
    server api2:5000;
    server api3:5000;
}

server {
    listen 80;
    server_name api.example.com;

    location / {
        proxy_pass http://task_api;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
    }
}
```

### Vertical Scaling

Monitor and adjust:
- Memory allocation
- CPU resources
- Database connection limits

## Troubleshooting

### Common Issues

**Problem: 502 Bad Gateway**
```bash
# Check application is running
docker ps  # or: kubectl get pods

# Check logs
docker logs task-api  # or: kubectl logs deployment/task-api

# Check health endpoint
curl http://localhost:5000/health
```

**Problem: Database Connection Failed**
```bash
# Verify connection string
echo $ConnectionStrings__DefaultConnection

# Test database connectivity
psql "$(echo $ConnectionStrings__DefaultConnection | sed 's/postgresql:\/\///')"

# Check SSL mode requirement
# Connection string should include: SSL Mode=Require
```

**Problem: High Memory Usage**
```bash
# Check .NET GC settings
dotnet --info

# Adjust heap size
DOTNET_gcHeapHardLimit=536870912  # 512MB
DOTNET_GCRetainVM=1               # Retain VM memory
```

## Maintenance

### Regular Tasks

- **Daily**: Monitor application logs and health endpoints
- **Weekly**: Review performance metrics and error rates
- **Monthly**: Update dependencies (security patches)
- **Quarterly**: Full disaster recovery drill
- **Annually**: Security audit and penetration testing

### Update Procedure

1. Test updates in Staging environment
2. Run database migrations
3. Deploy to Production during maintenance window
4. Monitor health endpoints and logs
5. Rollback plan if issues detected

```bash
# Staging deployment
dotnet publish -c Release -o ./publish/staging
docker build -t task-api:staging -f Dockerfile .
docker push task-api:staging

# Production deployment (after testing)
docker tag task-api:staging task-api:latest
docker push task-api:latest

# Kubernetes
kubectl set image deployment/task-api \
  task-api=task-management-api:latest \
  -n task-management
```

## Support & Documentation

- **API Documentation**: Available at `/swagger` in Development/Staging
- **Issue Tracking**: GitHub Issues
- **Monitoring**: Application Insights / Datadog
- **Support Contact**: support@taskmanagement.api

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0.0   | 2026-06-21 | Initial production release |

---

**Last Updated**: June 21, 2026  
**Maintained by**: Development Team
