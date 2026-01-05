# Services Overview

Complete microservice architecture with 6 independent services running on separate ports.

## Service Registry

| Service | Port | Status | Purpose |
|---------|------|--------|---------|
| **Auth** | 5001 | ✅ Production | User authentication, JWT tokens, SQLite |
| **User** | 5002 | 🟡 Phase 2 | User profiles, preferences, quotas |
| **Generator** | 5289 | ✅ Production | Website generation (Ollama Llama 3.1), project mgmt, ZIP export |
| **Image** | 5004 | 🟡 Phase 2 | Image generation (DALL·E), CDN |
| **Billing** | 5005 | 🟡 Phase 2 | Usage tracking, Stripe, invoices |
| **Admin** | 5006 | 🟡 Phase 2 | Admin dashboard, templates, audit |

## Architecture Pattern

```
┌─────────────────────────────────────────┐
│           YARP API Gateway              │
│  (Central routing, auth validation)     │
└──────────┬──────────────────────────────┘
           │
    ┌──────┼──────┬──────┬──────┬──────┐
    │      │      │      │      │      │
   [Auth] [User] [Gen]  [Img]  [Bill] [Admin]
    │      │      │      │      │      │
    └──────┼──────┼──────┼──────┼──────┘
           │
    ┌──────┴──────────────────────┐
    │   Message Bus (RabbitMQ)    │
    │   • Job events              │
    │   • Usage tracking          │
    │   • Invoice events          │
    └─────────────────────────────┘
```

## Database Per Service

Each service owns its database following microservice principles:

```
Auth Service     → auth.db       (Users, Sessions)
User Service     → user.db       (Profiles, Preferences, Quotas)
Generator        → generator.db  (Projects, Jobs, Generated Code)
Image Service    → image.db      (Images, Metadata, Cache)
Billing Service  → billing.db    (Invoices, Usage, Subscriptions)
Admin Service    → admin.db      (Templates, Audit Logs, Settings)
```

## Communication Patterns

### Synchronous (HTTP/gRPC)
- Frontend → API Gateway → Services
- API Gateway → Service-to-service calls
- Service health checks

### Asynchronous (Message Bus)
- Generator publishes `WebsiteGenerated` event
- Billing subscribes to track usage
- Image service publishes `ImageGenerated` event
- User service updates quotas

## Directory Structure

```
services/
├─ auth-service/
│  ├─ src/
│  │  ├─ Program.cs
│  │  ├─ AuthService.csproj
│  │  ├─ Models/
│  │  ├─ Services/
│  │  └─ Controllers/
│  ├─ Dockerfile
│  └─ README.md
├─ user-service/
│  ├─ src/
│  │  └─ .gitkeep (ready for Phase 2)
│  ├─ Dockerfile
│  └─ README.md
├─ generator-service/
│  ├─ src/
│  │  ├─ Program.cs
│  │  ├─ GeneratorService.csproj
│  │  ├─ Models/
│  │  ├─ Services/
│  │  └─ Controllers/
│  ├─ Dockerfile
│  └─ README.md
├─ image-service/
│  ├─ src/
│  │  └─ .gitkeep (ready for Phase 2)
│  ├─ Dockerfile
│  └─ README.md
├─ billing-service/
│  ├─ src/
│  │  └─ .gitkeep (ready for Phase 2)
│  ├─ Dockerfile
│  └─ README.md
└─ admin-service/
   ├─ src/
   │  └─ .gitkeep (ready for Phase 2)
   ├─ Dockerfile
   └─ README.md
```

## Local Development

### Start All Services (6 terminals)

```bash
# Terminal 1: Auth Service
cd services/auth-service/src && dotnet run --urls http://localhost:5001

# Terminal 2: User Service (Phase 2)
cd services/user-service/src && dotnet run --urls http://localhost:5002

# Terminal 3: Generator Service
cd services/generator-service/src && dotnet run --urls http://localhost:5003

# Terminal 4: Image Service (Phase 2)
cd services/image-service/src && dotnet run --urls http://localhost:5004

# Terminal 5: Billing Service (Phase 2)
cd services/billing-service/src && dotnet run --urls http://localhost:5005

# Terminal 6: Admin Service (Phase 2)
cd services/admin-service/src && dotnet run --urls http://localhost:5006
```

