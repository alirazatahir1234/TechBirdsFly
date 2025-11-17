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
┌──────────────────────────────────────────────────────────────┐
│                    React Frontend (Port 3000)                │
│              Tailwind CSS + shadcn/ui Components             │
└────────────────────────┬─────────────────────────────────────┘
                         │ HTTP/REST
                ┌────────▼────────────┐
                │  API Gateway (YARP) │
                │   Port 5500         │
                └────┬───────────────┬┤
         ┌──────────┬┤  Routes       ├┴──────────┐
         │          │                │           │
    ┌────▼──┐  ┌────▼─────┐  ┌───────▼──┐  ┌────▼───┐
    │ Auth  │  │  Billing  │  │   Image  │  │  Admin │
    │ 5001  │  │   5177    │  │   5007   │  │  5006  │
    └───────┘  └───────────┘  └──────────┘  └────────┘
         │           │            │            │
    ┌────▼──────┐  ┌─────▼────┐  ┌───▼────┐      │
    │PostgreSQL │  │PostgreSQL│  │MongoDB  │      │
    └───────────┘  └──────────┘  └─────────┘      │
                                               │
            ┌──────────────────────────────────┤
            │                                  │
      ┌─────▼─────┐  ┌─────────────┐  ┌────────▼───┐
      │   User    │  │   EventBus  │  │   Cache    │
      │   5005    │  │   5030      │  │   8100     │
      └───────────┘  └─────────────┘  └────────────┘
            │              │                   │
      ┌─────▼──┐     ┌─────▼─────┐       ┌────▼───┐
      │PostgreSQL    │PostgreSQL │       │ Redis  │
      └──────────┘   └───────────┘       └────────┘

