# PROJECT SERVICE - COMPLETE IMPLEMENTATION

## 🎯 Overview

The **Project Service** is the brain of TechBirdsFly's AI Website Builder. It manages:
- **Project Lifecycle**: Create, read, update, delete user projects
- **Version Control**: Automatic versioning (v1, v2, v3...) for website regenerations
- **HTML Storage**: Complete HTML content for each version
- **Metadata Management**: Industry, style, palette, and other project attributes
- **User Isolation**: Projects are scoped to individual users via UserId

**Port**: 5010  
**Database**: PostgreSQL (TechBirdsFly_Project)  
**Architecture**: Clean Architecture (4 layers)  
**Pattern**: CQRS with MediatR

---

## 📊 Architecture

```
┌─────────────────────────────────────────────────────────┐
│                    WebAPI Layer                          │
│         (ProjectsController - 6 Endpoints)              │
└──────────────────┬──────────────────────────────────────┘
                   │
┌──────────────────▼──────────────────────────────────────┐
│              Application Layer                           │
│  • MediatR Commands & Handlers (5 handlers)             │
│  • Queries for retrieving data                          │
│  • DTOs for data transfer                               │
└──────────────────┬──────────────────────────────────────┘
                   │
┌──────────────────▼──────────────────────────────────────┐
│              Domain Layer                                │
│  • Project entity (aggregate root)                       │
│  • ProjectVersion entity                                 │
│  • Repository contracts                                  │
│  • Domain exceptions                                     │
└──────────────────┬──────────────────────────────────────┘
                   │
┌──────────────────▼──────────────────────────────────────┐
│            Infrastructure Layer                          │
│  • Entity Framework Core DbContext                       │
│  • Repository implementations                            │
│  • Database migrations                                   │
│  • Dependency injection configuration                    │
└─────────────────────────────────────────────────────────┘
```

---

## 📁 Project Structure

```
services/project-service/
├── src/
│   └── TechBirdsFly.ProjectService/
│       ├── Domain/
│       │   ├── Entities/
│       │   │   ├── BaseEntity.cs
│       │   │   ├── Project.cs
│       │   │   └── ProjectVersion.cs
│       │   ├── Repositories/
│       │   │   ├── IProjectRepository.cs
│       │   │   └── IVersionRepository.cs
│       │   └── Exceptions/
│       │       └── ProjectNotFoundException.cs
│       │
│       ├── Application/
│       │   ├── DTOs/
│       │   │   ├── ProjectDto.cs
│       │   │   └── ProjectListDto.cs
│       │   └── Features/
│       │       ├── CreateProject/
│       │       │   ├── CreateProjectCommand.cs
│       │       │   └── CreateProjectHandler.cs
│       │       ├── SaveVersion/
│       │       │   ├── SaveVersionCommand.cs
│       │       │   └── SaveVersionHandler.cs
│       │       ├── GetProject/
│       │       │   ├── GetProjectQuery.cs
│       │       │   └── GetProjectHandler.cs
│       │       ├── ListProjects/
│       │       │   ├── ListProjectsQuery.cs
│       │       │   └── ListProjectsHandler.cs
│       │       └── DeleteProject/
│       │           ├── DeleteProjectCommand.cs
│       │           └── DeleteProjectHandler.cs
│       │
│       ├── Infrastructure/
│       │   └── Persistence/
│       │       ├── ProjectDbContext.cs
│       │       ├── ProjectRepository.cs
│       │       ├── VersionRepository.cs
│       │       └── DependencyInjection.cs
│       │
│       ├── WebAPI/
│       │   ├── Controllers/
│       │   │   └── ProjectsController.cs
│       │   ├── Program.cs
│       │   ├── appsettings.json
│       │   └── appsettings.Development.json
│       │
│       └── ProjectService.csproj
│
└── tests/
    └── ProjectService.Tests/
        └── (Unit tests - Coming soon)
```

---

## 🗄️ Database Schema

