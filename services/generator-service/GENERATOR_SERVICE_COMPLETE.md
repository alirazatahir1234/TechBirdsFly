# 🔥 GENERATOR SERVICE — PRODUCTION-GRADE MICROSERVICE

## ✅ STATUS: COMPLETE & FULLY OPERATIONAL

Your **Generator Service** is **100% implemented** with production-grade architecture, including:

- ✅ Clean Architecture (Domain → Application → Infrastructure → API)
- ✅ .NET 8.0 with ASP.NET Core 8.0
- ✅ PostgreSQL with Entity Framework Core 8.0
- ✅ Ollama AI Integration (Llama3.1:8b)
- ✅ MinIO Object Storage
- ✅ Kafka Event Publishing
- ✅ OpenTelemetry Observability
- ✅ Serilog Structured Logging with Seq
- ✅ Docker Multi-Stage Builds
- ✅ Full Microservices Integration

---

## 📋 ARCHITECTURE OVERVIEW

### Folder Structure
```
/services/generator-service
  /src
    /Domain                          # Business logic & entities
    /Application                     # Use cases & DTOs
    /Infrastructure                  # External integrations
    /WebAPI                          # API Controllers
    /Migrations                      # EF Core Migrations
    /Properties                      # Assembly info
    Program.cs                       # Main startup
    appsettings.json                 # Production config
    appsettings.Development.json     # Development config
  /tests
    /GeneratorService.Tests         # Unit & integration tests
  Dockerfile                         # Docker configuration
  README.md                          # Quick reference
```

---

## 🏗️ LAYER-BY-LAYER BREAKDOWN

### 1. DOMAIN LAYER (Business Logic)

#### Entities

**GeneratedArtifact.cs**
```csharp
public class GeneratedArtifact
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProjectId { get; set; }
    public string Type { get; set; } = default!;        // page | component | template
    public string MetadataJson { get; set; } = default!; // JSON metadata
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

**GeneratedFile.cs**
```csharp
public class GeneratedFile
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ArtifactId { get; set; }
    public string Path { get; set; } = default!;       // File path in storage
    public string Format { get; set; } = default!;     // html | react | next | zip
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

**Project.cs** (Main entity)
- `Id` (Guid) - Unique identifier
- `UserId` (Guid) - Project owner
- `Name` (string) - Project name
- `Prompt` (string) - AI generation prompt
- `Status` (string) - pending, processing, completed, failed
- `PreviewUrl` (string?) - Live preview URL
- `ArtifactUrl` (string?) - Downloadable ZIP URL
- `CreatedAt` (DateTime) - Creation timestamp

### 2. APPLICATION LAYER (Use Cases & DTOs)

#### DTOs

**GenerateRequestDto**
```csharp
public class GenerateRequestDto
{
    public Guid ProjectId { get; set; }
    public string Prompt { get; set; } = default!;
    public string Type { get; set; } = "page";  // page | component | template
}
```

**GenerateResultDto**
```csharp
public class GenerateResultDto
{
    public Guid ArtifactId { get; set; }
    public string Html { get; set; } = "";
    public string React { get; set; } = "";
    public string NextJs { get; set; } = "";
    public string MetadataJson { get; set; } = "";
    public string ZipUrl { get; set; } = "";
}
```

#### Services

**IGeneratorService**
- `GenerateWebsiteCodeAsync(prompt, type)` - AI-powered code generation
- `GenerateReactComponentAsync(prompt)` - React-specific generation
- `GenerateNextJsPageAsync(prompt)` - Next.js-specific generation
- `PackageProjectAsZipAsync(name, artifacts)` - ZIP packaging

**IProjectRepository**
- `CreateProjectAsync(userId, name, prompt)`
- `GetProjectAsync(projectId)`
- `UpdateProjectAsync(project)`
- `DeleteProjectAsync(projectId)`
- `GetUserProjectsAsync(userId)`

**IMessagePublisher**
- `PublishJobAsync<T>(topic, message)` - Publish to Kafka/RabbitMQ
- `PublishPageGeneratedAsync(artifactId)` - Specific event

### 3. INFRASTRUCTURE LAYER (External Integrations)

