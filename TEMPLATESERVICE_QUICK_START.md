# TemplateService Quick Start Guide

## 🚀 Quick Launch

### Option 1: Docker Compose (Full Stack)
```bash
# Start all services including TemplateService
docker-compose -f infra/docker-compose.yml up -d

# Wait ~30 seconds for services to become healthy
docker-compose ps

# Test health check
curl http://localhost:7402/api/templates/health

# Access Swagger
open http://localhost:7402/swagger
```

### Option 2: VS Code Debugger (Development)
1. Open Debug panel (Ctrl+Shift+D)
2. Select "Template Service (Port 7402)"
3. Press F5
4. Swagger opens automatically at http://localhost:7402/swagger

### Option 3: Run All Services (Compound Debug)
1. Open Debug panel (Ctrl+Shift+D)
2. Select "WORKING SERVICES (Built Successfully)"
3. Press F5
4. All 9 services start including TemplateService

---

## 📋 Core Operations

### Create a Template
```bash
curl -X POST http://localhost:7402/api/templates \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Modern Landing Page",
    "category": "Landing",
    "description": "Responsive landing with hero section"
  }'

# Response: 201 Created with Template object including ID
```

### List All Templates
```bash
curl http://localhost:7402/api/templates

# Response: 200 OK with array of templates
```

### List Templates by Category
```bash
curl "http://localhost:7402/api/templates?category=Landing"
```

### Search Templates
```bash
curl "http://localhost:7402/api/templates?search=responsive"
```

### Get Template by ID
```bash
curl http://localhost:7402/api/templates/{templateId}

# Replace {templateId} with actual UUID
```

### Upload Preview Image
```bash
curl -X POST http://localhost:7402/api/templates/{templateId}/preview \
  -F "file=@preview.png"

# Response: 200 OK with previewUrl
```

### Upload Template Files
```bash
curl -X POST http://localhost:7402/api/templates/{templateId}/files \
  -H "Content-Type: application/json" \
  -d '{
    "index.html": "<html><body>Hello</body></html>",
    "App.tsx": "export default function App() { return <div>App</div> }",
    "config.json": "{\"theme\": \"dark\"}"
  }'

# Response: 200 OK with {success: true}
```

---

## 🔗 Access via Different Endpoints

### Direct Service (Port 7402)
```
http://localhost:7402/api/templates
```

### YARP Gateway (Port 8000)
```
http://localhost:8000/api/templates
```

### From Docker Container (Service-to-Service)
```
http://template-service:8080/api/templates
```

All three endpoints work identically.

---

## 📊 Database Information

### PostgreSQL
- **Host**: localhost
- **Port**: 5438
- **Database**: templates
- **Username**: postgres
- **Password**: postgres
- **Connection Command**:
  ```bash
  psql -h localhost -p 5438 -U postgres -d templates
  ```

### View Tables
```sql
-- List all templates
SELECT * FROM templates;

-- List all template files
SELECT * FROM template_files;

-- Get template with its files
SELECT t.*, f.* FROM templates t 
LEFT JOIN template_files f ON t.id = f.template_id 
WHERE t.id = '{template-id}';
```

---

## 🗂️ MinIO File Storage

### Access MinIO Console
- **URL**: http://localhost:9001
- **Username**: minioadmin
- **Password**: minioadmin

### Bucket Location
- **Bucket Name**: techbirdsfly-storage
- **File Structure**:
  ```
  techbirdsfly-storage/
  ├── templates/
  │   └── {template-id}/
  │       └── preview.png
  ```

### Upload File to MinIO (Manual)
```bash
# Using MinIO CLI (mc)
mc cp preview.png minio/techbirdsfly-storage/templates/{id}/
```

---

## 🏗️ Build & Rebuild

### Build Only TemplateService
```bash
# Using task
Task: "build-template-service"

# Or command line
dotnet build services/template-service/TemplateService.sln
```

### Build All Services
```bash
# Using task
Task: "build-all-services"

# Or command line
dotnet build TechBirdsFly.sln
```

### Clean Build
```bash
dotnet clean services/template-service/TemplateService.sln
dotnet build services/template-service/TemplateService.sln
```

---

## 🔍 Troubleshooting

