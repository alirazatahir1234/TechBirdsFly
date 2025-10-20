# Generator Service

The **Generator Service** is responsible for orchestrating AI-powered website generation, managing projects, and coordinating background jobs.

## Features

- **Project Management**: Create and track website generation projects
- **AI Integration**: Calls Azure OpenAI (mocked for MVP) to generate React landing pages
- **Job Queue**: Publishes jobs to message bus for async processing
- **ZIP Packaging**: Packages generated code as downloadable project files
- **Local Message Bus**: Uses simple in-memory queue for local development (RabbitMQ/Azure Service Bus integration pending)

## Endpoints

### Create Project
```http
POST /api/projects
Content-Type: application/json
X-User-Id: <user-guid>

{
  "name": "My Portfolio",
  "prompt": "Create a modern portfolio website for a software engineer"
}
```

**Response:**
```json
{
  "projectId": "550e8400-e29b-41d4-a716-446655440000",
  "jobId": "650e8400-e29b-41d4-a716-446655440001",
  "status": "pending",
  "message": "Project created and queued for generation"
}
```

### Get Project Status
```http
GET /api/projects/{projectId}
X-User-Id: <user-guid>
```

**Response:**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "name": "My Portfolio",
  "status": "pending",
  "previewUrl": null,
  "artifactUrl": null,
  "jobStatus": "queued",
  "createdAt": "2025-10-16T12:00:00Z"
}
```

### Download Project
```http
GET /api/projects/{projectId}/download
X-User-Id: <user-guid>
```

## Running Locally

### Prerequisites
- .NET 8 SDK
- Optional: RabbitMQ (for production message queue)

### Development

```bash
cd services/generator-service/GeneratorService
dotnet restore
dotnet ef database update
dotnet run --urls http://localhost:5003
```

The service will:
1. Restore NuGet packages
2. Apply EF Core migrations (creates SQLite database)
3. Start on http://localhost:5003
4. Swagger UI available at http://localhost:5003/swagger

## Database

- **Type**: SQLite (local dev)
- **File**: `generator.db` (created on first run)
- **Tables**: 
  - `Projects` - website generation projects
  - `GenerateWebsiteJobs` - async job tracking

## Architecture

### Models

**Project**
- `Id` (Guid) - unique identifier
- `UserId` (Guid) - project owner
- `Name` (string) - project name
- `Prompt` (string) - AI generation prompt
- `Status` (string) - pending, processing, completed, failed
- `PreviewUrl` (string?) - URL to live preview
- `ArtifactUrl` (string?) - URL to downloadable ZIP
- `CreatedAt` (DateTime) - creation timestamp

**GenerateWebsiteJob**
- `Id` (Guid) - job identifier
- `ProjectId` (Guid) - associated project
- `UserId` (Guid) - job owner
- `Prompt` (string) - generation prompt
- `Status` (string) - queued, processing, completed, failed
- `GeneratedCode` (string?) - output React component code
- `ErrorMessage` (string?) - error details if failed
- `CreatedAt` (DateTime)
- `CompletedAt` (DateTime?)

### Services

**IGeneratorService**
- `GenerateWebsiteCodeAsync(prompt)` - mocked AI call for MVP
- `PackageProjectAsZipAsync(name, code)` - packages generated code as ZIP

**IMessagePublisher**
- `PublishJobAsync<T>(queue, message)` - publishes job to message bus
- Current: LocalMessagePublisher (logs to console)
- Future: RabbitMQPublisher, Azure Service Bus

## Next Steps

1. **Integrate Real Azure OpenAI**: Replace mocked code generation with actual GPT-4o-mini calls
2. **Add Background Worker**: Implement hosted service to process jobs from queue
3. **Blob Storage Integration**: Store generated ZIPs in Azure Blob Storage
4. **RabbitMQ Integration**: Wire up RabbitMQ for production message queue
5. **Image Generation**: Add DALL·E integration for hero image generation

## Configuration

Environment variables (appsettings.Development.json):

```json
{
  "Logging": {
    "LogLevel": { "Default": "Information" }
  },
  "RabbitMQ": {
    "Host": "localhost"
  },
  "ConnectionStrings": {
    "GeneratorDb": "Data Source=generator.db"
  }
}
```

## Testing

### Test Create Project
```bash
curl -X POST http://localhost:5003/api/projects \
  -H "Content-Type: application/json" \
  -H "X-User-Id: 12345678-1234-5678-1234-567812345678" \
  -d '{"name":"Test Site","prompt":"Create a landing page for a tech startup"}'
```

### Test Get Status
```bash
curl http://localhost:5003/api/projects/{projectId} \
  -H "X-User-Id: 12345678-1234-5678-1234-567812345678"
```

## Deployment

### Docker Build
```bash
docker build -f services/generator-service/Dockerfile -t techbirdsfly/generator-service:latest .
```

### Docker Run
```bash
docker run -p 5003:80 \
  -e ConnectionStrings__GeneratorDb="Server=db;Database=GeneratorDb;..." \
  -e ASPNETCORE_ENVIRONMENT=Production \
  techbirdsfly/generator-service:latest
```

---

**Status**: MVP Complete ✅ | **Next**: Background Worker Implementation