### Docker Compose (All-in-one)

```bash
docker-compose up -d
```

### Access Swagger Docs

- Auth: http://localhost:5001/swagger
- User: http://localhost:5002/swagger (Phase 2)
- Generator: http://localhost:5003/swagger
- Image: http://localhost:5004/swagger (Phase 2)
- Billing: http://localhost:5005/swagger (Phase 2)
- Admin: http://localhost:5006/swagger (Phase 2)

## Phase 2 Scaffolding Checklist

For each Phase 2 service, create:

```bash
cd services/[service-name]/src

# 1. Create .NET project
dotnet new webapi -name [ServiceName]Service

# 2. Add NuGet packages
dotnet add package Microsoft.EntityFrameworkCore.PostgreSQL
dotnet add package System.IdentityModel.Tokens.JsonWebTokenHandler

# 3. Create directory structure
mkdir Models Services Controllers Data Migrations

# 4. Create Program.cs (copy from auth-service/src/Program.cs as template)
# 5. Create DbContext and models
# 6. Create services and controllers
# 7. Add migrations: dotnet ef migrations add InitialCreate
# 8. Test: dotnet run
```

## Service Responsibilities

### Auth Service ✅
- User registration & login
- JWT token generation & validation
- Token refresh
- Password reset (future)
- Email verification (future)

### User Service (Phase 2)
- User profile management
- User preferences
- Quota tracking
- Usage limits
- Profile statistics

### Generator Service ✅
- Website generation request handling
- Project management (CRUD)
- Job orchestration
- Code generation coordination
- ZIP packaging

### Image Service (Phase 2)
- AI image generation (DALL·E)
- Image storage
- CDN URL generation
- Image caching
- Background image optimization

### Billing Service (Phase 2)
- Usage metering
- Billing calculations
- Invoice generation
- Stripe payment processing
- Subscription management
- Usage quotas enforcement

### Admin Service (Phase 2)
- User management
- Template management
- System analytics
- Audit logging
- Health monitoring
- System configuration

## API Gateway (YARP)

Routes all traffic through centralized gateway:

```
POST /api/auth/register      → Auth Service
POST /api/auth/login         → Auth Service
GET  /api/users/{id}         → User Service
POST /api/projects           → Generator Service
GET  /api/projects/{id}      → Generator Service
POST /api/images/generate    → Image Service
GET  /api/billing/user/{id}  → Billing Service
POST /api/admin/users        → Admin Service (admin-only)
```

## Deployment

### Docker Compose (Local Dev)
```bash
docker-compose up -d
```

### Kubernetes (Production)
```bash
kubectl apply -f infra/k8s/
```

### Azure Container Registry
```bash
az acr build -r [registry-name] -t techbirdsfly:[version] .
```

## Monitoring & Logging

Each service outputs to:
- **Console**: Development logs
- **Application Insights**: Production telemetry
- **Azure Monitor**: Metrics and traces
- **Audit Logs**: Admin Service

## Related Documentation

- [Full Architecture](/docs/architecture.md)
- [Quick Start Guide](/QUICK_START.md)
- [Completion Summary](/COMPLETION_SUMMARY.md)
- Individual service READMEs

## Current Ports

```
5001 - Auth Service            ✅ Active
5002 - User Service            🟡 Ready
5289 - Generator Service       ✅ Active (was 5003, changed for conflict resolution)
5004 - Image Service           🟡 Ready
5005 - Billing Service         🟡 Ready
5006 - Admin Service           🟡 Ready
3000 - Frontend (Next.js)      ✅ Running
8000 - API Gateway (YARP)      ✅ Running (was 5007, changed for standardization)
5432 - PostgreSQL              ✅ Running
11434 - Ollama (Llama 3.1)     ✅ Running
6379 - Redis                   ✅ Running
5341 - Seq Logging             ✅ Running
16686 - Jaeger Tracing         ✅ Running
```

---

**Status**: 🟢 **Ready for Phase 2 development**

Next: Choose a service to scaffold (recommendation: User Service first)