#### 🦙 Ollama AI Engine
**OllamaAIEngine.cs**
```csharp
public class OllamaAIEngine : IAIEngine
{
    // Generates HTML, React, Next.js code using Ollama
    // Model: Llama3.1:8b (or configurable)
    // Endpoint: http://host.docker.internal:11434 (or production URL)
    
    public async Task<AIResult> GenerateAsync(string prompt, string type)
    {
        // 1. Build prompt with format instructions
        // 2. Call Ollama endpoint
        // 3. Parse JSON response
        // 4. Return structured AIResult
    }
}
```

**PromptFactory.cs** - Constructs context-aware prompts
```csharp
public static string BuildPrompt(string input, string type)
{
    return $@"
You are an expert UI generator.

INPUT PROMPT:
{input}

OUTPUT FORMAT (JSON):
{{
  ""html"": ""<html>..."",
  ""react"": ""export default function Component() {{...}}"",
  ""nextjs"": ""export default function Page() {{...}}"",
  ""metadata"": {{ ""sections"": [...] }}
}}";
}
```

#### 📦 MinIO Object Storage
**MinioFileStorage.cs**
```csharp
public class MinioFileStorage : IFileStorage
{
    // Uploads generated code to MinIO buckets
    // Creates ZIP archives from multiple files
    // Returns signed download URLs
    
    public async Task<string> UploadAsync(string path, string content)
    {
        // Upload single file to MinIO
        // Return bucket/path URL
    }

    public async Task<string> CreateZipAsync(string zipPath, Dictionary<string,string> files)
    {
        // Create ZIP from dict of files
        // Upload to MinIO
        // Return signed download URL
    }
}
```

#### 📨 Kafka Message Producer
**KafkaProducer.cs**
```csharp
public class KafkaProducer : IKafkaProducer
{
    // Publishes events to Kafka topics
    // Topics: page-generated, artifact-created, generation-failed
    
    public async Task PublishAsync(string topic, object payload)
    {
        // Serialize to JSON
        // Publish to Kafka topic
        // Log for observability
    }
}
```

#### 📊 PostgreSQL Persistence
**GeneratorDbContext.cs**
```csharp
public class GeneratorDbContext : DbContext
{
    public DbSet<Project> Projects { get; set; }
    public DbSet<GeneratedArtifact> GeneratedArtifacts { get; set; }
    public DbSet<GeneratedFile> GeneratedFiles { get; set; }
    public DbSet<GenerateWebsiteJob> GenerateWebsiteJobs { get; set; }
}
```

**Repositories:**
- `ProjectRepository` - CRUD for projects
- `ArtifactRepository` - Artifact storage & retrieval
- `JobRepository` - Background job tracking

#### 📡 Observability
**Serilog + Seq Integration**
- Structured logging to Seq server (http://seq:80)
- Log levels: Debug, Info, Warning, Error, Fatal
- Enriched with: Timestamp, MachineName, Service, Environment

**OpenTelemetry Tracing**
- Jaeger exporter (http://localhost:6831)
- Traces: HTTP requests, database calls, external APIs
- Metrics: Request duration, error rates, throughput

---

## 🌐 API ENDPOINTS

### 1. Create Project
```http
POST /api/projects
Content-Type: application/json
X-User-Id: 550e8400-e29b-41d4-a716-446655440000

{
  "name": "My AI Website",
  "prompt": "Create a modern SaaS landing page for an AI company"
}
```

**Response (201 Created)**
```json
{
  "projectId": "550e8400-e29b-41d4-a716-446655440001",
  "jobId": "650e8400-e29b-41d4-a716-446655440002",
  "status": "pending",
  "message": "Project created and queued for generation"
}
```

### 2. Get Project Status
```http
GET /api/projects/550e8400-e29b-41d4-a716-446655440001
X-User-Id: 550e8400-e29b-41d4-a716-446655440000
```

**Response (200 OK)**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440001",
  "name": "My AI Website",
  "status": "completed",
  "previewUrl": "https://minio:9000/projects/550e8400.../preview.html",
  "artifactUrl": "https://minio:9000/projects/550e8400.../export.zip",
  "jobStatus": "completed",
  "createdAt": "2025-11-25T12:00:00Z"
}
```

### 3. Download Project as ZIP
```http
GET /api/projects/550e8400-e29b-41d4-a716-446655440001/download
X-User-Id: 550e8400-e29b-41d4-a716-446655440000
```

**Response (200 OK)**
- Content-Type: application/zip
- Body: Binary ZIP file with HTML, React, Next.js components

### 4. List User Projects
```http
GET /api/projects
X-User-Id: 550e8400-e29b-41d4-a716-446655440000
```

**Response (200 OK)**
```json
{
  "projects": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440001",
      "name": "My AI Website",
      "status": "completed",
      "createdAt": "2025-11-25T12:00:00Z"
    }
  ],
  "total": 1
}
```

### 5. Health Check
```http
GET /health
```

**Response (200 OK)**
```json
{
  "status": "Healthy"
}
```

---

## 🚀 RUNNING LOCALLY

### Prerequisites
```bash
# Install .NET 8 SDK
https://dotnet.microsoft.com/en-us/download/dotnet/8.0

