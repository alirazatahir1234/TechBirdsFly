# Gateway Integration Guide - Export Service

This guide shows how to integrate the Export Service with the YARP Gateway.

## 1. Update Gateway appsettings.json

Add the export service cluster and route to your gateway's `appsettings.json`:

```json
{
  "ReverseProxy": {
    "Clusters": {
      "auth_service": { ... },
      "user_service": { ... },
      "export_service": {
        "Destinations": {
          "destination_1": {
            "Address": "http://localhost:8200"
          }
        }
      }
    },
    "Routes": {
      "auth_route": { ... },
      "user_route": { ... },
      "export_route": {
        "ClusterId": "export_service",
        "Match": {
          "Path": "/api/export/{**catch-all}"
        },
        "Transforms": [
          {
            "PathPattern": "/api/export/{**catch-all}"
          }
        ]
      }
    }
  }
}
```

## 2. Run Export Service

```bash
cd services/export-service/src/ExportService.Api
dotnet run
```

Service will be available at: `http://localhost:8200`

## 3. Verify Through Gateway

Test through the gateway on port 5500:

```bash
# Get frameworks
curl http://localhost:5500/api/export/frameworks

# Generate HTML export
curl -X POST http://localhost:5500/api/export/test-project-1/html

# Generate React export
curl -X POST http://localhost:5500/api/export/test-project-1/react
```

## 4. Docker Compose Integration

Update `docker-compose.yml` to include export service:

```yaml
version: '3.8'

services:
  auth-service:
    build: ./services/auth-service
    ports:
      - "5001:5001"
    networks:
      - techbirdsfly-network

  gateway:
    build: ./gateway/yarp-gateway
    ports:
      - "5500:5500"
    depends_on:
      - auth-service
      - export-service
    networks:
      - techbirdsfly-network

  export-service:
    build: ./services/export-service
    ports:
      - "8200:8200"
    volumes:
      - ./exports:/app/exports
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
    networks:
      - techbirdsfly-network

networks:
  techbirdsfly-network:
    driver: bridge
```

Run all services:
```bash
docker-compose up -d
```

## 5. Health Checks

Verify all services are running:

```bash
# Export Service directly
curl http://localhost:8200/health

# Through Gateway
curl http://localhost:5500/api/export/health
```

Expected response:
```json
{
  "status": "Healthy"
}
```

## 6. API Endpoints

All endpoints are accessible through the gateway:

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/api/export/{projectId}/{framework}` | POST | Generate code export |
| `/api/export/{projectId}/{framework}` | GET | Retrieve export |
| `/api/export/{projectId}` | DELETE | Delete exports |
| `/api/export/frameworks` | GET | List supported frameworks |
| `/api/export/health` | GET | Health check |

## 7. CORS Configuration

The export service allows requests from frontend (localhost:3000).

If you need to change:

```csharp
// In Program.cs
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "https://yourdomain.com")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
```

## 8. Load Balancing

For production, the gateway can load balance across multiple export service instances:

```json
{
  "ClusterId": "export_service",
  "Destinations": {
    "destination_1": { "Address": "http://export-1:8200" },
    "destination_2": { "Address": "http://export-2:8200" },
    "destination_3": { "Address": "http://export-3:8200" }
  },
  "SessionAffinity": {
    "Enabled": "true",
    "Mode": "Cookie"
  }
}
```

## 9. Monitoring

Check gateway logs for export service requests:

```bash
# View gateway logs
docker logs gateway

# Look for routes to export service:
# [INF] Proxying GET http://localhost:5500/api/export/frameworks -> http://localhost:8200/api/export/frameworks
```

## 10. Troubleshooting

### "502 Bad Gateway" Error
- Export service not running on port 8200
- Check: `curl http://localhost:8200/health`
- Start service: `dotnet run` in export-service directory

### "404 Not Found"
- Route not configured in gateway
- Verify route path includes `/api/export/`
- Check appsettings.json syntax

### "Connection Refused"
- Firewall blocking port 8200
- Service running on different port
- Check configuration and startup logs

### Slow Responses
- GeneratorService unavailable (will use mock data)
- Large project causing slow code generation
- Disk I/O bottleneck for file storage
- Consider Azure Blob Storage for production

## 11. Performance Tuning

### For High Traffic

1. **Increase replicas** in Kubernetes
2. **Use session affinity** to route to same instance
3. **Enable response caching** for unchanged projects
4. **Switch to Azure Blob Storage** instead of local disk

### In Gateway appsettings.json

```json
{
  "RateLimit": {
    "Enabled": true,
    "LimitRequests": 100,
    "LimitWindowSeconds": 60
  },
  "Timeout": {
    "Seconds": 30
  }
}
```

## 12. Next: Frontend Integration

See `FRONTEND_INTEGRATION.md` for adding download buttons in Next.js app.