Infrastructure:
- PostgreSQL (5433): Auth, User, Billing, EventBus, Admin, Generator services
- MongoDB (27017): Image Service
- Redis (6379): Caching
- Kafka (9092): Event streaming
```

### Microservices Overview

| Service | Port | Purpose | Database | Status |
|---------|------|---------|----------|--------|
| **API Gateway** | 5500 | Route requests to services | - | ✅ Running |
| **Auth Service** | 5001 | User registration, login, JWT tokens | PostgreSQL | ✅ Running |
| **User Service** | 5005 | User profiles, settings management | PostgreSQL | ✅ Running |
| **Billing Service** | 5177 | Billing, subscriptions, payments | PostgreSQL | ⏳ Ready |
| **Image Service** | 5007 | Image processing, AI image generation | MongoDB | ⏳ Ready |
| **EventBus Service** | 5030 | Async events, event publishing | PostgreSQL | ⏳ Ready |
| **Admin Service** | 5006 | Admin dashboard, monitoring | PostgreSQL | ⏳ Ready |
| **Generator Service** | 5003 | Website generation, project management | PostgreSQL | ⏳ Ready |
| **Cache Service** | 8100 | Distributed caching layer | Redis | ⏳ Ready |
| **Frontend** | 3000 | React SPA - User interface | - | ✅ Running |

### Service Responsibilities

- **Auth Service**: JWT token generation, user authentication, password management
- **User Service**: User profiles, preferences, account settings
- **Billing Service**: Subscription plans, payment processing, invoice generation
- **Image Service**: Image upload, storage, AI-powered image generation via DALL·E
- **EventBus Service**: Async event handling, service-to-service communication
- **Admin Service**: System monitoring, user management, analytics dashboard
- **Generator Service**: Website project management, AI content generation, ZIP packaging
- **Cache Service**: Distributed caching with Redis backend, shared across all services
- **API Gateway**: Request routing, load balancing, health checks, request/response logging

## 📋 Project Structure

```
TechBirdsFly/
├─ .github/
│  └─ copilot-instructions.md        # Development checklist
├─ docs/
│  ├─ architecture.md                # Service design details
│  ├─ architecture_mermaid.md        # Diagrams & flows
│  └─ README.md
├─ infra/
│  ├─ docker-compose.yml             # Docker infrastructure
│  └─ k8s/                           # Kubernetes configs
├─ gateway/
│  └─ yarp-gateway/
│     └─ src/
│        ├─ Program.cs
│        ├─ appsettings.json         # Routes for all services
│        └─ Properties/
├─ services/                         # Microservices
│  ├─ auth-service/
│  │  ├─ AuthService/                # .NET 8 API
│  │  │  ├─ Controllers/
│  │  │  ├─ Services/
│  │  │  ├─ Data/                    # EF Core DbContext
│  │  │  ├─ Models/
│  │  │  └─ Migrations/
│  │  └─ Dockerfile
│  ├─ user-service/
│  │  ├─ UserService/                # .NET 8 API
│  │  └─ Dockerfile
│  ├─ billing-service/
│  │  ├─ BillingService/             # .NET 8 API
│  │  └─ Dockerfile
│  ├─ image-service/
│  │  ├─ ImageService/               # .NET 8 API + MongoDB
│  │  └─ Dockerfile
│  ├─ eventbus-service/
│  │  ├─ EventBusService/            # .NET 8 API + Kafka
│  │  └─ Dockerfile
│  ├─ admin-service/
│  │  ├─ AdminService/               # .NET 8 API
│  │  └─ Dockerfile
│  ├─ generator-service/
│  │  ├─ GeneratorService/           # .NET 8 API
│  │  └─ Dockerfile
│  ├─ cache-service/
│  │  ├─ CacheService/               # .NET 8 API + Redis
│  │  └─ Dockerfile
│  └─ README.md                      # Services overview
├─ web-frontend/
│  └─ techbirdsfly-frontend-nextjs/  # React 18 TypeScript
│     ├─ pages/
│     ├─ components/
│     ├─ lib/
│     ├─ auth.ts
│     └─ next.config.js
├─ TechBirdsFly.sln                  # Visual Studio solution
└─ README.md (this file)
```

## 🚀 Quick Start

### Prerequisites
- .NET 8 SDK
- Node.js 18+
- PostgreSQL 12+ (EnterpriseDB or Docker)
- MongoDB (Docker recommended)
- Optional: Docker & Docker Compose

### Option 1: Local Development (Recommended)

#### Step 1: Start Infrastructure (Docker)
```bash
docker compose -f infra/docker-compose.yml up -d
# Starts: PostgreSQL, MongoDB, Redis, Kafka, Zookeeper, Schema Registry
```

#### Step 2: Start Backend Services (4 terminals)

**Terminal 1 - Auth Service** (Port 5001)
```bash
cd services/auth-service/AuthService
dotnet run --urls http://localhost:5001
```

**Terminal 2 - User Service** (Port 5005)
```bash
cd services/user-service/UserService
dotnet run --urls http://localhost:5005
```

**Terminal 3 - API Gateway** (Port 5500)
```bash
cd gateway/yarp-gateway/src
dotnet run --urls http://localhost:5500
```

**Terminal 4 - Frontend** (Port 3000)
```bash
cd web-frontend/techbirdsfly-frontend
npm install  # First time only
npm start    # Opens http://localhost:3000
```

#### Step 3: Verify All Services Running
```bash
# Check Gateway health
curl http://localhost:5500/health

# Check Auth Service
curl http://localhost:5001/health

# Check User Service
curl http://localhost:5005/health

# Check Frontend
open http://localhost:3000
```

**All 4 services running** ✅ → Ready for end-to-end testing!

### Option 2: Full Stack with Docker Compose

```bash
# Start all infrastructure
docker compose -f infra/docker-compose.yml up -d

# Start backend services (as above, 4 terminals)
# Or use Docker Compose override file for full containerization
```

### Option 3: Quick Gateway Test

```bash
# Get Auth token
curl -X POST http://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email": "test@example.com", "password": "Password123!"}'

# Extract token from response, then test Gateway routing
curl http://localhost:5500/api/auth/me \
  -H "Authorization: Bearer <YOUR_TOKEN>"