### Projects Table
```sql
CREATE TABLE Projects (
    Id UUID PRIMARY KEY,
    UserId UUID NOT NULL,
    Name VARCHAR(255) NOT NULL,
    Industry VARCHAR(100) NOT NULL,
    Style VARCHAR(100) NOT NULL,
    Palette VARCHAR(100) NOT NULL,
    CreatedAt TIMESTAMP NOT NULL,
    UpdatedAt TIMESTAMP NULL
);

CREATE INDEX IX_Projects_UserId ON Projects(UserId);
CREATE INDEX IX_Projects_CreatedAt ON Projects(CreatedAt);
```

### ProjectVersions Table
```sql
CREATE TABLE ProjectVersions (
    Id UUID PRIMARY KEY,
    ProjectId UUID NOT NULL REFERENCES Projects(Id) ON DELETE CASCADE,
    VersionNumber INT NOT NULL,
    Html TEXT NOT NULL,
    CreatedAt TIMESTAMP NOT NULL
);

CREATE INDEX IX_ProjectVersions_ProjectId ON ProjectVersions(ProjectId);
CREATE INDEX IX_ProjectVersions_ProjectId_VersionNumber ON ProjectVersions(ProjectId, VersionNumber);
```

---

## 🔄 CQRS Flow

### 1. CreateProject Command
**Purpose**: Create a new project with initial HTML (v1)

```csharp
public record CreateProjectCommand(
    Guid UserId,
    string Name,
    string Industry,
    string Style,
    string Palette,
    string Html
) : IRequest<Guid>;
```

**Flow**:
1. Handler creates Project aggregate root
2. Automatically creates ProjectVersion (v1)
3. Persists both to database
4. Returns project ID

**Response**: `Guid` (Project ID)

---

### 2. SaveVersion Command
**Purpose**: Save a new version of existing project

```csharp
public record SaveVersionCommand(
    Guid ProjectId,
    string Html
) : IRequest<int>;
```

**Flow**:
1. Handler fetches last version number
2. Increments version (v1 → v2 → v3)
3. Creates new ProjectVersion entity
4. Persists to database
5. Returns new version number

**Response**: `int` (New version number)

**Example**: 
- v1: Initial generation
- v2: User regenerates with different prompts
- v3: Another regeneration iteration

---

### 3. GetProject Query
**Purpose**: Retrieve project with latest version HTML

```csharp
public record GetProjectQuery(Guid ProjectId) : IRequest<ProjectDto>;
```

**Response**:
```json
{
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "name": "My AI Website",
    "industry": "Technology",
    "style": "Modern",
    "palette": "Blue-White",
    "html": "<html>...</html>",
    "currentVersion": 3,
    "createdAt": "2024-01-15T10:30:00Z",
    "updatedAt": "2024-01-15T14:45:00Z"
}
```

---

### 4. ListProjects Query
**Purpose**: Retrieve all projects for a specific user

```csharp
public record ListProjectsQuery(Guid UserId) : IRequest<List<ProjectListDto>>;
```

**Response**:
```json
{
    "success": true,
    "data": [
        {
            "id": "550e8400-e29b-41d4-a716-446655440000",
            "name": "My AI Website",
            "industry": "Technology",
            "style": "Modern",
            "palette": "Blue-White",
            "createdAt": "2024-01-15T10:30:00Z"
        },
        {
            "id": "660e8400-e29b-41d4-a716-446655440001",
            "name": "E-Commerce Store",
            "industry": "Retail",
            "style": "Professional",
            "palette": "Green-White",
            "createdAt": "2024-01-16T09:15:00Z"
        }
    ],
    "message": "Retrieved 2 projects"
}
```

---

### 5. DeleteProject Command
**Purpose**: Delete a project and all its versions

```csharp
public record DeleteProjectCommand(Guid ProjectId) : IRequest<bool>;
```

**Response**: `bool` (Success status)

**Note**: Cascade delete removes all associated versions automatically.

---

## 🔌 REST API Endpoints

### Base URL
```
http://localhost:5010/api/projects
```

---

### 1️⃣ Health Check
```http
GET /health/status
```

**Response** (200 OK):
```json
{
    "status": "healthy",
    "timestamp": "2024-01-15T15:30:00Z"
}
```

---