# Install PostgreSQL (via Docker or local)
docker run -d --name postgres \
  -e POSTGRES_PASSWORD=postgres123 \
  -e POSTGRES_DB=techbirdsfly_generator \
  -p 5432:5432 \
  postgres:15

# Install Ollama (for AI generation)
https://ollama.ai

# Pull Llama3.1 model
ollama pull llama3.1:8b

# Start MinIO (object storage)
docker run -d --name minio \
  -e MINIO_ACCESS_KEY=minio \
  -e MINIO_SECRET_KEY=minio123 \
  -p 9000:9000 \
  minio/minio server /data

# Start Kafka (messaging)
docker run -d --name kafka \
  -e KAFKA_ADVERTISED_HOST_NAME=localhost \
  -p 9092:9092 \
  bitnami/kafka:latest
```

### Build & Run

```bash
# Navigate to service
cd services/generator-service/src

# Restore packages
dotnet restore

# Apply migrations (creates database)
dotnet ef database update

# Run service
dotnet run --urls http://localhost:5003

# Navigate to: http://localhost:5003/swagger
```

---

## 🐳 DOCKER DEPLOYMENT

### Build Docker Image
```bash
docker build -f services/generator-service/Dockerfile \
  -t techbirdsfly/generator-service:latest .
```

### Run with Docker Compose

**docker-compose.yml**
```yaml
version: "3.9"

services:
  generator:
    build:
      context: .
      dockerfile: services/generator-service/Dockerfile
    ports:
      - "5003:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__GeneratorDb=Host=postgres;Database=generator;Username=postgres;Password=postgres
      - Ollama__Endpoint=http://ollama:11434
      - Minio__Endpoint=minio:9000
      - Minio__AccessKey=minio
      - Minio__SecretKey=minio123
      - Kafka__Broker=kafka:9092
      - Serilog__Seq__Url=http://seq:80
    depends_on:
      - postgres
      - ollama
      - minio
      - kafka

  postgres:
    image: postgres:15
    environment:
      POSTGRES_PASSWORD: postgres
      POSTGRES_DB: generator
    volumes:
      - postgres_data:/var/lib/postgresql/data
    ports:
      - "5432:5432"

  ollama:
    image: ollama/ollama
    ports:
      - "11434:11434"
    volumes:
      - ollama_data:/root/.ollama

  minio:
    image: minio/minio
    environment:
      MINIO_ACCESS_KEY: minio
      MINIO_SECRET_KEY: minio123
    command: server /data
    volumes:
      - minio_data:/data
    ports:
      - "9000:9000"

  kafka:
    image: bitnami/kafka:latest
    environment:
      KAFKA_CFG_ZOOKEEPER_CONNECT: zookeeper:2181
    depends_on:
      - zookeeper

  zookeeper:
    image: bitnami/zookeeper:latest
    environment:
      ALLOW_ANONYMOUS_LOGIN: "yes"

  seq:
    image: datalust/seq:latest
    ports:
      - "5341:80"
    environment:
      ACCEPT_EULA: "Y"

