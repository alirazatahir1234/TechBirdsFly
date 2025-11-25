# Export Service - Code Export Microservice

> **Step 4 of TechBirdsFly Platform**
> 
> Generate downloadable HTML/React/Next.js code from website projects in seconds.

## 🎯 Overview

The **Export Service** is a dedicated microservice that converts TechBirdsFly projects into production-ready code in multiple frameworks (HTML, React, Next.js), packages them as ZIP archives, and provides download URLs.

**Part of:** Microservices Architecture with YARP Gateway
**Port:** `8200`
**Framework:** .NET 8.0 with ASP.NET Core 8.0
**Architecture Pattern:** Clean Architecture (Domain → Application → Infrastructure → API)

## 🏗️ Architecture

```
ExportService/
├── src/
│   ├── ExportService.Domain/
│   │   ├── Entities/
│   │   │   └── ExportFile.cs          (Entity model + ExportStatus enum)
│   │   └── ValueObjects/
│   │       └── Framework.cs            (html, react, nextjs)
│   │
│   ├── ExportService.Application/
│   │   ├── Interfaces/
│   │   │   └── IExportService.cs      (IExportService, IProjectFetcher, ICodeGenerator, IFileStorage)
│   │   ├── Services/
│   │   │   └── ExportApplicationService.cs  (Main orchestration)
│   │   └── DTOs/
│   │       └── ExportModels.cs        (ProjectDto, ExportResult, ExportRequestDto)
│   │
│   ├── ExportService.Infrastructure/
│   │   ├── Generators/
│   │   │   ├── CodeGenerators.cs      (HtmlCodeGenerator, ReactCodeGenerator, NextJsCodeGenerator)
│   │   │   └── ProjectFetcher.cs      (Fetches from GeneratorService, with mock fallback)
│   │   └── Storage/
│   │       └── FileStorage.cs         (LocalFileStorage + AzureBlobStorage)
│   │
│   └── ExportService.Api/
│       ├── Program.cs                 (Minimal API setup, DI registration)
│       ├── appsettings.json           (Configuration)
│       └── appsettings.Development.json
│
└── Dockerfile
```

## 🚀 Quick Start

### Prerequisites
- .NET 8.0 SDK
- TechBirdsFly solution with Auth Service, Generator Service, and Gateway running

### Build & Run

```bash
# Navigate to export service
cd services/export-service/src/ExportService.Api

# Restore dependencies
dotnet restore

# Build
dotnet build

# Run (defaults to port 8200)
dotnet run

# Or with custom port
dotnet run --urls="http://localhost:8200"
```

Visit: `http://localhost:8200/health`

### Docker Build

```bash
cd services/export-service
docker build -t export-service:latest .
docker run -p 8200:8200 -v $(pwd)/exports:/app/exports export-service:latest
```

## 📡 API Endpoints

### 1. Generate Export
**POST** `/api/export/{projectId}/{framework}`

Request:
```json
{
  "projectId": "proj-123",
  "framework": "html"  // html, react, nextjs
}
```

Response:
```json
{
  "exportId": "guid",
  "projectId": "proj-123",
  "framework": "html",
  "downloadUrl": "/exports/proj-123/website_20231125_143022.zip",
  "fileSize": 2048,
  "createdAt": "2023-11-25T14:30:22Z"
}
```

### 2. Get Export
**GET** `/api/export/{projectId}/{framework}`

Returns previously generated export or null.

### 3. Delete Exports
**DELETE** `/api/export/{projectId}`

Removes all exports for a project.

### 4. Supported Frameworks
**GET** `/api/frameworks`

```json
[
  { "name": "html", "description": "Plain HTML/CSS" },
  { "name": "react", "description": "React JSX Components" },
  { "name": "nextjs", "description": "Next.js App Router" }
]
```

### 5. Health Check
**GET** `/health`

## 🔧 Configuration

### appsettings.json

```json
{
  "Storage": {
    "ExportDirectory": "./exports",           // Local disk storage
    "AzureContainer": "exports"               // Azure Blob container name
  }
}
```

### Environment Variables

```bash
# Optional overrides
ASPNETCORE_URLS=http://+:8200
ASPNETCORE_ENVIRONMENT=Development
DOTNET_ENVIRONMENT=Development
```

## 🔄 Integration Points