```

### Connection Strings

Ensure these match your environment:

**PostgreSQL Databases (Local)**
```
Auth Service:        Host=localhost;Port=5432;Database=techbirdsfly_auth;Username=postgres;Password=postgres123
User Service:        Host=localhost;Port=5432;Database=techbirdsfly_user;Username=postgres;Password=postgres123
Billing Service:     Host=localhost;Port=5432;Database=techbirdsfly_billing;Username=postgres;Password=postgres123
EventBus Service:    Host=localhost;Port=5432;Database=techbirdsfly_eventbus;Username=postgres;Password=postgres123
Admin Service:       Host=localhost;Port=5432;Database=techbirdsfly_admin;Username=postgres;Password=postgres123
Generator Service:   Host=localhost;Port=5432;Database=techbirdsfly_generator;Username=postgres;Password=postgres123
```

**PostgreSQL Databases (Docker)**
```
Auth Service:        Host=localhost;Port=5433;Database=techbirdsfly_auth;Username=postgres;Password=postgres123
User Service:        Host=localhost;Port=5433;Database=techbirdsfly_user;Username=postgres;Password=postgres123
Billing Service:     Host=localhost;Port=5433;Database=techbirdsfly_billing;Username=postgres;Password=postgres123
EventBus Service:    Host=localhost;Port=5433;Database=techbirdsfly_eventbus;Username=postgres;Password=postgres123
Admin Service:       Host=localhost;Port=5433;Database=techbirdsfly_admin;Username=postgres;Password=postgres123
Generator Service:   Host=localhost;Port=5433;Database=techbirdsfly_generator;Username=postgres;Password=postgres123
```

**MongoDB**
```
mongodb://localhost:27017
```

**Redis**
```
localhost:6379
```

## 📚 API Documentation

### API Gateway Routing

All services are accessed through the API Gateway at `http://localhost:5500`:

```
/api/auth/**      → Auth Service (5001)
/api/users/**     → User Service (5005)
/api/billing/**   → Billing Service (5177)
/api/images/**    → Image Service (5007)
/api/events/**    → EventBus Service (5030)
/api/admin/**     → Admin Service (5006)
```

### Auth Service (`/api/auth`)

**Direct**: `http://localhost:5001`  
**Via Gateway**: `http://localhost:5500/api/auth`

| Endpoint | Method | Purpose | Auth |
|----------|--------|---------|------|
| `/register` | POST | Register new user | ❌ |
| `/login` | POST | Login & get JWT | ❌ |
| `/refresh` | POST | Refresh access token | ✅ JWT |
| `/verify-email` | GET | Verify email link | ❌ |
| `/me` | GET | Get current user | ✅ JWT |
| `/health` | GET | Health check | ❌ |

**Example: Register**
```bash
curl -X POST http://localhost:5500/api/auth/register \
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
  "fullName": "Ali Raza",
  "createdAt": "2025-01-01T10:00:00Z"
}
```

### User Service (`/api/users`)

**Direct**: `http://localhost:5005`  
**Via Gateway**: `http://localhost:5500/api/users`

| Endpoint | Method | Purpose | Auth |
|----------|--------|---------|------|
| `/profile` | GET | Get user profile | ✅ JWT |
| `/profile` | PUT | Update user profile | ✅ JWT |
| `/settings` | GET | Get user settings | ✅ JWT |
| `/settings` | PUT | Update settings | ✅ JWT |
| `/health` | GET | Health check | ❌ |

### Billing Service (`/api/billing`)

**Direct**: `http://localhost:5177`  
**Via Gateway**: `http://localhost:5500/api/billing`

| Endpoint | Method | Purpose | Auth |
|----------|--------|---------|------|
| `/plans` | GET | List subscription plans | ❌ |
| `/subscriptions` | GET | Get user subscription | ✅ JWT |
| `/subscribe` | POST | Create subscription | ✅ JWT |
| `/invoices` | GET | List invoices | ✅ JWT |
| `/health` | GET | Health check | ❌ |

### Image Service (`/api/images`)

**Direct**: `http://localhost:5007`  
**Via Gateway**: `http://localhost:5500/api/images`

| Endpoint | Method | Purpose | Auth |
|----------|--------|---------|------|
| `/` | POST | Upload image | ✅ JWT |
| `/{id}` | GET | Get image | ✅ JWT |
| `/{id}` | DELETE | Delete image | ✅ JWT |
| `/generate` | POST | Generate image via DALL·E | ✅ JWT |
| `/health` | GET | Health check | ❌ |

