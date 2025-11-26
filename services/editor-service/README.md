# 🎯 Editor Service — CRUD + Section Regeneration

**Status:** ✅ **COMPLETE**

## Overview

The Editor Service manages sections of website projects. It allows you to:
- ✅ Create new sections
- ✅ Update section HTML/CSS
- ✅ Delete sections
- ✅ List all sections for a project
- ✅ Regenerate section HTML using Llama3

## Architecture

```
Domain Layer
├── Entities: Section
├── Interfaces: ISectionRepository
└── Exceptions: SectionNotFoundException

Application Layer
├── Features
│   ├── CreateSection
│   ├── UpdateSection
│   ├── DeleteSection
│   ├── ListSections
│   └── RegenerateSection
└── Interfaces: ISectionAIService

Infrastructure Layer
├── Persistence: EditorDbContext
├── Repositories: SectionRepository
├── AI: SectionAIService, ILlamaClient
└── DependencyInjection

WebAPI Layer
└── Controllers: EditorController
```

## API Endpoints

### List Sections
```
GET /api/editor/{projectId}
Response: List<Section>
```

### Create Section
```
POST /api/editor
Body: {
  "projectId": "guid",
  "type": "hero|features|testimonials|footer",
  "html": "<section>...</section>",
  "order": 1,
  "css": null
}
Response: { id: "guid" }
```

### Update Section
```
PUT /api/editor/{id}
Body: {
  "html": "<section>...</section>",
  "css": null
}
Response: { message: "Section updated successfully" }
```

### Delete Section
```
DELETE /api/editor/{id}
Response: { message: "Section deleted successfully" }
```

### Regenerate Section (AI)
```
POST /api/editor/regenerate/{id}
Response: { html: "<section>...</section>", message: "Section regenerated successfully" }
```

### Health Check
```
GET /api/editor/health
Response: { status: "healthy", timestamp: "..." }
```

## Running the Service

### Prerequisites
- .NET 8.0
- PostgreSQL running on localhost:5432
- Database: `techbirdsfly_editor`

### Build
```bash
cd services/editor-service/src
dotnet build EditorService.csproj -c Debug
```

### Run
```bash
ASPNETCORE_URLS="http://localhost:5008" dotnet run --configuration Debug
```

### Database Setup
The service automatically creates tables on startup.

```bash
# Optional: Manual migration
dotnet ef database update
```

## Testing

### Create a Section
```bash
curl -X POST http://localhost:5008/api/editor \
  -H "Content-Type: application/json" \
  -d '{
    "projectId": "550e8400-e29b-41d4-a716-446655440000",
    "type": "hero",
    "html": "<section class=\"py-20 bg-blue-600\">Hero Section</section>",
    "order": 1
  }'
```

### List Sections
```bash
curl http://localhost:5008/api/editor/550e8400-e29b-41d4-a716-446655440000
```

### Update Section
```bash
curl -X PUT http://localhost:5008/api/editor/{sectionId} \
  -H "Content-Type: application/json" \
  -d '{
    "html": "<section class=\"py-20 bg-purple-600\">Updated Hero</section>"
  }'
```

### Regenerate Section
```bash
curl -X POST http://localhost:5008/api/editor/regenerate/{sectionId}
```

## Database Schema

### Sections Table
| Column | Type | Notes |
|--------|------|-------|
| Id | UUID | Primary Key |
| ProjectId | UUID | FK to Projects |
| Type | VARCHAR(100) | hero, features, testimonials, etc. |
| Html | TEXT | HTML content |
| Css | VARCHAR(5000) | Optional custom CSS |
| Order | INT | Display order (1, 2, 3, ...) |
| CreatedAt | TIMESTAMP | Creation timestamp |
| UpdatedAt | TIMESTAMP | Last update timestamp |

**Indexes:**
- ProjectId
- (ProjectId, Order)

## Dependencies

- MediatR 12.0.1 — CQRS pattern
- Entity Framework Core 8.0 — ORM
- Npgsql 8.0 — PostgreSQL driver
- Swagger 6.4.0 — API documentation

## Configuration

**appsettings.json:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=techbirdsfly_editor;Username=postgres;Password=postgres"
  },
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://localhost:5008"
      }
    }
  }
}
```

## Integration with API Gateway

The Editor Service is routed through the API Gateway:

```
Frontend (3000)
  ↓
API Gateway (5500)
  ↓ /api/editor/**
  ↓
Editor Service (5008)
```

### Gateway Configuration
```json
{
  "Routes": {
    "editor-route": {
      "ClusterId": "editor-cluster",
      "Match": { "Path": "/api/editor/{**catch-all}" }
    }
  },
  "Clusters": {
    "editor-cluster": {
      "Destinations": {
        "destination1": { "Address": "http://localhost:5008" }
      }
    }
  }
}
```

## Next Steps

1. ✅ Editor Service is production-ready
2. Add to API Gateway routing
3. Integrate with Next.js frontend
4. Add authentication/authorization
5. Add caching layer for frequently accessed sections

---

**Status:** ✅ Complete and ready for deployment