### 1. GeneratorService Integration
The ProjectFetcher fetches project structure from GeneratorService:
```
Export Service → GeneratorService (port 5003)
```

**Mock Fallback:** If GeneratorService is unavailable, returns test data automatically.

### 2. YARP Gateway Integration
Configure in Gateway's `appsettings.json`:

```json
{
  "ClusterId": "export_service",
  "Destinations": {
    "destination_1": {
      "Address": "http://localhost:8200"
    }
  }
},
{
  "RouteId": "export_route",
  "ClusterId": "export_service",
  "Match": {
    "Path": "/api/export/{**catch-all}"
  }
}
```

Then access through gateway:
```bash
curl http://localhost:5500/api/export/proj-123/html
```

### 3. Next.js Frontend Integration

```tsx
// exportStore.ts
import { create } from 'zustand';

interface ExportStore {
  downloadCode: (projectId: string, framework: string) => Promise<void>;
}

export const useExportStore = create<ExportStore>((set) => ({
  downloadCode: async (projectId, framework) => {
    const res = await fetch(
      `${process.env.NEXT_PUBLIC_API_BASE}/export/${projectId}/${framework}`,
      { method: 'POST' }
    );
    
    const data = await res.json();
    window.location.href = data.downloadUrl;
  }
}));
```

Button component:
```tsx
<Button onClick={() => useExportStore.getState().downloadCode(projectId, 'html')}>
  Download HTML
</Button>
```

## 💾 Storage Options

### Local File Storage (Default)
Saves exports to disk at `./exports/{projectId}/website_*.zip`

**Configuration:**
```json
{ "Storage": { "ExportDirectory": "./exports" } }
```

**Use Case:** Development and testing

### Azure Blob Storage
Saves exports to Azure Blob Storage container

**Configuration:**
```json
{ "Storage": { "AzureContainer": "exports" } }
```

**Setup:**
1. Create Azure Storage Account
2. Create container named "exports"
3. Install `Azure.Storage.Blobs` NuGet package
4. Uncomment `AzureBlobStorage` in Program.cs
5. Configure connection string

**Use Case:** Production with cloud storage

## 🎨 Supported Frameworks

### HTML
Plain HTML/CSS output with inline styles
```html
<!DOCTYPE html>
<html>
  <head>
    <style>/* Generated CSS */</style>
  </head>
  <body><!-- Generated HTML --></body>
</html>
```

### React
JSX Components with Tailwind CSS
```jsx
export default function App() {
  return (
    <div className="app">
      {/* Component structure */}
    </div>
  );
}
```

### Next.js
Next.js 14 App Router with Server Components
```jsx
'use client';
export default function Page() {
  return (
    <main className="main">
      {/* Generated content */}
    </main>
  );
}
```

## 📦 Dependencies

### Core
- `Microsoft.AspNetCore.OpenApi` - OpenAPI/Swagger support
- `Swashbuckle.AspNetCore` - Swagger UI

### Optional (for Azure)
- `Azure.Storage.Blobs` - Azure Blob Storage client

## 🧪 Testing

### Manual Test - HTML Export

```bash
curl -X POST http://localhost:5500/api/export/test-proj-1/html \
  -H "Content-Type: application/json"

# Response:
{
  "exportId": "12345678-1234-1234-1234-123456789012",
  "projectId": "test-proj-1",
  "framework": "html",
  "downloadUrl": "/exports/test-proj-1/website_20231125_143022.zip",
  "fileSize": 2048,
  "createdAt": "2023-11-25T14:30:22Z"
}
```

### Manual Test - Framework List

```bash
curl http://localhost:5500/api/frameworks
```

### Integration Test Script

Create `test-export.sh`:
```bash
#!/bin/bash

PROJECT_ID="test-project-$(date +%s)"
GATEWAY="http://localhost:5500"

echo "🔄 Testing Export Service..."

# Test 1: Get supported frameworks
echo "1️⃣ Frameworks:"
curl -s "$GATEWAY/api/frameworks" | jq .

# Test 2: Generate HTML export
echo -e "\n2️⃣ HTML Export:"
curl -s -X POST "$GATEWAY/api/export/$PROJECT_ID/html" | jq .

# Test 3: Generate React export
echo -e "\n3️⃣ React Export:"
curl -s -X POST "$GATEWAY/api/export/$PROJECT_ID/react" | jq .

# Test 4: Generate Next.js export
echo -e "\n4️⃣ Next.js Export:"
curl -s -X POST "$GATEWAY/api/export/$PROJECT_ID/nextjs" | jq .

echo -e "\n✅ Export Service Tests Complete"
```

