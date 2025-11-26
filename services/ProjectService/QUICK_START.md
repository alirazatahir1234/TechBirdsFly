# Project Service - Quick Start Guide

Get the Project Service running locally in 5 minutes.

## Prerequisites

✅ **Required**:
- .NET 8.0 SDK installed
- PostgreSQL 15+ running on `localhost:5432`
- Username: `postgres`, Password: `postgres`

✅ **Optional**:
- Serilog Seq server (for centralized logging)
- Jaeger (for distributed tracing)

## Step 1: Database Setup (1 min)

PostgreSQL should be running with default credentials:

```bash
# Verify PostgreSQL is running
psql -U postgres -c "SELECT version();"
```

The database will be auto-created by EF Core migrations on first run.

## Step 2: Build the Solution (2 min)

```bash
cd services/ProjectService

# Restore dependencies
dotnet restore

# Build solution
dotnet build
```

## Step 3: Run the Service (1 min)

```bash
# Run the API service
dotnet run --project src/ProjectService.Api

# Output should show:
# info: Microsoft.Hosting.Lifetime[0]
#      Now listening on: http://localhost:5004
#      Application started. Press Ctrl+C to exit.
```

## Step 4: Test the API (1 min)

### Option A: Swagger UI
Open browser to: **http://localhost:5004/swagger**

### Option B: curl Commands

**Create a project**:
```bash
curl -X POST http://localhost:5004/api/projects \
  -H "Content-Type: application/json" \
  -d '{
    "ownerId": "123e4567-e89b-12d3-a456-426614174000",
    "name": "My Website",
    "framework": "nextjs",
    "theme": "dark",
    "description": "A modern website"
  }'
```

**Get project**:
```bash
curl http://localhost:5004/api/projects/PROJECT_ID
```

**List user projects**:
```bash
curl http://localhost:5004/api/projects/user/OWNER_ID
```

**Create version**:
```bash
curl -X POST http://localhost:5004/api/projects/PROJECT_ID/versions
```

**Health check**:
```bash
curl http://localhost:5004/health
```

## Common Issues

### ❌ Connection Refused
**Problem**: `Attempt to connect to 127.0.0.1:5432 failed`

**Solution**:
```bash
# Start PostgreSQL
brew services start postgresql  # macOS
# or
sudo systemctl start postgresql  # Linux
```

### ❌ Database Error
**Problem**: `The database 'project_service' does not exist`

**Solution**: EF Core will create it automatically on first run. If not:
```bash
psql -U postgres -c "CREATE DATABASE project_service;"
```

### ❌ Port Already in Use
**Problem**: `The port 5004 is already in use`

**Solution**: Use different port:
```bash
dotnet run --project src/ProjectService.Api -- --urls=http://localhost:5005
```

## Project Structure

```
services/ProjectService/
├── src/
│   ├── ProjectService.Domain/          # Entities (Project, Version, Artifact)
│   ├── ProjectService.Application/     # DTOs, Commands, Handlers
│   ├── ProjectService.Infrastructure/  # DbContext, DI
│   └── ProjectService.Api/             # REST endpoints
├── ProjectService.sln                  # Solution file
├── Dockerfile                          # Docker build
├── README.md                           # Full documentation
└── appsettings.json                    # Configuration
```

## API Endpoints Summary

| Method | Endpoint | Purpose |
|--------|----------|---------|
| **POST** | `/api/projects` | Create project |
| **GET** | `/api/projects/{id}` | Get project |
| **GET** | `/api/projects/user/{ownerId}` | List user projects |
| **PUT** | `/api/projects/{id}/rename` | Rename project |
| **PUT** | `/api/projects/{id}/settings` | Update settings |
| **DELETE** | `/api/projects/{id}` | Delete project |
| **POST** | `/api/projects/{id}/versions` | Create version |
| **GET** | `/api/projects/{id}/versions` | List versions |
| **POST** | `/api/projects/versions/link-artifact` | Link artifact |
| **GET** | `/health` | Health check |

## Testing with Postman

1. Import endpoints to Postman
2. Create collection variable: `project_id` = Generated UUID
3. Create collection variable: `owner_id` = Generated UUID

**Sample request flow**:
1. POST `/api/projects` → Save `project_id` from response
2. GET `/api/projects/{project_id}` → Verify creation
3. POST `/api/projects/{project_id}/versions` → Create version
4. GET `/api/projects/{project_id}/versions` → List versions

## Next Steps

1. **Integrate with Gateway**
   - Add route in YARP configuration
   - Prefix: `/projects` → Service: `http://project-service:5004`

2. **Connect Frontend**
   - Create Zustand store for projects
   - Implement UI for CRUD operations
   - Add forms for create/update

3. **Add Tests**
   - Unit tests for handlers
   - Integration tests for endpoints
   - E2E tests for flows

4. **Deploy to Docker**
   ```bash
   docker build -t project-service:1.0 .
   docker run -p 5004:5004 \
     -e ConnectionStrings__ProjectServiceDatabase="Host=postgres;..." \
     project-service:1.0
   ```

## Monitoring

### Logs
Logs are written to:
- Console (structured JSON)
- Serilog Seq (if running on `localhost:5341`)

### Health Check
```bash
curl http://localhost:5004/health
# Response: Healthy
```

### Swagger Docs
```
http://localhost:5004/swagger
```

## Troubleshooting

### Check Service Status
```bash
curl -v http://localhost:5004/health
```

### View Logs
```bash
# Service logs are printed to console
# Look for ERROR or WARN entries
```

### Database Migrations
```bash
# List pending migrations
dotnet ef migrations list -p src/ProjectService.Infrastructure

# Apply migrations (auto on startup, or manually)
dotnet ef database update -p src/ProjectService.Infrastructure
```

## Clean Up

```bash
# Stop service
Ctrl+C

# Remove database (if needed)
psql -U postgres -c "DROP DATABASE project_service;"

# Clean build artifacts
dotnet clean
```

## Performance Tips

1. **Use indexes**: Optimized for OwnerId, ProjectId, VersionId
2. **Pagination**: Add `Page` and `PageSize` to list endpoints
3. **Caching**: Consider Redis for frequently accessed projects
4. **Batch operations**: Combine multiple operations in single transaction

## Security Notes

⚠️ **Development Only**:
- Database password is hardcoded in `appsettings.json`
- Use environment variables for production
- Enable authentication on Gateway

## Support

For detailed documentation, see `README.md`

For API reference, see Swagger at `http://localhost:5004/swagger`

---

**Ready to go!** 🚀 Your Project Service is now running locally.