### EventBus Service (`/api/events`)

**Direct**: `http://localhost:5030`  
**Via Gateway**: `http://localhost:5500/api/events`

| Endpoint | Method | Purpose | Auth |
|----------|--------|---------|------|
| `/` | GET | Get recent events | ✅ JWT |
| `/subscribe` | POST | Subscribe to event type | ✅ JWT |
| `/health` | GET | Health check | ❌ |

### Admin Service (`/api/admin`)

**Direct**: `http://localhost:5006`  
**Via Gateway**: `http://localhost:5500/api/admin`

| Endpoint | Method | Purpose | Auth |
|----------|--------|---------|------|
| `/users` | GET | List all users | ✅ Admin |
| `/users/{id}` | DELETE | Delete user | ✅ Admin |
| `/stats` | GET | System statistics | ✅ Admin |
| `/logs` | GET | System logs | ✅ Admin |
| `/health` | GET | Health check | ❌ |

## 🔐 Authentication

- **JWT Tokens**: 60-minute access + refresh tokens
- **Claims**: `sub` (userId), `email`, `name`
- **Header**: `Authorization: Bearer <token>`
- **Validation**: Gateway validates all protected routes

## 📊 Tech Stack

### Backend Services
| Technology | Version | Purpose |
|------------|---------|---------|
| ASP.NET Core | 8.0 | Web API framework |
| Entity Framework Core | 8.0 | ORM for database access |
| AutoMapper | 13.0 | Object mapping |
| MediatR | 12.0 | CQRS & request handling |
| Serilog | 3.0 | Structured logging |

### Databases
| Technology | Version | Purpose |
|------------|---------|---------|
| PostgreSQL | 12+ | All microservices (Auth, User, Billing, EventBus, Admin, Generator) |
| MongoDB | 5+ | Image Service storage |
| Redis | 6+ | Distributed caching |

### Message Queue & Events
| Technology | Version | Purpose |
|------------|---------|---------|
| Kafka | 3.0 | Event streaming |
| Zookeeper | 3.0 | Kafka coordination |

### Frontend
| Technology | Version | Purpose |
|------------|---------|---------|
| React | 18.0 | UI library |
| TypeScript | 5.0 | Type safety |
| Next.js | 14.0 | Framework & routing |
| Tailwind CSS | 3.4 | Styling |
| shadcn/ui | Latest | Component library |
| TanStack Query | 5.0 | Data fetching |

### API Gateway & Reverse Proxy
| Technology | Version | Purpose |
|------------|---------|---------|
| YARP | 2.0 | API Gateway (Yet Another Reverse Proxy) |

### Observability
| Technology | Version | Purpose |
|------------|---------|---------|
| Seq | Latest | Centralized logging |
| Jaeger | Latest | Distributed tracing |
| Prometheus | Latest | Metrics (planned) |
| Grafana | Latest | Visualization (planned) |

### Deployment & DevOps
| Technology | Version | Purpose |
|------------|---------|---------|
| Docker | Latest | Containerization |
| Kubernetes | 1.24+ | Orchestration (planned) |
| Azure Container Registry | - | Image registry |
| Azure App Service | - | Hosting (planned) |

## 🎨 Development Workflow

### Common Tasks

#### Adding a New API Endpoint

1. Create controller action in service (e.g., `UserController.cs`)
```csharp
[HttpGet("{id}")]
public async Task<IActionResult> GetUser(Guid id)
{
    var user = await _userService.GetUserAsync(id);
    return Ok(user);
}
```

2. Add service method in `Services/` folder
```csharp
public async Task<UserDto> GetUserAsync(Guid id)
{
    var user = await _context.Users.FindAsync(id);
    return _mapper.Map<UserDto>(user);
}
```

3. Update Gateway routes (if new service route needed)
   - Edit `/gateway/yarp-gateway/src/appsettings.json`
   - Add route + cluster

4. Test via Gateway
```bash
curl http://localhost:5500/api/users/{id} \
  -H "Authorization: Bearer <TOKEN>"
```

#### Running Database Migrations

