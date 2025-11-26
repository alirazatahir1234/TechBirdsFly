# Project Service - Complete Implementation

## Overview

The **Project Service** is a core microservice in the TechBirdsFly platform responsible for managing project lifecycle, versioning, and artifact management. It follows clean architecture principles with CQRS pattern using MediatR.

## Architecture

### Layer Structure

```
ProjectService
├── Domain Layer (Entities)
│   ├── Project.cs
│   ├── ProjectVersion.cs
│   └── ProjectArtifact.cs
├── Application Layer (CQRS)
│   ├── DTOs
│   │   ├── ProjectDto.cs
│   │   └── ProjectRequestDtos.cs
│   └── Commands & Handlers
│       ├── ProjectCommands.cs (8 CQRS requests)
│       └── ProjectHandlers.cs (8 handlers with business logic)
├── Infrastructure Layer (Persistence)
│   ├── ProjectDbContext.cs (EF Core with PostgreSQL)
│   └── DependencyInjection.cs (Service registration)
└── API Layer (REST Endpoints)
    └── Program.cs (9 endpoints)
```

### Technology Stack

| Component | Technology | Version |
|-----------|-----------|---------|
| **Runtime** | .NET | 8.0 |
| **Web Framework** | ASP.NET Core | 8.0 |
| **Database** | PostgreSQL | 15+ |
| **ORM** | Entity Framework Core | 8.0 |
| **CQRS** | MediatR | 12.2.0 |
| **Logging** | Serilog | 3.0.1 |
| **API Docs** | Swagger/OpenAPI | 6.4.8 |
| **Tracing** | OpenTelemetry + Jaeger | 1.7.0 |
| **Container** | Docker | Latest |

## Domain Model

### Project Entity
```csharp
public class Project
{
    public Guid Id { get; set; }
    public Guid OwnerId { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public string Framework { get; set; } // nextjs, react, html
    public string? Theme { get; set; }
    public DateTime CreatedAt { get; set; }
    public ICollection<ProjectVersion> Versions { get; set; }
}
```

### ProjectVersion Entity
```csharp
public class ProjectVersion
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public int VersionNumber { get; set; } // 1, 2, 3...
    public DateTime CreatedAt { get; set; }
    public Project Project { get; set; }
    public ICollection<ProjectArtifact> Artifacts { get; set; }
}
```

### ProjectArtifact Entity
```csharp
public class ProjectArtifact
{
    public Guid Id { get; set; }
    public Guid VersionId { get; set; }
    public Guid ArtifactId { get; set; } // From GeneratorService
    public string Type { get; set; }
    public DateTime LinkedAt { get; set; }
    public ProjectVersion Version { get; set; }
}
```

## API Endpoints

### Project Management

| Method | Endpoint | Description |
|--------|----------|-------------|
| **POST** | `/api/projects` | Create new project |
| **GET** | `/api/projects/{projectId}` | Get project by ID |
| **GET** | `/api/projects/user/{ownerId}` | List user's projects |
| **PUT** | `/api/projects/{projectId}/rename` | Rename project |
| **PUT** | `/api/projects/{projectId}/settings` | Update settings |
| **DELETE** | `/api/projects/{projectId}` | Delete project |

### Version Management

| Method | Endpoint | Description |
|--------|----------|-------------|
| **POST** | `/api/projects/{projectId}/versions` | Create new version |
| **GET** | `/api/projects/{projectId}/versions` | List all versions |

### Artifact Management

| Method | Endpoint | Description |
|--------|----------|-------------|
| **POST** | `/api/projects/versions/link-artifact` | Link artifact to version |

### Health

| Method | Endpoint | Description |
|--------|----------|-------------|
| **GET** | `/health` | Service health check |

## CQRS Commands & Handlers

### Commands (8 total)

1. **CreateProjectCommand** → Creates project + initial version
   - Input: CreateProjectRequest (OwnerId, Name, Framework, Theme, Description)
   - Output: CreateProjectResponse (Project + InitialVersion)
   - Logic: Creates project entity, generates initial version 1

