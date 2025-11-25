# Export Service - Quick Start

## 🚀 Build and Run in 60 Seconds

### Prerequisites
- .NET 8.0 SDK installed
- Port 8200 available
- (Optional) Auth Service & Gateway running on ports 5001 & 5500

### Step 1: Build the Project

```bash
cd services/export-service/src/ExportService.Api
dotnet restore
dotnet build
```

### Step 2: Run the Service

```bash
dotnet run
```

You'll see:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:8200
```

### Step 3: Test It Works

```bash
# In another terminal
curl http://localhost:8200/health
```

Expected response:
```json
{
  "status": "Healthy"
}
```

## 📡 Test Export Endpoints

### Get Supported Frameworks

```bash
curl http://localhost:8200/api/frameworks
```

### Generate HTML Export

```bash
curl -X POST http://localhost:8200/api/export/test-project-1/html
```

Response:
```json
{
  "exportId": "guid",
  "projectId": "test-project-1",
  "framework": "html",
  "downloadUrl": "/exports/test-project-1/website_20231125_143022.zip",
  "fileSize": 2048,
  "createdAt": "2023-11-25T14:30:22Z"
}
```

### Generate React Export

```bash
curl -X POST http://localhost:8200/api/export/test-project-1/react
```

### Generate Next.js Export

```bash
curl -X POST http://localhost:8200/api/export/test-project-1/nextjs
```

## 🐳 Docker Build

```bash
cd services/export-service
docker build -t export-service:latest .
docker run -p 8200:8200 -v $(pwd)/exports:/app/exports export-service:latest
```

## 🔗 Through Gateway (if running)

```bash
# Gateway should be on port 5500
curl -X POST http://localhost:5500/api/export/test-project-1/html
```

## 📁 Project Structure

```
src/
├── ExportService.Domain/           # Entities & Value Objects
│   ├── Entities/ExportFile.cs
│   └── ValueObjects/Framework.cs
├── ExportService.Application/      # Interfaces & Use Cases
│   ├── Interfaces/IExportService.cs
│   ├── Services/ExportApplicationService.cs
│   └── DTOs/ExportModels.cs
├── ExportService.Infrastructure/   # Implementations
│   ├── Generators/CodeGenerators.cs
│   ├── Generators/ProjectFetcher.cs
│   └── Storage/FileStorage.cs
└── ExportService.Api/              # Minimal API
    ├── Program.cs
    └── appsettings.json
```

## 🎯 Key Features

✅ **HTML Export** - Plain HTML/CSS with inline styles
✅ **React Export** - JSX components with styling
✅ **Next.js Export** - App Router ready with server components
✅ **Local Storage** - Saves zips to disk
✅ **Azure Support** - Ready for Blob Storage
✅ **Mock Data** - Works without GeneratorService
✅ **Health Checks** - `/health` endpoint
✅ **Error Handling** - Comprehensive validation

## 📝 Configuration

Edit `appsettings.json`:

```json
{
  "Storage": {
    "ExportDirectory": "./exports",
    "AzureContainer": "exports"
  }
}
```

## 🔄 Integration Path

1. ✅ **Export Service Running** (port 8200)
2. ⬜ **Add to Gateway** - Update gateway routes
3. ⬜ **Frontend Integration** - Add download buttons
4. ⬜ **Test Full Flow** - End-to-end testing

See `GATEWAY_INTEGRATION.md` and `FRONTEND_INTEGRATION.md` for next steps.

## 🆘 Troubleshooting

### Service won't start
```bash
# Check if port 8200 is in use
lsof -i :8200

# Kill existing process
kill -9 <PID>

# Try different port
dotnet run --urls="http://localhost:8201"
```

### "Project not found" error
- This is normal if GeneratorService isn't running
- Service automatically uses mock data
- See logs: `info: ExportService.Infrastructure.Generators.ProjectFetcher`

### "Access denied" on exports folder
```bash
# Create exports folder with permissions
mkdir -p ./exports
chmod 755 ./exports
```

### Build errors
```bash
# Clear and rebuild
cd services/export-service/src/ExportService.Api
dotnet clean
dotnet restore
dotnet build
```

## 📚 Next Steps

1. **Read Full README.md** - Complete documentation
2. **View Architecture** - See clean architecture pattern
3. **Integrate Gateway** - Follow GATEWAY_INTEGRATION.md
4. **Add Frontend Buttons** - Follow FRONTEND_INTEGRATION.md
5. **Test Endpoints** - Use curl or Postman
6. **Deploy** - Docker or Kubernetes

## 🎓 Learning Resources

- [Clean Architecture in .NET](https://docs.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/common-web-application-architectures)
- [ASP.NET Core Minimal APIs](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis)
- [YARP Gateway](https://microsoft.github.io/reverse-proxy/)

## 💡 Tips

- Service includes mock project data for offline testing
- Zips are stored with timestamps to prevent overwrites
- Each export is independent - no state shared between requests
- Supports all three frameworks simultaneously
- Ready for horizontal scaling (stateless design)

## ✨ That's It!

You now have a fully functional Code Export Microservice integrated into TechBirdsFly architecture.

**Next:** Check GATEWAY_INTEGRATION.md to route exports through your API Gateway.

---

Questions? See README.md or the integration guides.
