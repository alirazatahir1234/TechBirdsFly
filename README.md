# TechBirdsFly.AI — AI-Powered Website Generator

A modern, full-stack application that uses AI to generate professional, ready-to-deploy websites from simple text prompts.

**Status**: MVP Phase 1 ✅ | **Architecture**: Microservices (.NET 8 + React) | **Deployment**: Azure-ready

## 🎯 What It Does

1. User enters a prompt: *"Create a modern portfolio website for a photographer"*
2. Backend calls Azure OpenAI (GPT-4o-mini) for content & layout ideas
3. Generates a complete React project with Tailwind CSS styling
4. User previews the site live in the browser
5. Downloads as a ready-to-deploy ZIP file

## 🏗️ Architecture

```
┌─────────────────┐
│  React Frontend │  (Tailwind + shadcn/ui)
│  Port 3000      │
└────────┬────────┘
         │ HTTP
┌────────▼────────────────┐
│    API Gateway (YARP)   │
└────────┬─────────────────┘
         │
    ┌────┴────┬─────────────┐
    │          │             │
┌───▼──┐  ┌────▼────┐  ┌────▼─────┐
│ Auth │  │Generator │  │   Image   │
│ 5001 │  │ 5003     │  │   (soon)  │
└──────┘  └──────────┘  └───────────┘
   │ SQLite  │ SQLite       (future)
   └─────────┴────────────────
        │ (Async Jobs)
        │ RabbitMQ/Event Bus
        └─ Background Workers
```

### Services

| Service | Port | Purpose | Stack |
|---------|------|---------|-------|
| **Auth** | 5001 | User registration, login, JWT tokens | .NET 8, SQLite, EF Core |
| **Generator** | 5003 | Website generation, project management | .NET 8, SQLite, EF Core |
| **Image** | (soon) | AI image generation via DALL·E | .NET 8 |
| **Frontend** | 3000 | React SPA | React 18, TypeScript, Tailwind |

## 📋 Project Structure

```
TechBirdsFly/
├─ .github/
│  └─ copilot-instructions.md        # Development checklist
├─ docs/
│  ├─ architecture.md                # Service responsibilities & data
│  ├─ architecture_mermaid.md        # System & sequence diagrams
│  └─ README.md
├─ infra/
│  └─ docker-compose.yml             # Local dev infrastructure
├─ services/
│  ├─ auth-service/
│  │  ├─ AuthService/                # .NET 8 API
│  │  │  ├─ Controllers/
│  │  │  ├─ Services/
│  │  │  ├─ Data/                    # EF Core DbContext
│  │  │  ├─ Models/
│  │  │  └─ Migrations/
│  │  ├─ Dockerfile
│  │  └─ README.md
│  ├─ generator-service/
│  │  ├─ GeneratorService/           # .NET 8 API
│  │  ├─ Dockerfile
│  │  └─ README.md
│  └─ image-service/                 # (planned)
├─ web-frontend/
│  └─ techbirdsfly-frontend/         # React 18 TypeScript
├─ backend/
│  └─ TechBirdsFly.Api/              # (legacy, can archive)
└─ README.md (this file)
```

## 🚀 Quick Start

### Prerequisites
- .NET 8 SDK
- Node.js 18+
- Optional: Docker & Docker Compose

### Local Development (3 steps)

1. **Start Auth Service**
```bash
cd services/auth-service/AuthService
dotnet run --urls http://localhost:5001
```

2. **Start Generator Service** (new terminal)
```bash
cd services/generator-service/GeneratorService
dotnet run --urls http://localhost:5003
```

3. **Start Frontend** (new terminal)
```bash
cd web-frontend/techbirdsfly-frontend
npm start  # Opens http://localhost:3000
```

**All 3 services running** ✅ → Ready for end-to-end testing!

### With Docker Compose (optional)

```bash
docker compose -f infra/docker-compose.yml up -d
```

This starts:
- auth-db (SQL Server)
- redis (caching)
- rabbitmq (message queue)
- All services

## 📚 API Documentation

### Auth Service (`/api/auth`)

| Endpoint | Method | Purpose |
|----------|--------|---------|
| `/register` | POST | Register new user |
| `/login` | POST | Login & get JWT |
| `/refresh` | POST | Refresh access token |
| `/verify-email` | GET | Verify email link |

**Example: Register**
```bash
curl -X POST http://localhost:5001/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "fullName": "Ali Raza",
    "email": "ali@example.com",
    "password": "SecurePass123!"
  }'
```

**Response:**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "email": "ali@example.com",
  "fullName": "Ali Raza"
}
```

### Generator Service (`/api/projects`)

| Endpoint | Method | Purpose |
|----------|--------|---------|
| `/projects` | POST | Create project & submit for generation |
| `/projects/{id}` | GET | Get project status |
| `/projects/{id}/download` | GET | Get download link |

**Example: Create Project**
```bash
curl -X POST http://localhost:5003/api/projects \
  -H "Content-Type: application/json" \
  -H "X-User-Id: 12345678-1234-5678-1234-567812345678" \
  -d '{
    "name": "My Portfolio",
    "prompt": "Create a modern portfolio website for a photographer with dark theme"
  }'