2. **CreateVersionCommand** → Creates incremented version
   - Input: ProjectId
   - Output: ProjectVersionDto
   - Logic: Gets latest version, increments VersionNumber, creates new version

3. **LinkArtifactCommand** → Links GeneratorService artifact
   - Input: VersionId, ArtifactId, Type
   - Output: bool
   - Logic: Creates ProjectArtifact linking entry

4. **RenameProjectCommand** → Updates project name
   - Input: ProjectId, NewName
   - Output: bool
   - Logic: Validates and updates Project.Name

5. **DeleteProjectCommand** → Deletes project
   - Input: ProjectId
   - Output: bool
   - Logic: Removes project (cascades to versions/artifacts)

6. **UpdateProjectSettingsCommand** → Updates project metadata
   - Input: ProjectId, UpdateProjectSettingsRequest
   - Output: ProjectDto
   - Logic: Updates Description, Framework, Theme with null checks

### Queries (3 total)

1. **GetProjectQuery** → Retrieves single project
   - Input: ProjectId
   - Output: ProjectDto?
   - Logic: Fetches project with version count

2. **GetUserProjectsQuery** → Lists user's projects
   - Input: OwnerId
   - Output: List<ProjectDto>
   - Logic: Filters projects by OwnerId, includes metadata

3. **GetProjectVersionsQuery** → Lists project versions
   - Input: ProjectId
   - Output: List<ProjectVersionDto>
   - Logic: Retrieves ordered versions with artifact counts

## Data Access Layer

### ProjectDbContext Configuration

```csharp
// Relationships
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Project → Versions (1-to-many)
    modelBuilder.Entity<Project>()
        .HasMany(p => p.Versions)
        .WithOne(v => v.Project)
        .HasForeignKey(v => v.ProjectId)
        .OnDelete(DeleteBehavior.Cascade);
    
    // ProjectVersion → Artifacts (1-to-many)
    modelBuilder.Entity<ProjectVersion>()
        .HasMany(v => v.Artifacts)
        .WithOne(a => a.Version)
        .HasForeignKey(a => a.VersionId)
        .OnDelete(DeleteBehavior.Cascade);
}
```

### Indexes (Performance Optimization)

- **OwnerId**: Fast project lookup by owner
- **ProjectId**: Version queries
- **VersionId**: Artifact queries
- **ArtifactId**: Artifact lookups

## Database Initialization

The `DependencyInjection.cs` registers:

```csharp
// PostgreSQL connection
services.AddDbContext<ProjectDbContext>(options =>
    options.UseNpgsql(configuration.GetConnectionString("ProjectServiceDatabase"))
);

// Auto-migrations on startup
await app.Services.InitializeDatabaseAsync();
```

## Configuration Files

### appsettings.json (Production)

```json
{
  "ConnectionStrings": {
    "ProjectServiceDatabase": "Host=localhost;Port=5432;Database=project_service;Username=postgres;Password=postgres"
  },
  "Serilog": {
    "MinimumLevel": "Information"
  }
}
```

### appsettings.Development.json (Local)

```json
{
  "ConnectionStrings": {
    "ProjectServiceDatabase": "Host=localhost;Port=5432;Database=project_service_dev;Username=postgres;Password=postgres"
  },
  "Serilog": {
    "MinimumLevel": "Debug"
  }
}
```

## Docker Configuration

### Dockerfile (Multi-stage)

- **Stage 1 (Builder)**: .NET 8 SDK
  - Restores dependencies
  - Builds solution
  - Publishes Release build
  
- **Stage 2 (Runtime)**: .NET 8 ASP.NET Core
  - Installs curl for health checks
  - Copies published app
  - Exposes port 5004
  - Includes health check probe

### Health Check

```dockerfile
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:5004/health || exit 1
```

## Project Files

### Solution Structure