### 2️⃣ Create Project
```http
POST /create
Content-Type: application/json

{
    "userId": "550e8400-e29b-41d4-a716-446655440000",
    "name": "My AI Website",
    "industry": "Technology",
    "style": "Modern",
    "palette": "Blue-White",
    "html": "<html><body><h1>Welcome</h1></body></html>"
}
```

**Response** (201 Created):
```json
{
    "success": true,
    "data": "550e8400-e29b-41d4-a716-446655440000",
    "message": "Project created successfully"
}
```

**Error** (400 Bad Request):
```json
{
    "success": false,
    "message": "Invalid request parameters"
}
```

---

### 3️⃣ Get Project with Latest Version
```http
GET /{projectId}
```

**Example**:
```
GET /550e8400-e29b-41d4-a716-446655440000
```

**Response** (200 OK):
```json
{
    "success": true,
    "data": {
        "id": "550e8400-e29b-41d4-a716-446655440000",
        "name": "My AI Website",
        "industry": "Technology",
        "style": "Modern",
        "palette": "Blue-White",
        "html": "<html><body><h1>Welcome</h1></body></html>",
        "currentVersion": 3,
        "createdAt": "2024-01-15T10:30:00Z",
        "updatedAt": "2024-01-15T14:45:00Z"
    }
}
```

**Error** (404 Not Found):
```json
{
    "success": false,
    "message": "Project not found"
}
```

---

### 4️⃣ List User Projects
```http
GET /user/{userId}
```

**Example**:
```
GET /user/550e8400-e29b-41d4-a716-446655440000
```

**Response** (200 OK):
```json
{
    "success": true,
    "data": [
        {
            "id": "550e8400-e29b-41d4-a716-446655440000",
            "name": "My AI Website",
            "industry": "Technology",
            "style": "Modern",
            "palette": "Blue-White",
            "createdAt": "2024-01-15T10:30:00Z"
        }
    ],
    "message": "Retrieved 1 projects"
}
```

---

### 5️⃣ Save New Version
```http
POST /{projectId}/versions
Content-Type: application/json

{
    "html": "<html><body><h1>Updated Content</h1></body></html>"
}
```

**Example**:
```
POST /550e8400-e29b-41d4-a716-446655440000/versions
```

**Response** (201 Created):
```json
{
    "success": true,
    "data": 2,
    "message": "Version 2 saved"
}
```

---

### 6️⃣ Delete Project
```http
DELETE /{projectId}
```

**Example**:
```
DELETE /550e8400-e29b-41d4-a716-446655440000
```

**Response** (200 OK):
```json
{
    "success": true,
    "data": true,
    "message": "Project deleted successfully"
}
```

---

## 🚀 Getting Started

### Prerequisites
- .NET 8.0 SDK
- PostgreSQL 14+
- Docker (optional, for containerization)

### 1. Database Setup
```bash
# Create database
createdb techbirdsfly_project

# Update connection string in appsettings.json
# Default: "Host=localhost;Database=techbirdsfly_project;Username=postgres;Password=password"
```

### 2. Run Migrations
```bash
cd services/project-service/src

dotnet ef database update -c ProjectDbContext
```

### 3. Start the Service
```bash
cd services/project-service/src

dotnet run
# Service starts on http://localhost:5010
```

### 4. Verify Service
```bash
curl http://localhost:5010/api/projects/health/status
```

---

## 🧪 Testing

### Using cURL

**Create Project**:
```bash
curl -X POST http://localhost:5010/api/projects/create \
  -H "Content-Type: application/json" \
  -d '{
    "userId": "550e8400-e29b-41d4-a716-446655440000",
    "name": "My Website",
    "industry": "Tech",
    "style": "Modern",
    "palette": "Blue",
    "html": "<h1>Hello</h1>"
  }'
```

**List Projects**:
```bash
curl http://localhost:5010/api/projects/user/550e8400-e29b-41d4-a716-446655440000
```

**Save Version**:
```bash
curl -X POST http://localhost:5010/api/projects/{projectId}/versions \
  -H "Content-Type: application/json" \
  -d '{"html": "<h1>Updated</h1>"}'
```