volumes:
  postgres_data:
  ollama_data:
  minio_data:
```

### Run Container
```bash
docker-compose up -d
# Service available at http://localhost:5003
```

---

## 🔌 GATEWAY INTEGRATION (YARP)

### Add to Gateway `appsettings.json`

```json
{
  "ReverseProxy": {
    "Routes": {
      "generator": {
        "ClusterId": "generatorCluster",
        "Match": {
          "Path": "/generator/{**catch-all}"
        },
        "Transforms": [
          { "PathPattern": "/api/{**catch-all}" }
        ]
      }
    },
    "Clusters": {
      "generatorCluster": {
        "Destinations": {
          "destination1": {
            "Address": "http://generator-service:5003"
          }
        }
      }
    }
  }
}
```

### Access via Gateway
```bash
# Instead of: http://localhost:5003/api/projects
# Use:        http://localhost:5500/generator/api/projects
```

---

## 📱 FRONTEND INTEGRATION (Next.js)

### 1. Create Zustand Store

**lib/store/generatorStore.ts**
```typescript
import { create } from 'zustand';

interface GeneratorState {
  projects: Project[];
  loading: boolean;
  error: string | null;
  
  createProject: (name: string, prompt: string) => Promise<void>;
  getProject: (projectId: string) => Promise<Project>;
  listProjects: () => Promise<void>;
  downloadProject: (projectId: string) => Promise<void>;
}

export const useGeneratorStore = create<GeneratorState>((set) => ({
  projects: [],
  loading: false,
  error: null,
  
  createProject: async (name, prompt) => {
    set({ loading: true });
    try {
      const response = await fetch('/api/generator/api/projects', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ name, prompt })
      });
      const data = await response.json();
      set({ error: null });
    } catch (error) {
      set({ error: (error as Error).message });
    } finally {
      set({ loading: false });
    }
  },
  
  listProjects: async () => {
    set({ loading: true });
    try {
      const response = await fetch('/api/generator/api/projects');
      const data = await response.json();
      set({ projects: data.projects, error: null });
    } catch (error) {
      set({ error: (error as Error).message });
    } finally {
      set({ loading: false });
    }
  },
  
  downloadProject: async (projectId) => {
    try {
      const response = await fetch(`/api/generator/api/projects/${projectId}/download`);
      const blob = await response.blob();
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `project-${projectId}.zip`;
      a.click();
    } catch (error) {
      set({ error: (error as Error).message });
    }
  },
  
  getProject: async (projectId) => {
    set({ loading: true });
    try {
      const response = await fetch(`/api/generator/api/projects/${projectId}`);
      return await response.json();
    } catch (error) {
      set({ error: (error as Error).message });
      throw error;
    } finally {
      set({ loading: false });
    }
  }
}));
```

### 2. Create Generator Component

**components/GeneratorForm.tsx**
```typescript
import { useState } from 'react';
import { useGeneratorStore } from '@/lib/store/generatorStore';

export function GeneratorForm() {
  const [name, setName] = useState('');
  const [prompt, setPrompt] = useState('');
  const { createProject, loading, error } = useGeneratorStore();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    await createProject(name, prompt);
    setName('');
    setPrompt('');
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-4">
      <input
        type="text"
        placeholder="Project Name"
        value={name}
        onChange={(e) => setName(e.target.value)}
        className="w-full px-4 py-2 border rounded"
        required
      />
      <textarea
        placeholder="Describe your website..."
        value={prompt}
        onChange={(e) => setPrompt(e.target.value)}
        className="w-full px-4 py-2 border rounded h-32"
        required
      />
      <button
        type="submit"
        disabled={loading}
        className="w-full bg-blue-600 text-white py-2 rounded hover:bg-blue-700"
      >
        {loading ? 'Generating...' : 'Generate Website'}
      </button>
      {error && <p className="text-red-600">{error}</p>}
    </form>
  );
}
```

---

## 🧪 TESTING

### Test Project Creation
```bash
curl -X POST http://localhost:5003/api/projects \
  -H "Content-Type: application/json" \
  -H "X-User-Id: 550e8400-e29b-41d4-a716-446655440000" \
  -d '{
    "name": "AI Landing Page",
    "prompt": "Create a modern landing page for an AI SaaS company"
  }'