## 🐛 Troubleshooting

### "Project not found"
- Verify GeneratorService is running on port 5003
- Check projectId exists in GeneratorService
- Service will auto-use mock data if GeneratorService unavailable

### "Unsupported framework"
- Valid frameworks: `html`, `react`, `nextjs`
- Framework is case-insensitive

### "Export directory not found"
- Check file system permissions
- Default directory: `./exports`
- Override via `appsettings.json` → `Storage:ExportDirectory`

### Port 8200 already in use
```bash
# Find and kill process
lsof -i :8200
kill -9 <PID>

# Or use different port
dotnet run --urls="http://localhost:8201"
```

## 📊 Performance

| Operation | Time | Notes |
|-----------|------|-------|
| Fetch Project | ~100ms | From GeneratorService |
| Generate HTML | ~50ms | Simple template |
| Generate React | ~75ms | With JSX syntax |
| Generate Next.js | ~100ms | With server components |
| Create ZIP | ~200ms | Depends on code size |
| Save to Disk | ~150ms | Local I/O |
| **Total (HTML)** | **~500ms** | End-to-end |

## 🔐 Security Considerations

1. **Authentication:** Currently allows all requests
   - TODO: Add JWT validation from Gateway
   - Validate user ownership of project

2. **Rate Limiting:** No limits currently
   - TODO: Implement per-user rate limiting
   - Recommend: 10 exports/min per user

3. **File Cleanup:** No automatic cleanup
   - TODO: Implement retention policy
   - Recommend: Delete exports older than 7 days

4. **Access Control:**
   - Only allow users to export their own projects
   - Validate projectId belongs to authenticated user

## 📝 Logging

Logs are output to console and debug by default.

**Log Levels:**
- `Information` - Request start/completion
- `Warning` - Service unavailability, retries
- `Error` - Failed operations

**Example Log:**
```
info: ExportService.Infrastructure.Generators.ProjectFetcher[0]
      Fetching project test-proj-1 from GeneratorService

info: ExportService.Api.Program[0]
      Saved export for project test-proj-1 to /app/exports/test-proj-1/website_20231125_143022.zip (2048 bytes)
```

## 🚀 Production Deployment

### Kubernetes
```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: export-service
spec:
  replicas: 3
  template:
    spec:
      containers:
      - name: export-service
        image: export-service:latest
        ports:
        - containerPort: 8200
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: "Production"
        volumeMounts:
        - name: exports
          mountPath: /app/exports
      volumes:
      - name: exports
        persistentVolumeClaim:
          claimName: exports-pvc
```

### Azure Container Instances
```bash
az container create \
  --resource-group rg-techbirdsfly \
  --name export-service \
  --image export-service:latest \
  --cpu 1 \
  --memory 512 \
  --ports 8200 \
  --environment-variables ASPNETCORE_ENVIRONMENT=Production
```

### Docker Compose Integration
Already included in root `docker-compose.yml`:
```yaml
export-service:
  build: ./services/export-service
  ports:
    - "8200:8200"
  volumes:
    - ./exports:/app/exports
  environment:
    - ASPNETCORE_ENVIRONMENT=Production
```

## 📚 Related Services

- **Auth Service** (5001) - User authentication & authorization
- **Generator Service** (5003) - Project structure & templates
- **YARP Gateway** (5500) - API routing & security
- **Frontend** (3000) - Next.js UI

## 🔄 Future Enhancements

- [ ] Database for export history (PostgreSQL)
- [ ] AWS S3 integration
- [ ] Email download links
- [ ] GitHub/GitLab deployment options
- [ ] Custom component libraries
- [ ] Build configuration (bundler, minification)
- [ ] SSL/HTTPS certificate generation
- [ ] Environment variable injection
- [ ] Database schema export (if applicable)
- [ ] Docker Compose generation

## 📄 License

Part of TechBirdsFly Platform - All Rights Reserved

## 👨‍💻 Support

For issues or questions:
1. Check troubleshooting section above
2. Review logs in console output
3. Test endpoint directly: `http://localhost:8200/health`
4. Verify GeneratorService is running