```

**Response:**
```json
{
  "projectId": "660e8400-e29b-41d4-a716-446655440000",
  "jobId": "760e8400-e29b-41d4-a716-446655440001",
  "status": "pending",
  "message": "Project created and queued for generation"
}
```

## 🔐 Authentication

- **JWT Tokens**: 60-minute access + refresh tokens
- **Claims**: `sub` (userId), `email`, `name`
- **Header**: `Authorization: Bearer <token>`
- **Validation**: Gateway validates all protected routes

## 📊 Key Technologies

| Layer | Tech | Version |
|-------|------|---------|
| Frontend | React | 18.0 |
| Frontend Styling | Tailwind CSS | 3.4 |
| Frontend Components | shadcn/ui | Latest |
| Backend API | ASP.NET Core | 8.0 |
| Database | SQLite (dev) / SQL Server (prod) | Latest |
| Message Bus | RabbitMQ | 3 (optional MVP) |
| Cache | Redis | 6 |
| AI | Azure OpenAI | GPT-4o-mini |
| Deployment | Docker / Azure | - |

## 🎨 Development Workflow

### Creating New Features

1. **Add API Endpoint**: Create controller action in desired service
2. **Add DB Model**: Create EF Core model in `Models/`
3. **Add Service Logic**: Implement in `Services/`
4. **Create Migration**: `dotnet ef migrations add MigrationName`
5. **Test Locally**: Use curl or Postman
6. **Update Frontend**: Add React component + API call

### Testing an Endpoint

```bash
# Test Auth Login
curl -X POST http://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "ali@example.com",
    "password": "SecurePass123!"
  }'

# Extract token from response, use in next request
BEARER_TOKEN="eyJhbGc..."

# Test protected endpoint
curl http://localhost:5003/api/projects/550e8400-e29b-41d4-a716-446655440000 \
  -H "Authorization: Bearer $BEARER_TOKEN" \
  -H "X-User-Id: 550e8400-e29b-41d4-a716-446655440000"
```

## 🔄 Async Processing Flow

```
User submits prompt
    ↓
POST /api/projects
    ↓
Create Project + GenerateWebsiteJob
    ↓
Publish to message bus
    ↓
Background worker picks up
    ↓
Call Azure OpenAI
    ↓
Package ZIP file
    ↓
Store in Blob Storage
    ↓
Update project status
    ↓
Frontend polls /api/projects/{id}
    ↓
Shows download link
```

## 📈 Next Phases

### Phase 2 (Planned)
- ✅ API Gateway (YARP) routing
- ✅ Frontend integration (React UI)
- 🔄 Background worker implementation
- 🔄 Real Azure OpenAI integration (replace mock)
- 🔄 Image generation (DALL·E API)
- 🔄 Blob Storage integration

### Phase 3 (Growth)
- Multi-page site generation
- Custom themes & templates
- Stripe billing integration
- User project dashboard
- GitHub deployment integration
- SSO (Google, GitHub, Microsoft)

## 🛠️ Development Commands

```bash
# Build all services
dotnet build services/auth-service/AuthService
dotnet build services/generator-service/GeneratorService

# Run migrations
cd services/auth-service/AuthService && dotnet ef database update
cd services/generator-service/GeneratorService && dotnet ef database update

# Create new migration
cd services/auth-service/AuthService && dotnet ef migrations add AddNewField

# View logs
dotnet run | grep -i error

# Clean databases
rm services/*/GeneratorService/generator.db
rm services/*/AuthService/auth.db
```

## 📝 Logging & Monitoring

- **Logs**: Console output + file rotation (in production)
- **Traces**: OpenTelemetry integration (planned)
- **Metrics**: Prometheus + Grafana (planned)
- **Health**: `/health` endpoint on each service (planned)

## 🚢 Deployment

### Azure Container Registry & AKS

```bash
# Build & push Docker image
az acr build --registry <registry-name> \
  -f services/auth-service/Dockerfile \
  -t techbirdsfly/auth-service:v1.0 .

# Deploy to AKS (using Helm charts)
helm install auth-service ./charts/auth-service \
  --set image.tag=v1.0
```

## ❓ FAQ

**Q: Why microservices?**  
A: Easier to scale individually, independent deployments, team autonomy as the project grows.

**Q: Can I run locally without Docker?**  
A: Yes! Just run `dotnet run` in each service folder. Docker is for production.

**Q: How do I change the AI model?**  
A: Update `GeneratorService.cs` to call different Azure OpenAI models (e.g., `gpt-4-turbo`).

**Q: Where are generated files stored?**  
A: Local: `/services/generator-service/generator.db` (SQLite) | Production: Azure Blob Storage

## 📞 Support & Contribution

- 📧 Email: ali@techbirdsfly.ai
- 🐙 GitHub: [TechBirdsFly](https://github.com/techbirdsfly)
- 💬 Discord: [Join Community](#)

---

**Built with ❤️ by Ali Raza | TechBirdsFly.AI © 2025**

**Next Step**: Read `/docs/architecture.md` for detailed service responsibilities or jump to `/services/auth-service/README.md` for Auth Service setup.