```
ProjectService.sln
├── ProjectService.Domain.csproj
│   └── Domain entities only (no dependencies)
├── ProjectService.Application.csproj
│   ├── References: Domain
│   └── Dependencies: MediatR
├── ProjectService.Infrastructure.csproj
│   ├── References: Domain, Application
│   └── Dependencies: EF Core PostgreSQL, MediatR
└── ProjectService.Api.csproj
    ├── References: Domain, Application, Infrastructure
    └── Dependencies: All (Swagger, Serilog, OpenTelemetry, etc.)
```

### NuGet Dependencies

**ProjectService.Application.csproj**:
- MediatR 12.2.0

**ProjectService.Infrastructure.csproj**:
- EntityFrameworkCore 8.0.0
- EntityFrameworkCore.PostgreSQL 8.0.0
- EntityFrameworkCore.Design 8.0.0
- MediatR 12.2.0
- Microsoft.Extensions.DependencyInjection 8.0.0
- Microsoft.Extensions.Configuration 8.0.0
- Serilog 3.0.1
- Serilog.Extensions.Logging 8.0.0

**ProjectService.Api.csproj**:
- All of above +
- Swashbuckle.AspNetCore 6.4.8
- Microsoft.AspNetCore.OpenApi 8.0.0
- MediatR.Extensions.Microsoft.DependencyInjection 11.1.0
- Serilog.AspNetCore 8.0.1
- Serilog.Enrichers.Environment 3.0.1
- Serilog.Sinks.Console 5.0.0
- Serilog.Sinks.Seq 7.0.0
- OpenTelemetry.Exporter.Jaeger 1.7.0
- OpenTelemetry.Extensions.Hosting 1.7.0
- OpenTelemetry.Instrumentation.AspNetCore 1.7.1
- OpenTelemetry.Instrumentation.EntityFrameworkCore 1.0.0-beta.11
- OpenTelemetry.Instrumentation.Http 1.7.1

## Implementation Details

### CQRS Pattern Implementation

**Commands** (State-changing operations):
- All commands inherit from `IRequest<TResponse>`
- Handlers implement `IRequestHandler<TRequest, TResponse>`
- Registered automatically via MediatR assembly scanning

**Queries** (Read-only operations):
- All queries inherit from `IRequest<TResponse>`
- Handlers implement `IRequestHandler<TRequest, TResponse>`
- No side effects, optimized for performance

### Dependency Injection

The `DependencyInjection.cs` extension method:

```csharp
public static IServiceCollection AddInfrastructureServices(
    this IServiceCollection services,
    IConfiguration configuration)
{
    // DbContext
    services.AddDbContext<ProjectDbContext>(options =>
        options.UseNpgsql(configuration.GetConnectionString("ProjectServiceDatabase"))
    );
    
    // MediatR handlers
    services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ProjectHandlers).Assembly));
    
    return services;
}
```

### Cascade Deletes

When a project is deleted:
1. All ProjectVersions are automatically deleted
2. All ProjectArtifacts are automatically deleted
3. Data integrity maintained through foreign key constraints

## Testing Strategy

### Unit Testing (Coming Soon)
- Test each handler with mock DbContext
- Validate business logic
- Test edge cases (null values, duplicate names, etc.)

### Integration Testing (Coming Soon)
- Test full CQRS flow
- Verify database persistence
- Test cascade deletes
- Validate relationships

### End-to-End Testing (Coming Soon)
- Test all API endpoints
- Verify HTTP status codes
- Test error scenarios
- Validate response DTOs

## Gateway Integration

Add to YARP gateway configuration:

```csharp
routes:
  projectService:
    match:
      path: /api/projects/**
    forwardTo: ProjectService
    
clusters:
  ProjectService:
    destinations:
      default:
        address: http://project-service:5004
```

## Frontend Integration

Create Zustand store (`projectStore.ts`):