### Using Test Script
```bash
chmod +x test-project-service.sh
./test-project-service.sh
```

---

## 📊 Key Features

### ✅ User Isolation
- Every project is tied to a specific UserId
- Users can only access their own projects
- `ListProjects` filters by UserId automatically

### ✅ Automatic Versioning
- First save creates v1
- Each subsequent save increments version
- No manual version management needed
- Full history preserved

### ✅ Clean Architecture
- Domain: Pure business logic
- Application: Use case handlers
- Infrastructure: Data access
- WebAPI: HTTP contracts

### ✅ CQRS Pattern
- Commands for state changes (Create, Save, Delete)
- Queries for data retrieval (Get, List)
- MediatR mediates all requests
- Easy to extend with new operations

### ✅ Entity Framework Core
- Fluent API configuration
- Automatic migrations support
- Cascade delete handling
- Strategic indexing for performance

### ✅ Error Handling
- Custom ProjectNotFoundException
- Try-catch blocks on all endpoints
- Logging with Serilog
- Meaningful HTTP status codes

---

## 🔌 Integration Points

### Gateway Integration
The Project Service integrates with YARP Gateway on port 5010.

**Gateway Route**:
```yaml
/api/projects -> http://localhost:5010/api/projects
```

### Frontend Integration
Next.js frontend makes requests to:
```
GET  /api/projects/{projectId}          # Get project with latest version
GET  /api/projects/user/{userId}        # List all user projects
POST /api/projects/create               # Create new project
POST /api/projects/{projectId}/versions # Save new version
DELETE /api/projects/{projectId}        # Delete project
```

### Event Bus Integration (Future)
- ProjectCreated events
- ProjectVersionSaved events
- ProjectDeleted events

---

## 📈 Performance

### Indexing Strategy
```sql
-- Indexed for fast user project lookups
CREATE INDEX IX_Projects_UserId ON Projects(UserId);

-- Indexed for sorting by creation date
CREATE INDEX IX_Projects_CreatedAt ON Projects(CreatedAt);

-- Indexed for version lookups
CREATE INDEX IX_ProjectVersions_ProjectId ON ProjectVersions(ProjectId);
CREATE INDEX IX_ProjectVersions_ProjectId_VersionNumber ON ProjectVersions(ProjectId, VersionNumber);
```

### Query Optimization
- Cascade delete prevents orphaned versions
- Composite indexes for version lookups
- Eager loading where appropriate
- Async/await for non-blocking I/O

---

## 🛠️ Configuration

### appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=techbirdsfly_project;Username=postgres;Password=password"
  },
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://localhost:5010"
      }
    }
  },
  "Serilog": {
    "MinimumLevel": "Information"
  }
}
```

### Program.cs Setup
```csharp
// Service registration
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<IVersionRepository, VersionRepository>();

// DbContext
builder.Services.AddDbContext<ProjectDbContext>(options =>
    options.UseNpgsql(connectionString));

// MediatR
builder.Services.AddMediatR(config => 
    config.RegisterServicesFromAssembly(typeof(Program).Assembly));
```

---

## 📝 Dependencies

- **Microsoft.EntityFrameworkCore** v8.0.0
- **Npgsql.EntityFrameworkCore.PostgreSQL** v8.0.0
- **MediatR** v12.1.1
- **Serilog.AspNetCore** v8.0.0
- **Swashbuckle.AspNetCore** v6.4.6
- **Microsoft.AspNetCore.OpenApi** v8.0.0

---

## 🎓 Comparable Systems

The Project Service is comparable to:
- **Wix ADI**: Stores generated websites and versions
- **Framer AI**: Version history for AI-generated designs
- **Squarespace Blueprint**: Project management and templates
- **Base44**: Design versioning system
- **Durable**: Project storage and iteration

---

## 📞 Support

For issues or questions:
1. Check logs in `bin/Debug/net8.0/logs/`
2. Verify database connection
3. Ensure migrations are applied
4. Check service is running on port 5010

---

## 📄 License

Part of TechBirdsFly AI Website Builder  
© 2024 All Rights Reserved