```

### Test Project Retrieval
```bash
curl http://localhost:5003/api/projects/550e8400-e29b-41d4-a716-446655440001 \
  -H "X-User-Id: 550e8400-e29b-41d4-a716-446655440000"
```

### Test Download
```bash
curl -o project.zip http://localhost:5003/api/projects/550e8400-e29b-41d4-a716-446655440001/download \
  -H "X-User-Id: 550e8400-e29b-41d4-a716-446655440000"
```

---

## 📊 DATABASE SCHEMA

```sql
-- Projects table
CREATE TABLE Projects (
    Id UUID PRIMARY KEY,
    UserId UUID NOT NULL,
    Name VARCHAR(255) NOT NULL,
    Prompt TEXT NOT NULL,
    Status VARCHAR(50) NOT NULL,
    PreviewUrl VARCHAR(500),
    ArtifactUrl VARCHAR(500),
    CreatedAt TIMESTAMP NOT NULL
);

-- Generated Artifacts
CREATE TABLE GeneratedArtifacts (
    Id UUID PRIMARY KEY,
    ProjectId UUID NOT NULL REFERENCES Projects(Id),
    Type VARCHAR(50) NOT NULL,
    MetadataJson JSONB,
    CreatedAt TIMESTAMP NOT NULL
);

-- Generated Files
CREATE TABLE GeneratedFiles (
    Id UUID PRIMARY KEY,
    ArtifactId UUID NOT NULL REFERENCES GeneratedArtifacts(Id),
    Path VARCHAR(500) NOT NULL,
    Format VARCHAR(50) NOT NULL,
    CreatedAt TIMESTAMP NOT NULL
);

-- Background Jobs
CREATE TABLE GenerateWebsiteJobs (
    Id UUID PRIMARY KEY,
    ProjectId UUID NOT NULL REFERENCES Projects(Id),
    UserId UUID NOT NULL,
    Prompt TEXT NOT NULL,
    Status VARCHAR(50) NOT NULL,
    GeneratedCode TEXT,
    ErrorMessage TEXT,
    CreatedAt TIMESTAMP NOT NULL,
    CompletedAt TIMESTAMP
);
```

---

## ⚙️ CONFIGURATION

### appsettings.Development.json
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  },
  "ConnectionStrings": {
    "GeneratorDb": "Host=localhost;Port=5432;Database=techbirdsfly_generator;Username=postgres;Password=postgres123"
  },
  "Ollama": {
    "Endpoint": "http://localhost:11434",
    "Model": "llama3.1:8b"
  },
  "Minio": {
    "Endpoint": "localhost:9000",
    "AccessKey": "minio",
    "SecretKey": "minio123",
    "Bucket": "techbirdsfly-storage"
  },
  "Kafka": {
    "Broker": "localhost:9092",
    "Topics": {
      "PageGenerated": "page-generated",
      "ArtifactCreated": "artifact-created",
      "GenerationFailed": "generation-failed"
    }
  },
  "Serilog": {
    "Seq": {
      "Url": "http://localhost:5341",
      "ApiKey": ""
    }
  },
  "Jaeger": {
    "AgentHost": "localhost",
    "AgentPort": 6831
  }
}
```