```typescript
import create from 'zustand';

interface Project {
  id: string;
  name: string;
  description?: string;
  framework: 'nextjs' | 'react' | 'html';
  createdAt: Date;
  versionCount: number;
}

export const useProjectStore = create((set) => ({
  projects: [] as Project[],
  
  createProject: async (data: CreateProjectRequest) => {
    const response = await fetch('/api/projects', {
      method: 'POST',
      body: JSON.stringify(data),
    });
    const project = await response.json();
    set((state) => ({ projects: [...state.projects, project.project] }));
  },
  
  getProjects: async (ownerId: string) => {
    const response = await fetch(`/api/projects/user/${ownerId}`);
    const projects = await response.json();
    set({ projects });
  },
  
  // ... more methods
}));
```

## Monitoring & Observability

### Logging

Serilog configured with:
- Console sink (structured logs)
- Seq sink (centralized log aggregation)
- Environment enrichment
- Request/response logging

### Tracing

OpenTelemetry configured with:
- Jaeger exporter (distributed tracing)
- AspNetCore instrumentation
- EntityFrameworkCore instrumentation
- HttpClient instrumentation

### Health Checks

ASP.NET Core HealthChecks endpoint:
- Route: `/health`
- Used by: Docker, Load Balancer, Service Mesh

## Performance Optimization

### Database Indexes
- OwnerId: Fast user project lookup
- ProjectId: Version queries
- VersionId: Artifact queries
- ArtifactId: Artifact resolution

### Query Optimization
- GetUserProjectsHandler uses `AsNoTracking()` for reads
- Version counts calculated via LINQ
- Pagination support (ready for future)

## Security

### Authorization
- OwnerId from JWT token claims
- Projects scoped to authenticated user
- No cross-user data access

### Input Validation
- Name length validation (coming)
- Framework enum validation
- Theme validation
- Description sanitization (coming)

## Error Handling

### Application Layer
- Custom exceptions for business logic errors
- Proper HTTP status codes
- Detailed error messages for debugging

### API Layer
- Global exception handler (coming)
- Validation error responses
- NOT FOUND (404) for missing projects
- BAD REQUEST (400) for invalid input

## Future Enhancements

1. **Pagination**
   - Add `Page` and `PageSize` to `GetUserProjectsQuery`
   - Return paginated results with total count

2. **Soft Deletes**
   - Add `DeletedAt` field to Project entity
   - Implement soft delete instead of hard delete
   - Add filter to exclude deleted projects

3. **Audit Trail**
   - Track who changed what and when
   - Add CreatedBy, ModifiedBy, ModifiedAt fields
   - Implement audit log table

4. **Project Templates**
   - Store project templates
   - Clone from template
   - Default versions

5. **Collaboration**
   - Share projects with other users
   - Permission levels (view, edit, admin)
   - Collaboration log

6. **Webhooks**
   - Notify external systems on project changes
   - Support for custom endpoints
   - Retry logic for failed deliveries

## Deployment

### Prerequisites
- PostgreSQL 15+ running on localhost:5432
- Network connectivity to Gateway (5500)
- Serilog Seq server (optional, localhost:5341)
- Jaeger exporter (optional, localhost:14268)

### Local Deployment
```bash
cd services/ProjectService
dotnet build
dotnet run --project src/ProjectService.Api
# Swagger UI: http://localhost:5004/swagger
```

### Docker Deployment
```bash
cd services/ProjectService
docker build -t project-service:1.0 .
docker run -p 5004:5004 \
  -e ConnectionStrings__ProjectServiceDatabase="Host=host.docker.internal;..." \
  project-service:1.0
```

### Docker Compose
See `infra/docker-compose.yml` for complete orchestration.

## Related Services

- **Auth Service**: Provides JWT tokens (OwnerId in claims)
- **Generator Service**: Creates artifacts linked in ProjectArtifact
- **Gateway**: Routes all project requests via /api/projects/**
- **Frontend**: Consumes Project endpoints for UI

## Support & Documentation

- **API Docs**: http://localhost:5004/swagger
- **Code**: `/services/ProjectService/src/`
- **Configuration**: `appsettings.json` and `appsettings.Development.json`
- **Logs**: Serilog console + Seq dashboard

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0.0 | 2024 | Initial implementation with 8 commands/queries |

## License

TechBirdsFly © 2024
