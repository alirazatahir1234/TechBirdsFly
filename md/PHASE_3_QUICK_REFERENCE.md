# Phase 3 Quick Reference - CQRS Queries & DTOs

## 🎯 What Was Built
Complete Application Layer with CQRS query pattern, AutoMapper DTOs, and proper dependency injection.

## 📦 DTOs Created

### ProjectDto
```csharp
public class ProjectDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Industry { get; set; }
    public required string Style { get; set; }
    public required string Description { get; set; }
    public required string PrimaryColor { get; set; }
    public required string SecondaryColor { get; set; }
    public required string AccentColor { get; set; }
    public int SectionCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<SectionDto> Sections { get; set; } = new();
}
```

### SectionDto
```csharp
public class SectionDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public required string Type { get; set; }  // "Hero", "Features", etc.
    public required string HtmlContent { get; set; }
    public required string CssClass { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

### GeneratedPageDto
```csharp
public class GeneratedPageDto
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public required string Html { get; set; }
    public required string Css { get; set; }
    public required string JavaScript { get; set; }
    public int Version { get; set; }
    public bool IsPublished { get; set; }
    public required string MetaTitle { get; set; }
    public required string MetaDescription { get; set; }
    public required string MetaKeywords { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

## 🔍 Queries Available

### 1. Get Single Project
```csharp
var query = new GetProjectQuery(projectId);
var response = await mediator.Send(query);  // returns GetProjectResponse
var projectDto = response.Project;  // ProjectDto or throws ResourceNotFoundException
```

### 2. Get All Projects (Paginated)
```csharp
var query = new GetAllProjectsQuery(skip: 0, take: 50);
var response = await mediator.Send(query);  // GetAllProjectsResponse
var projects = response.Projects;  // List<ProjectDto>
var total = response.Total;  // int - total count for UI pagination
```

### 3. Get Project Sections (Optional Filter)
```csharp
// Get all sections for a project
var query = new GetProjectSectionsQuery(projectId);

// Get only hero sections
var query = new GetProjectSectionsQuery(projectId, sectionType: "Hero");

var response = await mediator.Send(query);  // GetProjectSectionsResponse
var sections = response.Sections;  // List<SectionDto>
```

### 4. Get Generated Page
```csharp
var query = new GetGeneratedPageQuery(pageId);
var response = await mediator.Send(query);  // GetGeneratedPageResponse
var page = response.Page;  // GeneratedPageDto or throws ResourceNotFoundException
```

## 🗺️ AutoMapper Integration

### How It Works
1. **Automatic Discovery**: Profiles scanned and registered in DI container
2. **Entity → DTO**: ValueObjects flattened to primitive properties
3. **DTO → Entity**: Reconstructs aggregates with proper constructors

### Example Usage in Controller
```csharp
[HttpGet("{id}")]
public async Task<ActionResult<ProjectDto>> GetProject(Guid id)
{
    var response = await _mediator.Send(new GetProjectQuery(id));
    return Ok(response.Project);  // ProjectDto returned directly
}
```

## 🔗 Handler Chain
```
Controller Request
    ↓
MediatR Dispatcher
    ↓
Validation Behavior (FluentValidation)
    ↓
Logging Behavior (Serilog)
    ↓
Query Handler
    ├─ Repository Query
    ├─ Error Check (throws if not found)
    ├─ AutoMapper Conversion
    └─ Return DTO
    ↓
Performance Behavior (logs if >500ms)
    ↓
Response to Controller
```

## 📊 Pagination Pattern

### Request
```csharp
var query = new GetAllProjectsQuery(
    skip: 10,      // Start from 11th item
    take: 20       // Get 20 items
);
```

### Response
```csharp
new GetAllProjectsResponse(
    Projects: List<ProjectDto>,  // 20 items
    Total: 150                   // Total count in DB
)
```

### UI Implementation
```typescript
// Client-side pagination
const pageSize = 20;
const page = 1;
const response = await fetch(
  `/api/projects?skip=${(page-1)*pageSize}&take=${pageSize}`
);
const { projects, total } = await response.json();
const totalPages = Math.ceil(total / pageSize);
```

## 🚀 Ready for Phase 4
- ✅ All query contracts defined
- ✅ All DTOs structured
- ✅ AutoMapper profiles ready
- ✅ Dependency injection configured
- ✅ Repository contracts validated

Next: Implement EF Core + Database