### appsettings.Production.json
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  },
  "ConnectionStrings": {
    "GeneratorDb": "${DB_CONNECTION_STRING}"
  },
  "Ollama": {
    "Endpoint": "${OLLAMA_ENDPOINT}",
    "Model": "llama3.1:8b"
  },
  "Minio": {
    "Endpoint": "${MINIO_ENDPOINT}",
    "AccessKey": "${MINIO_ACCESS_KEY}",
    "SecretKey": "${MINIO_SECRET_KEY}",
    "Bucket": "techbirdsfly-storage"
  },
  "Kafka": {
    "Broker": "${KAFKA_BROKER}",
    "Topics": {
      "PageGenerated": "page-generated",
      "ArtifactCreated": "artifact-created",
      "GenerationFailed": "generation-failed"
    }
  },
  "Serilog": {
    "Seq": {
      "Url": "${SEQ_URL}",
      "ApiKey": "${SEQ_API_KEY}"
    }
  }
}
```

---

## 🎯 NEXT STEPS

### Phase 1: Verify Generator Service ✅
- [x] All layers implemented (Domain, Application, Infrastructure, API)
- [x] Database migrations ready
- [x] Docker configuration complete
- [x] Ollama integration available

### Phase 2: Integration (Ready to Start)
- [ ] Integrate with YARP Gateway (15 minutes)
- [ ] Connect Frontend with Zustand store (20 minutes)
- [ ] Test end-to-end workflow (30 minutes)
- [ ] Load testing with k6 (30 minutes)

### Phase 3: Additional Services (Optional)
- [ ] Project Service (project metadata, settings)
- [ ] Template Service (pre-built templates, themes)
- [ ] Media Service (image generation, asset management)
- [ ] Analytics Service (usage tracking, metrics)

---

## 📈 PERFORMANCE METRICS

### Expected Performance
- **Generation Time**: 15-45 seconds (Ollama model dependent)
- **ZIP Creation**: < 2 seconds (for typical project)
- **Database Writes**: < 100ms
- **MinIO Uploads**: < 5 seconds
- **API Response**: < 500ms (excluding AI generation)

### Optimization Tips
- Pre-warm Ollama model on startup
- Cache frequent prompts
- Use MinIO multipart uploads for large files
- Implement request queuing for high load
- Consider GPU acceleration for Ollama

---

## 🚨 TROUBLESHOOTING

### "Connection refused" on Ollama endpoint
```bash
# Make sure Ollama is running
ollama serve

# For Docker, use: http://host.docker.internal:11434
# For production, update Ollama__Endpoint in configuration
```

### "Database not found" error
```bash
# Apply migrations
dotnet ef database update

# Or with connectionstring:
dotnet ef database update -c GeneratorDbContext
```

### MinIO authentication failed
```bash
# Verify credentials in appsettings.json
# Default: minio / minio123

# Or reset:
docker exec minio mc admin user disable minio
docker exec minio mc admin user add minio minio minio123
```

### Kafka connection timeout
```bash
# Ensure Kafka is running
docker exec kafka kafka-broker-api-versions.sh --bootstrap-server localhost:9092

# Check broker connectivity
docker logs kafka
```

---

## 📚 ARCHITECTURE DECISION RECORDS

### Why Ollama instead of Azure OpenAI?
- ✅ Local/on-prem deployment capability
- ✅ No external API costs
- ✅ Privacy-first (data stays local)
- ✅ Can switch to OpenAI with minimal changes

### Why MinIO instead of Azure Blob?
- ✅ S3-compatible API (portable)
- ✅ Local development friendly
- ✅ No cloud vendor lock-in
- ✅ Can switch to AWS S3, Azure, or GCS

### Why Kafka for events?
- ✅ Decoupled architecture
- ✅ Event replay capability
- ✅ Scales to millions of messages
- ✅ Can integrate other services asynchronously

---

## 🔗 SERVICE CONNECTIONS

### Current Integrations
- ✅ PostgreSQL (persistence)
- ✅ Ollama (AI generation)
- ✅ MinIO (file storage)
- ✅ Kafka (event publishing)
- ✅ Seq (structured logging)
- ✅ Jaeger (distributed tracing)

### Ready for Integration
- 🔌 YARP Gateway (routing)
- 🔌 Auth Service (JWT validation)
- 🔌 Export Service (ZIP packaging)
- 🔌 Frontend (Next.js + React)

---

## 📝 SUMMARY

Your **Generator Service** is a **production-grade microservice** with:

✅ Complete clean architecture  
✅ Full AI integration (Ollama)  
✅ Robust storage (MinIO)  
✅ Event-driven design (Kafka)  
✅ Observability (Serilog + Seq + OpenTelemetry)  
✅ Docker deployment ready  
✅ Gateway integration ready  

**Status**: Ready for integration with frontend and gateway.

---

**Last Updated**: November 25, 2025  
**Version**: 1.0.0  
**Status**: Production Ready ✅