```bash
# Auth Service
cd services/auth-service/AuthService
dotnet ef database update

# EventBus Service (uses PostgreSQL)
cd services/eventbus-service/EventBusService
dotnet ef database update

# Other services
dotnet ef database update
```

#### Adding a New Database Model

1. Create model in `Models/`
```csharp
public class UserProfile
{
    public Guid Id { get; set; }
    public string Bio { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

2. Add to DbContext
```csharp
public DbSet<UserProfile> UserProfiles { get; set; }
```

3. Create migration
```bash
dotnet ef migrations add AddUserProfile
dotnet ef database update
```

#### Testing an Endpoint

```bash
# 1. Get token from Auth Service
TOKEN=$(curl -X POST http://localhost:5500/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"Password123!"}' \
  -s | jq -r '.token')

# 2. Use token to call protected endpoint
curl http://localhost:5500/api/users/profile \
  -H "Authorization: Bearer $TOKEN"

# 3. Check Gateway routing
curl http://localhost:5500/health

# 4. Direct service test (bypass Gateway)
curl http://localhost:5005/api/users/profile \
  -H "Authorization: Bearer $TOKEN"
```

#### Running Tests

```bash
# Run all tests in a service
cd services/auth-service/AuthService
dotnet test

# Run specific test class
dotnet test --filter "ClassName=AuthControllerTests"

# Run with coverage
dotnet test /p:CollectCoverage=true
```

## 🔄 Service Communication

### Synchronous (REST via Gateway)
```
Client → Gateway → Target Service
```

### Asynchronous (Kafka Events)
```
Service A (publishes) → Kafka Topic → EventBus Service → Service B (subscribes)
```

### Caching Strategy
```
Request → Cache Service (Redis) → Return cached data
         → Cache miss → Database query → Cache result
```

## 📈 Roadmap

### Phase 1 ✅ (Completed)
- [x] 9 microservices with Clean Architecture
- [x] PostgreSQL integration (Auth, EventBus)
- [x] MongoDB integration (Image Service)
- [x] API Gateway (YARP) with routing
- [x] Frontend (Next.js with TypeScript)
- [x] Distributed caching (CacheClient library)
- [x] Docker infrastructure setup

### Phase 2 🔄 (Current)
- [ ] Cross-service communication testing
- [ ] Event publishing & subscription
- [ ] Background job processing
- [ ] Health checks & monitoring
- [ ] Rate limiting & throttling
- [ ] Request/response logging

### Phase 3 (Planned)
- [ ] Azure OpenAI integration (website generation)
- [ ] DALL·E image generation
- [ ] Stripe billing integration
- [ ] Multi-page site generation
- [ ] Custom themes & templates
- [ ] GitHub deployment integration
- [ ] SSO (Google, GitHub, Microsoft)
- [ ] Admin dashboard enhancements
- [ ] Kubernetes deployment

## 🛠️ Development Commands

### Build & Compile

```bash
# Build entire solution
dotnet build TechBirdsFly.sln

# Build individual service
cd services/auth-service/AuthService
dotnet build

# Clean build
dotnet clean && dotnet build
```

### Database Management

```bash
# Apply migrations
dotnet ef database update

# Create new migration
dotnet ef migrations add MigrationName

# Remove last migration
dotnet ef migrations remove

# Reset database (dev only!)
dotnet ef database drop --force
dotnet ef database update
```

### Running Services

```bash
# Run Auth Service with specific port
cd services/auth-service/AuthService
dotnet run --urls http://localhost:5001

# Run with watch mode (auto-reload on changes)
dotnet watch run --urls http://localhost:5001

# Run in release mode
dotnet run -c Release --urls http://localhost:5001
```

### Docker Management

```bash
# Start all infrastructure
docker compose -f infra/docker-compose.yml up -d

# View running containers
docker compose -f infra/docker-compose.yml ps

# View logs for specific service
docker compose -f infra/docker-compose.yml logs -f postgres

# Stop all containers
docker compose -f infra/docker-compose.yml down

# Remove all containers and volumes
docker compose -f infra/docker-compose.yml down -v
```

### Health & Diagnostics

```bash
# Check Gateway health
curl http://localhost:5500/health