### Service won't start
```bash
# Check if ports are in use
lsof -i :7402      # TemplateService
lsof -i :5438      # PostgreSQL
lsof -i :9000      # MinIO

# Kill process if needed
kill -9 {PID}
```

### Database connection failed
```bash
# Check PostgreSQL is running
docker ps | grep templatedb

# Test connection
psql -h localhost -p 5438 -U postgres

# Restart PostgreSQL
docker-compose restart templatedb

# Check logs
docker logs techbirdsfly-templatedb
```

### MinIO connection failed
```bash
# Check MinIO is running
docker ps | grep minio

# Test MinIO health
curl http://localhost:9000/minio/health/live

# Restart MinIO
docker-compose restart minio

# Check logs
docker logs techbirdsfly-minio
```

### Files not uploading
```bash
# Check MinIO bucket exists
curl http://localhost:9001/admin/api/v2/admin/users

# Manually create bucket (in MinIO console)
# Or restart MinIO and service (auto-creates)

# Check file permissions
ls -la /path/to/minio_data
```

---

## 🧪 Complete Test Scenario

```bash
# 1. Create template
TEMPLATE=$(curl -s -X POST http://localhost:7402/api/templates \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Test Template",
    "category": "Landing",
    "description": "A test template"
  }')

TEMPLATE_ID=$(echo $TEMPLATE | jq -r '.id')
echo "Created template: $TEMPLATE_ID"

# 2. Upload preview image
curl -X POST http://localhost:7402/api/templates/$TEMPLATE_ID/preview \
  -F "file=@test-preview.png"

# 3. Upload template files
curl -X POST http://localhost:7402/api/templates/$TEMPLATE_ID/files \
  -H "Content-Type: application/json" \
  -d '{
    "index.html": "<html><body>Test</body></html>",
    "App.tsx": "export default App;"
  }'

# 4. Retrieve template
curl http://localhost:7402/api/templates/$TEMPLATE_ID

# 5. List all templates
curl http://localhost:7402/api/templates

# 6. Search templates
curl "http://localhost:7402/api/templates?search=Test"
```

---

## 📚 Useful Resources

### View Swagger Documentation
- **Direct**: http://localhost:7402/swagger
- **Gateway**: http://localhost:8000/api/templates/swagger

### Check Service Health
```bash
curl -i http://localhost:7402/api/templates/health
```

### View Service Logs (Docker)
```bash
docker logs -f techbirdsfly-template-service
```

### View Database Logs (Docker)
```bash
docker logs -f techbirdsfly-templatedb
```

### View MinIO Logs (Docker)
```bash
docker logs -f techbirdsfly-minio
```

---

## 🎯 Frontend Integration Example

### React/Next.js Component
```typescript
// Get all templates
const response = await fetch('http://localhost:8000/api/templates');
const templates = await response.json();

// Create template
const newTemplate = await fetch('http://localhost:8000/api/templates', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    name: 'My Template',
    category: 'Landing',
    description: 'My custom template'
  })
});

// Get template details
const templateId = '...';
const template = await fetch(
  `http://localhost:8000/api/templates/${templateId}`
).then(r => r.json());

// Upload preview image
const formData = new FormData();
formData.append('file', imageFile);
await fetch(
  `http://localhost:8000/api/templates/${templateId}/preview`,
  { method: 'POST', body: formData }
);
```

---

## 🔑 Key Ports Reference

| Service | Port | Internal | Type |
|---------|------|----------|------|
| TemplateService | 7402 | 8080 | HTTP |
| Gateway | 8000 | 8080 | HTTP |
| PostgreSQL | 5438 | 5432 | TCP |
| MinIO API | 9000 | 9000 | HTTP |
| MinIO Console | 9001 | 9001 | HTTP |

---

## ✅ Completion Status

- ✅ 4 Clean Architecture layers
- ✅ 6 RESTful API endpoints
- ✅ PostgreSQL integration
- ✅ MinIO file storage
- ✅ Docker containerization
- ✅ YARP Gateway routing
- ✅ VS Code debug setup
- ✅ Comprehensive documentation

**Ready for**: Development, testing, production deployment

---

**Last Updated**: 2024
**Status**: ✅ COMPLETE & PRODUCTION-READY

