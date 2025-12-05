# Launch Configuration Guide

This guide describes the updated VS Code launch configurations for TechBirdsFly microservices.

## Individual Services

All individual services can be launched independently from the "Run and Debug" panel:

### Core Services
| Service | Port | Configuration |
|---------|------|---|
| **Auth Service** | 5001 | `Auth Service (Port 5001)` |
| **User Service** | 5002 | `User Service (Port 5002)` |
| **Billing Service** | 5003 | `Billing Service (Port 5003)` |
| **API Gateway** | 8000 | `API Gateway (Port 8000)` |

### Content Management Services
| Service | Port | Configuration |
|---------|------|---|
| **Generator Service** | 5004 | `Generator Service (Port 5004)` |
| **Media Service** | 5011 | `Media Service (Port 5011)` |
| **Export Service** | 5012 | `Export Service (Port 5012)` |
| **Editor Service** | 5013 | `Editor Service (Port 5013)` |
| **Project Service** | 5014 | `Project Service (Port 5014)` |

### Publishing & Infrastructure Services
| Service | Port | Configuration |
|---------|------|---|
| **Event Bus Service** | 5009 | `Event Bus Service (Port 5009)` |
| **Publish Service (Port 5015)** | 5015 | `Publish Service (Port 5015)` |
| **Publish Service (Port 5025)** | 5025 | `Publish Service (Port 5025)` |
| **Template Service** | 5016 | `Template Service (Port 5016)` |

### Frontend
| Service | Port | Configuration |
|---------|------|---|
| **Next.js Frontend** | 3000 | `Next.js Frontend (Port 3000)` |

## Compound Launch Configurations

Use these to launch multiple services at once:

### 1. **All Services (Complete Stack)**
Launches the entire TechBirdsFly system with all microservices, gateway, and frontend.

**Services Included:**
- API Gateway (Port 8000)
- Auth Service (Port 5001)
- User Service (Port 5002)
- Billing Service (Port 5003)
- Generator Service (Port 5004)
- Media Service (Port 5011)
- Export Service (Port 5012)
- Editor Service (Port 5013)
- Project Service (Port 5014)
- Publish Service (Port 5015)
- Template Service (Port 5016)
- Event Bus Service (Port 5009)
- Next.js Frontend (Port 3000)

**Use When:** You need to test the complete system end-to-end.

### 2. **Core Services Only**
Launches the minimal set of services needed for basic operation.

**Services Included:**
- API Gateway (Port 8000)
- Auth Service (Port 5001)
- User Service (Port 5002)
- Next.js Frontend (Port 3000)

**Use When:** You're testing authentication and user management functionality.

### 3. **Content Services**
Launches all content-related services.

**Services Included:**
- Editor Service (Port 5013)
- Media Service (Port 5011)
- Generator Service (Port 5004)
- Export Service (Port 5012)
- Project Service (Port 5014)

**Use When:** You're working on content creation, editing, media handling, or export features.

### 4. **Publishing Services**
Launches all publishing-related services.

**Services Included:**
- Publish Service (Port 5015)
- Template Service (Port 5016)
- Event Bus Service (Port 5009)

**Use When:** You're testing the publishing workflow and event handling.

## How to Use

### Launch a Single Service
1. Open VS Code Debug view (Ctrl+Shift+D / Cmd+Shift+D)
2. Select the desired service from the dropdown (e.g., "Auth Service (Port 5001)")
3. Click the green play button or press F5

### Launch a Compound Configuration
1. Open VS Code Debug view (Ctrl+Shift+D / Cmd+Shift+D)
2. Select a compound configuration (e.g., "Core Services Only")
3. Click the green play button or press F5

### Stop All Services
- Click the stop button (Shift+F5) or
- Use the dropdown menu and select "Stop All"

## Prerequisites

Before launching services, ensure:

1. **Build the Solution**
   ```bash
   dotnet build TechBirdsFly.sln --configuration Debug
   ```

2. **Start the Infrastructure**
   ```bash
   # Start Docker containers (PostgreSQL, Redis, RabbitMQ, etc.)
   docker-compose -f infra/docker-compose.yml up -d
   ```

3. **Ensure Ports are Available**
   - Ports 5001-5016, 8000, 3000 should be available
   - No other services should be running on these ports

## Environment Variables

Services use the following environment variables (configured in launch.json):
- `ASPNETCORE_ENVIRONMENT`: Development
- `ASPNETCORE_URLS`: http://localhost:{port}
- `NEXT_PUBLIC_API_URL`: http://localhost:8000 (for frontend)

## Swagger/OpenAPI Documentation

Each service exposes Swagger documentation at:
- Service: `http://localhost:{port}/swagger`

Example:
- Auth Service: `http://localhost:5001/swagger`
- API Gateway: `http://localhost:8000/swagger`
- Next.js Frontend: `http://localhost:3000`

## Notes

- All services are configured to use **Debug** configuration
- Services will open their Swagger UI automatically after startup (if available)
- Logs for all services will appear in the Debug Console
- Use `stopAll: true` to stop all services when exiting a compound configuration
- Database migrations and initial setup might be required for first-time startup

## Troubleshooting

### Port Already in Use
If a port is already in use, you can:
1. Kill the process using that port
2. Modify the port in the launch configuration
3. Update the service's `ASPNETCORE_URLS` environment variable

### Service Won't Start
1. Check that the DLL file exists in the bin/Debug/net8.0 directory
2. Verify that the service has been built: `dotnet build {service.csproj}`
3. Check the Debug Console for error messages

### Cannot Connect Between Services
1. Ensure the API Gateway is running (port 8000)
2. Verify service ports match the configuration
3. Check that all services are using the correct connection strings

## Related Documentation

- See `DEPLOYMENT_STATUS.md` for service port mapping
- See `SERVICES_OVERVIEW.md` for service architecture
- See `.vscode/launch.json` for detailed launch configurations