# Check Auth Service
curl http://localhost:5001/health

# Check User Service
curl http://localhost:5005/health

# List running processes on port
lsof -i :5001  # Port 5001
lsof -i :5500  # Port 5500

# Kill process on port
pkill -f "dotnet run"
```

### Git & Repository

```bash
# Clean build artifacts
git rm -r --cached bin obj *.dll *.pdb

# View staged changes
git diff --cached

# Commit with conventional commits
git commit -m "feat(auth): add password reset endpoint"
git commit -m "fix(gateway): correct user service port mapping"
```

## 📝 Logging & Monitoring

- **Logs**: Console output + file rotation (in production)
- **Traces**: OpenTelemetry integration (planned)
- **Metrics**: Prometheus + Grafana (planned)
- **Health**: `/health` endpoint on each service (planned)

## 🚢 Deployment

### Local Docker Deployment

```bash
# Build Docker images for all services
docker build -t techbirdsfly/auth:latest ./services/auth-service
docker build -t techbirdsfly/user:latest ./services/user-service
docker build -t techbirdsfly/billing:latest ./services/billing-service
docker build -t techbirdsfly/image:latest ./services/image-service
docker build -t techbirdsfly/eventbus:latest ./services/eventbus-service
docker build -t techbirdsfly/gateway:latest ./gateway/yarp-gateway

# Run infrastructure
docker compose -f infra/docker-compose.yml up -d

# Run containers with proper networking
docker run --network techbirdsfly_default \
  -e "ConnectionStrings__Default=Host=postgres;Port=5432;Database=techbirdsfly_auth" \
  -p 5001:5001 \
  techbirdsfly/auth:latest
```

### Azure Deployment

#### 1. Push to Azure Container Registry

```bash
# Login to ACR
az acr login --name techbirdsflyreg

# Build and push image
az acr build --registry techbirdsflyreg \
  -f services/auth-service/Dockerfile \
  -t techbirdsfly/auth-service:v1.0 \
  ./services/auth-service

# Tag and push
docker tag techbirdsfly/auth:latest techbirdsflyreg.azurecr.io/auth:latest
docker push techbirdsflyreg.azurecr.io/auth:latest
```

#### 2. Deploy to App Service

```bash
# Create App Service Plan
az appservice plan create \
  --resource-group techbirdsfly \
  --name techbirdsfly-plan \
  --sku B2 \
  --is-linux

# Create Web App
az webapp create \
  --resource-group techbirdsfly \
  --plan techbirdsfly-plan \
  --name techbirdsfly-auth \
  --deployment-container-image-name techbirdsflyreg.azurecr.io/auth:latest

# Configure App Settings
az webapp config appsettings set \
  --resource-group techbirdsfly \
  --name techbirdsfly-auth \
  --settings @appsettings.azure.json
```

#### 3. Deploy to Kubernetes (AKS)

```bash
# Create resource group
az group create --name techbirdsfly --location eastus

# Create AKS cluster
az aks create \
  --resource-group techbirdsfly \
  --name techbirdsfly-aks \
  --node-count 3 \
  --enable-managed-identity

# Get credentials
az aks get-credentials \
  --resource-group techbirdsfly \
  --name techbirdsfly-aks

# Deploy services
kubectl apply -f infra/k8s/namespace.yaml
kubectl apply -f infra/k8s/services/auth-service.yaml
kubectl apply -f infra/k8s/services/user-service.yaml
# ... deploy other services

# Port forward Gateway for testing
kubectl port-forward svc/gateway 5500:5500
```

### Environment Configuration

**appsettings.Production.json**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=tcp:techbirdsfly.database.windows.net;Database=techbirdsfly_prod;User ID=dbadmin;Password=SecurePassword123!"
  },
  "AzureOpenAi": {
    "Endpoint": "https://techbirdsfly.openai.azure.com/",
    "ApiKey": "your-api-key",
    "DeploymentName": "gpt-4o-mini"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

### Database Backup & Migration

```bash
# Backup PostgreSQL
pg_dump -h localhost -U postgres -d techbirdsfly_auth > backup.sql

