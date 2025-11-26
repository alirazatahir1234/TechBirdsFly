# PROJECT SERVICE - QUICK START GUIDE

## 🎯 30-Second Intro
The **Project Service** (port 5010) manages AI-generated website projects with automatic versioning:
- Create projects
- Save multiple versions (v1, v2, v3...)
- List user projects
- Get project with latest HTML
- Delete projects

---

## ⚡ Quick Setup

### 1. Start Database
```bash
# PostgreSQL must be running with database: techbirdsfly_project
psql -U postgres -c "CREATE DATABASE techbirdsfly_project;"
```

### 2. Apply Migrations
```bash
cd services/project-service/src
dotnet ef database update -c ProjectDbContext
```

### 3. Run Service
```bash
dotnet run
# ✅ Listening on http://localhost:5010
```

### 4. Verify Health
```bash
curl http://localhost:5010/api/projects/health/status
# Returns: {"status":"healthy","timestamp":"..."}
```

---

## 🚀 Essential Commands

### Create Project
```bash
PROJECT_ID=$(curl -s -X POST http://localhost:5010/api/projects/create \
  -H "Content-Type: application/json" \
  -d '{
    "userId": "550e8400-e29b-41d4-a716-446655440000",
    "name": "My AI Website",
    "industry": "Technology",
    "style": "Modern",
    "palette": "Blue-White",
    "html": "<html><body><h1>AI Generated</h1></body></html>"
  }' | jq -r '.data')

echo "Created: $PROJECT_ID"
```

### Get Project (Latest Version)
```bash
curl -s http://localhost:5010/api/projects/$PROJECT_ID | jq '.'
```

### List User Projects
```bash
USER_ID="550e8400-e29b-41d4-a716-446655440000"
curl -s http://localhost:5010/api/projects/user/$USER_ID | jq '.'
```

### Save New Version
```bash
curl -s -X POST http://localhost:5010/api/projects/$PROJECT_ID/versions \
  -H "Content-Type: application/json" \
  -d '{"html": "<html><body><h1>Updated v2</h1></body></html>"}' | jq '.'
```

### Delete Project
```bash
curl -s -X DELETE http://localhost:5010/api/projects/$PROJECT_ID | jq '.'
```

---

## 📊 6 REST Endpoints

| Method | Endpoint | Purpose |
|--------|----------|---------|
| GET | `/health/status` | Service health check |
| POST | `/create` | Create new project with v1 |
| GET | `/{projectId}` | Get project with latest version |
| GET | `/user/{userId}` | List all user projects |
| POST | `/{projectId}/versions` | Save new version |
| DELETE | `/{projectId}` | Delete project |

---

## 💾 Database Tables

**Projects**: Stores project metadata
- `Id`, `UserId`, `Name`, `Industry`, `Style`, `Palette`, `CreatedAt`, `UpdatedAt`

**ProjectVersions**: Stores HTML for each version
- `Id`, `ProjectId`, `VersionNumber`, `Html`, `CreatedAt`

---

## 🔄 Version Workflow

```
Create Project
    ↓
Generates v1 (initial HTML)
    ↓
User regenerates website
    ↓
Save new version → v2
    ↓
Another iteration
    ↓
Save new version → v3
    ↓
Get Project always returns v3 (latest)
```

---

## 📁 Project Structure

```
services/project-service/src/TechBirdsFly.ProjectService/
├── Domain/          → Business entities
├── Application/     → CQRS handlers & DTOs
├── Infrastructure/  → EF Core & database
└── WebAPI/         → Controllers & configuration
```

---

## ✅ Expected Responses

### ✅ Success
```json
{
  "success": true,
  "data": {...},
  "message": "Operation completed"
}
```

### ❌ Error
```json
{
  "success": false,
  "message": "Error description"
}
```

---

## 🧪 Automated Test
```bash
chmod +x test-project-service.sh
./test-project-service.sh
# Runs all 6 endpoint tests
```

---

## 🔗 Integration

### With Gateway
```
Frontend → YARP Gateway → Project Service (5010)
```

### Typical Flow
1. User creates project → POST `/create`
2. Returns project ID
3. Fetch with GET `/{projectId}`
4. User regenerates → POST `/{projectId}/versions`
5. New version created automatically
6. Get again → Shows updated content

---

## 🐛 Troubleshooting

| Issue | Fix |
|-------|-----|
| Connection refused | PostgreSQL not running |
| Database not found | Run: `createdb techbirdsfly_project` |
| Migrations fail | Check connection string in appsettings.json |
| 404 on project | Wrong project ID or project deleted |
| Port 5010 in use | Change Kestrel port in appsettings.json |

---

## 📋 Checklist

- [ ] PostgreSQL running
- [ ] Database created
- [ ] Migrations applied
- [ ] Service running on 5010
- [ ] Health check responds
- [ ] Can create project
- [ ] Can save versions
- [ ] Can list projects
- [ ] Can delete project

---

## 🎓 Next Steps

1. **Integration**: Connect frontend to Project Service
2. **Testing**: Run full test suite
3. **Monitoring**: View Seq logs on http://localhost:5341
4. **Phase 6**: Build frontend dashboard
5. **Phase 7**: Package all services

---

## 📞 Quick Links

- Health: `http://localhost:5010/api/projects/health/status`
- Swagger: `http://localhost:5010/swagger` (if enabled)
- Logs: `http://localhost:5341` (Seq dashboard)

---

**Ready to use!** 🚀