# Restore PostgreSQL
psql -h localhost -U postgres -d techbirdsfly_auth < backup.sql

# Backup MongoDB
mongodump --uri="mongodb://localhost:27017" --out=backup

# Restore MongoDB
mongorestore --uri="mongodb://localhost:27017" backup
```

## ❓ FAQ

**Q: Why use microservices?**  
A: Each service scales independently, teams can own specific services, easier to replace/upgrade individual components, and aligns with domain-driven design.

**Q: What's the difference between running directly vs Docker?**  
A: Direct: Faster development, use local PostgreSQL. Docker: Full infrastructure isolation, matches production environment, consistent across team.

**Q: Can I run just a few services?**  
A: Yes! Each service is independent. You only need Auth + Gateway + Frontend for basic testing. Add others as needed.

**Q: How do I add a new microservice?**  
A: 
1. Create new folder in `/services`
2. Create .NET 8 API project
3. Add to `docker-compose.yml`
4. Add routes to Gateway `appsettings.json`
5. Register health check endpoint

**Q: How does the Gateway route requests?**  
A: YARP uses rules in `appsettings.json` to match URL paths to backend services. E.g., `/api/auth/**` → Auth Service (5001).

**Q: What if a service fails?**  
A: Gateway has health checks every 30 seconds. If a service is down, Gateway removes it from the pool and returns 503.

**Q: How do I share data between services?**  
A: Use Kafka events (async) or direct REST calls via Gateway (sync). For frequent access, cache results in CacheService.

**Q: Can I use SQL Server instead of PostgreSQL?**  
A: Yes! Change connection strings in `appsettings.json` and adjust EF Core provider. But PostgreSQL is recommended for this project as all services are configured to use it.

**Q: How do I debug issues?**  
A: Check logs with `docker logs <container>`, test endpoints directly with curl, use Gateway health check `/health`, and verify service ports with `lsof -i :5001`.

**Q: What happens if I restart a service?**  
A: State is preserved in databases (PostgreSQL for transactional data, MongoDB for images, Redis for cache). Sessions/tokens remain valid until expiration.

**Q: How is authentication handled across services?**  
A: JWT tokens issued by Auth Service, validated by Gateway. Each service trusts the Gateway's validation.

**Q: Can I deploy to production without Kubernetes?**  
A: Yes! Use Azure App Service or manual Docker deployments. Kubernetes is optional for high-scale deployments.

**Q: Where is configuration stored?**  
A: `appsettings.json` for development, environment variables for production (set via deployment scripts).

**Q: How do I monitor service performance?**  
A: Jaeger for tracing, Seq for logs, Prometheus for metrics, Grafana for dashboards (all optional, can be added later).

## 📞 Support & Contributing

### Documentation
- � **Main Architecture**: See `/docs/architecture.md`
- 🔗 **Service Diagrams**: See `/docs/architecture_mermaid.md`
- � **Gateway Setup**: See `/gateway/QUICK_START.md`
- 🎨 **Frontend Setup**: See `/web-frontend/QUICK_START.md`

### Reporting Issues
1. Check existing documentation
2. Review logs: `docker logs <service-name>`
3. Test directly via curl
4. Report with: service name, error message, steps to reproduce

### Contributing
1. Fork the repository
2. Create feature branch: `git checkout -b feature/amazing-feature`
3. Commit with conventional commits: `git commit -m "feat(service): description"`
4. Push to branch: `git push origin feature/amazing-feature`
5. Open Pull Request

### Code Standards
- Follow C# style guide (use `dotnet format`)
- Add unit tests for new services
- Update README when adding features
- Ensure all services have health checks

---

**Next Steps:**
1. ✅ Read this README completely
2. ✅ Run `docker compose -f infra/docker-compose.yml up -d`
3. ✅ Start 4 services (Auth, User, Gateway, Frontend)
4. ⏭️ Test authentication flow via Gateway
5. ⏭️ Add more services (Billing, Image, EventBus, Admin)
6. ⏭️ Deploy to Azure

**Questions?** Check `/docs/architecture.md` or review service-specific README files in `/services/*/README.md`

---

**Built with ❤️ by Ali Raza | TechBirdsFly.AI © 2025**
