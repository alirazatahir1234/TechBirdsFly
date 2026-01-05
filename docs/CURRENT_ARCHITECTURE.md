# TechBirdsFly.AI — Current Architecture (Jan 5, 2026)

**Status:** Production Ready (Phase 1 Complete) ✅
**Last Updated:** January 5, 2026
**Maintainer:** TechBirdsFly Team

## Executive Summary

TechBirdsFly is a full-stack **microservice architecture** for AI-powered website generation. The system uses an API Gateway pattern with multiple independent services communicating through HTTP/REST and asynchronous messaging. Currently running **6 services** in Phase 1 with Phase 2 services scaffolded and ready for development.

---

## System Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                    Frontend (Next.js)                           │
│              Port 3000 - React Multi-step Form UI              │
│         • Dashboard, Project Management, Generation UI         │
│         • Zustand State Management (authStore)                 │
│         • Axios HTTP Client (api.ts)                           │
└────────────────────────────┬────────────────────────────────────┘
                             │
                    ┌────────▼────────┐
                    │   HTTP/REST     │
                    │   JSON Payload  │
                    └────────┬────────┘
                             │
┌────────────────────────────▼────────────────────────────────────┐
│           YARP API Gateway (Port 8000)                         │
│  ┌──────────────────────────────────────────────────────────┐ │
│  │  Authentication   Path Routing   Health Checks   Logging │ │
│  │  JWT Validation   Transforms     Circuit Breaker Tracing│ │
│  └──────────────────────────────────────────────────────────┘ │
└────┬───────┬──────────┬──────────┬──────────┬────────────────┘
     │       │          │          │          │
  ┌──▼──┐ ┌─▼───┐ ┌───▼──┐ ┌───▼──┐ ┌────▼──┐
  │Auth │ │User │ │  Gen │ │Image │ │Billing│  ... (Phase 2)
  │Svc  │ │Svc  │ │  Svc │ │ Svc  │ │  Svc  │
  │5001 │ │5002 │ │ 5289 │ │ 5004 │ │ 5005  │
  └──┬──┘ └──┬──┘ └───┬──┘ └───┬──┘ └───┬──┘
     │       │        │        │        │
     │   ┌───┴────────┴────────┴────────┴──┐
     │   │  PostgreSQL 17 (Port 5432)     │
     │   │  • auth_service_db              │
     │   │  • user_service_db              │
     │   │  • generator_service_db         │
     │   │  • [image, billing, admin]_db   │
     │   └─────────────────────────────────┘
     │
     ├── SQLite (Auth Service Local)
     │
     └─ Ollama (Port 11434)
        └─ Llama 3.1 Model (4.9 GB)
           AI Website Generation
```

---

## Phase 1: Production Services (✅ Active)

### 1. Auth Service (Port 5001)
**Status:** ✅ Production Ready  
**Technology:** .NET 8 + SQLite  
**Database:** `authdb.sqlite3` (local to service)

**Responsibilities:**
- User registration and email validation
- User login with JWT token generation
- Token refresh mechanism
- Password hashing (bcrypt)
- Session management

**Key Endpoints:**
```
POST   /auth/register          → Register new user
POST   /auth/login             → User login
POST   /auth/refresh           → Refresh JWT token
POST   /auth/logout            → Logout user
GET    /auth/profile           → Get current user profile
```

**Technology Details:**
- JWT Token: RS256 signed, 1-hour expiry
- Password: bcrypt hashed, 12 salt rounds
- Database: SQLite for quick development, PostgreSQL ready for Phase 2
- Logging: Serilog → Seq

---

### 2. Generator Service (Port 5289)
**Status:** ✅ Production Ready  
**Technology:** .NET 8 + PostgreSQL + Ollama AI  
**Database:** `techbirdsfly_generator_service`

**Responsibilities:**
- AI-powered website generation (Llama 3.1)
- Project management (CRUD operations)
- Website metadata storage
- ZIP export of generated code
- Job orchestration and queuing

**Key Endpoints:**
```
POST   /generator/v1/generate          → Generate website
GET    /generator/v1/projects          → List all projects
GET    /generator/v1/projects/{id}     → Get project details
PUT    /generator/v1/projects/{id}     → Update project
DELETE /generator/v1/projects/{id}     → Delete project
GET    /generator/v1/health            → Health check
```

**AI Integration (Ollama):**
```
Ollama Configuration:
  Endpoint: http://localhost:11434
  Model: llama3.1
  Size: 4.9 GB
  API: http://localhost:11434/api/generate

Request Flow:
  1. User submits form (industry, company, style)
  2. Service constructs AI prompt
  3. Ollama generates HTML/CSS/JS content
  4. Service validates and stores in DB
  5. Returns project with generated content
  6. Frontend offers ZIP export

Generation Parameters:
  • Company Name (required)
  • Industry (Tech, Finance, Health, etc.)
  • Style (Modern, Classic, Minimal)
  • Color Palette (Blue, Purple, Green, etc.)
  • Custom Prompt (optional)
```

**Database Schema:**
```
Projects:
  • id (UUID)
  • name (string)
  • industry (string)
  • style (string)
  • palette (string)
  • htmlContent (text)
  • cssContent (text)
  • jsContent (text)
  • createdAt (timestamp)
  • updatedAt (timestamp)

GeneratedPages:
  • id (UUID)
  • projectId (FK)
  • htmlContent (text)
  • metadata (JSON)

Sections:
  • id (UUID)
  • projectId (FK)
  • title (string)
  • content (text)
  • order (int)
```

---

### 3. API Gateway (YARP) (Port 8000)
**Status:** ✅ Production Ready  
**Technology:** YARP (Yet Another Reverse Proxy)  
**Framework:** .NET 8

**Responsibilities:**
- Central request routing
- Path-based service discovery
- JWT authentication validation
- Request/response logging
- Health checks for all services
- Error handling and circuit breaking

**Route Configuration:**
```yaml
Routes:
  # Auth Service
  - path: /api/auth/**
    destination: http://localhost:5001
    transform: PathRemovePrefix /api

  # Generator Service
  - path: /api/generator/**
    destination: http://localhost:5289
    transform: PathRemovePrefix /api

  # User Service (Phase 2)
  - path: /api/users/**
    destination: http://localhost:5002
    transform: PathRemovePrefix /api

  # Image Service (Phase 2)
  - path: /api/images/**
    destination: http://localhost:5004
    transform: PathRemovePrefix /api

  # Billing Service (Phase 2)
  - path: /api/billing/**
    destination: http://localhost:5005
    transform: PathRemovePrefix /api

  # Admin Service (Phase 2)
  - path: /api/admin/**
    destination: http://localhost:5006
    transform: PathRemovePrefix /api
```

**Example Path Transform:**
```
Incoming:  POST /api/generator/v1/generate
Transform: Remove /api prefix
Send to:   POST http://localhost:5289/generator/v1/generate
Response:  Returned to client
```

**Middleware Stack:**
1. CORS handling
2. Request logging (Serilog)
3. JWT authentication
4. Service routing (YARP)
5. Response logging
6. Error handling
7. Circuit breaker (future)

---

### 4. Frontend (Next.js) (Port 3000)
**Status:** ✅ Production Ready  
**Technology:** React 18 + Next.js 15 + TypeScript  
**Styling:** TailwindCSS  
**State Management:** Zustand

**Pages & Components:**
```
/
  ├── /dashboard
  │   ├── /create         → Website generation form
  │   └── /projects       → Project management
  ├── /auth
  │   ├── /login          → Login form
  │   └── /register       → Registration form
  └── /profile
      └── /settings       → User preferences
```

**Key Features:**
- Multi-step form wizard for website generation
- Project list with filtering/sorting
- Real-time generation status
- Export functionality (ZIP)
- Authentication flows
- Responsive design (mobile-first)

**API Integration:**
```typescript
// api.ts - HTTP Client Configuration
const API_BASE = "http://localhost:8000/api"
const GATEWAY_URL = "http://localhost:8000"

// authStore.ts - State Management
- User credentials
- JWT token storage
- Login/logout handlers
- Token refresh logic
```

**Environment Configuration:**
```
.env.local:
  NEXT_PUBLIC_API_BASE=http://localhost:8000/api
  NEXT_PUBLIC_GATEWAY_URL=http://localhost:8000
  NEXT_PUBLIC_FRONTEND_URL=http://localhost:3000
```

---

## Phase 2: Scaffolded Services (🟡 Ready to Start)

### 5. User Service (Port 5002)
**Technology:** .NET 8 + PostgreSQL  
**Database:** `techbirdsfly_user_service`

**Planned Responsibilities:**
- User profile management
- User preferences (theme, notifications)
- Quota tracking (generation limits)
- Usage history
- Profile statistics

**Key Endpoints:**
```
GET    /users/{id}              → Get user profile
PUT    /users/{id}              → Update profile
GET    /users/{id}/quotas       → Get usage quotas
GET    /users/{id}/usage        → Get usage history
```

---

### 6. Image Service (Port 5004)
**Technology:** .NET 8 + PostgreSQL  
**Database:** `techbirdsfly_image_service`

**Planned Responsibilities:**
- AI image generation (DALL·E integration)
- Image storage and management
- CDN URL generation
- Image caching
- Background optimization

---

### 7. Billing Service (Port 5005)
**Technology:** .NET 8 + PostgreSQL  
**Database:** `techbirdsfly_billing_service`

**Planned Responsibilities:**
- Usage metering and tracking
- Billing calculations
- Invoice generation
- Stripe payment processing
- Subscription management
- Usage quotas enforcement

---

### 8. Admin Service (Port 5006)
**Technology:** .NET 8 + PostgreSQL  
**Database:** `techbirdsfly_admin_service`

**Planned Responsibilities:**
- User management
- Template management
- System analytics
- Audit logging
- Health monitoring
- System configuration

---

## Infrastructure Stack

### Core Services (Docker)

| Service | Port | Image | Status | Purpose |
|---------|------|-------|--------|---------|
| PostgreSQL 17 | 5432 | postgres:17-alpine | ✅ Running | Primary database |
| Ollama | 11434 | ollama/ollama:latest | ✅ Running | AI model (Llama 3.1) |
| Redis | 6379 | redis:7.4-alpine | ✅ Running | Caching & sessions |
| Seq | 5341 | datalust/seq:2024.1 | ✅ Running | Structured logging |
| Jaeger | 16686 | jaegertracing/all-in-one | ✅ Running | Distributed tracing |
| Kafka | 9092, 29092 | confluentinc/cp-kafka | ✅ Running | Event streaming |
| MongoDB | 27017 | mongo:7.0 | ✅ Running | Document DB (future) |
| Zookeeper | 2181 | confluentinc/cp-zookeeper | ✅ Running | Kafka coordination |
| SQL Server | 1433 | mcr.microsoft.com/mssql/server | ✅ Running | Optional legacy DB |

### Observability

**Seq Logging (Port 5341)**
- Endpoint: http://localhost:5341
- All structured logs from services
- Queryable, filterable interface
- Level-based filtering (Info, Warning, Error)

**Jaeger Tracing (Port 16686)**
- Endpoint: http://localhost:16686
- Distributed trace visualization
- Service dependency graphs
- Performance metrics

**Prometheus Metrics (Future)**
- Service CPU, memory, request rates
- Custom business metrics
- Alerting rules

---

## Data Architecture

### Database Per Service Pattern

```
PostgreSQL 17 (Primary DB)
├── techbirdsfly_auth_service
│   ├── Users
│   ├── Sessions
│   └── RefreshTokens
│
├── techbirdsfly_generator_service
│   ├── Projects
│   ├── GeneratedPages
│   ├── Sections
│   └── ProjectMetadata
│
├── techbirdsfly_user_service (Phase 2)
│   ├── UserProfiles
│   ├── Preferences
│   ├── Quotas
│   └── UsageHistory
│
├── techbirdsfly_billing_service (Phase 2)
│   ├── Invoices
│   ├── UsageMeters
│   └── Subscriptions
│
├── techbirdsfly_image_service (Phase 2)
│   ├── Images
│   ├── ImageMetadata
│   └── CDNUrls
│
└── techbirdsfly_admin_service (Phase 2)
    ├── AuditLogs
    ├── Templates
    └── SystemConfig

SQLite (Embedded)
└── Auth Service (Local authdb.sqlite3)
    └── User credentials & sessions (for development only)
```

### Migration Strategy
- All services use Entity Framework Core
- Automatic migrations on startup: `dotnet ef database update`
- Schema versioning for rollback capability
- Connection pooling: 25 connections per service

---

## Communication Patterns

### Synchronous (HTTP/REST)

```
Client Request Journey:
  1. Frontend → Gateway (http://localhost:3000 → http://localhost:8000)
  2. Gateway validates JWT
  3. Gateway routes based on path
  4. Service processes request
  5. Service returns response
  6. Gateway forwards to client
  
Total Latency: 10-200ms (depends on service logic)
```

**Request/Response Example:**
```
POST /api/generator/v1/generate HTTP/1.1
Host: localhost:8000
Authorization: Bearer {jwt_token}
Content-Type: application/json

{
  "companyName": "TechCorp",
  "industry": "Technology",
  "style": "Modern",
  "palette": "Blue"
}

---

HTTP/1.1 200 OK
Content-Type: application/json

{
  "success": true,
  "data": {
    "projectId": "uuid",
    "htmlContent": "...",
    "cssContent": "...",
    "jsContent": "",
    "generatedAt": "2026-01-05T10:30:00Z",
    "status": "Success"
  }
}
```

### Asynchronous (Event-Driven)

**Architecture:**
```
Services publish events to Kafka/RabbitMQ
Other services subscribe to relevant events
Non-blocking, eventual consistency

Event Examples:
  • WebsiteGenerated → Billing Service (track usage)
  • ImageGenerated → Billing Service (track usage)
  • UsageAlert → User Service (notify user)
  • QuotaExceeded → Auth Service (deny requests)
```

---

## Authentication & Authorization

### JWT Token Flow

```
1. User Registration
   POST /api/auth/register
   {
     "email": "user@example.com",
     "password": "secure_password",
     "name": "John Doe"
   }
   ↓ Auth Service validates & creates user
   Returns: { token, refreshToken }

2. User Login
   POST /api/auth/login
   {
     "email": "user@example.com",
     "password": "secure_password"
   }
   ↓ Auth Service validates credentials
   Returns: { token, refreshToken, user }

3. Authenticated Request
   GET /api/generator/v1/projects
   Headers: { Authorization: "Bearer {token}" }
   ↓ Gateway validates token signature & expiry
   ↓ Service receives decoded claims
   Returns: [projects list]

4. Token Refresh
   POST /api/auth/refresh
   {
     "refreshToken": "refresh_token_value"
   }
   ↓ Auth Service validates refresh token
   Returns: { token, refreshToken }
```

### Token Structure (JWT Payload)
```json
{
  "sub": "user-uuid",
  "email": "user@example.com",
  "name": "John Doe",
  "iat": 1704542400,
  "exp": 1704629000,
  "roles": ["user"],
  "iss": "techbirdsfly",
  "aud": "techbirdsfly-api"
}
```

### Authorization Levels
- **Public**: No token required (login, register)
- **User**: Valid token required (generation, projects)
- **Admin**: Token + admin role required (admin endpoints)

---

## Technology Details

### Backend Stack
```
Language:       C# 12.0
Framework:      .NET 8.0 LTS
ORM:            Entity Framework Core 8
Database:       PostgreSQL 17 / SQLite 3
API Style:      REST with JSON
Validation:     FluentValidation
Mapping:        AutoMapper
CQRS:           MediatR
Logging:        Serilog
Tracing:        OpenTelemetry
Gateway:        YARP
```

### Frontend Stack
```
Language:       TypeScript 5
Framework:      Next.js 15 (React 18)
Styling:        TailwindCSS 4
State Mgmt:     Zustand
HTTP Client:    Axios
Build Tool:     Webpack (Next.js)
```

### Infrastructure
```
Container:      Docker & Docker Compose
Orchestration:  Kubernetes (production-ready)
Cloud:          Azure (recommended)
CI/CD:          GitHub Actions (recommended)
Registry:       Azure Container Registry
```

---

## Key Achievements (Phase 1)

✅ **Architecture Complete**
- Microservices pattern with API Gateway
- Clear separation of concerns
- Scalable, independent services

✅ **Authentication Implemented**
- Secure JWT-based authentication
- Token refresh mechanism
- User registration & login

✅ **Website Generation Working**
- Ollama Llama 3.1 AI integration
- Full HTML/CSS generation
- Project persistence
- ZIP export capability

✅ **API Gateway Live**
- Centralized routing
- JWT validation
- Health monitoring
- Path transformations

✅ **Frontend Interactive**
- Multi-step generation form
- Project management UI
- Real-time status updates
- Export functionality

✅ **Observability Ready**
- Structured logging (Seq)
- Distributed tracing (Jaeger)
- Health checks
- Performance metrics

---

## Quick Start Guide

### Start Everything (5 minutes)

```bash
# 1. Start infrastructure
cd infra
docker-compose -f docker-compose.yml up -d

# 2. Wait for containers to be ready (30 seconds)
sleep 30

# 3. Build all services
cd ..
dotnet build TechBirdsFly.sln

# 4. Start services in separate terminals

# Terminal 1: Auth Service
cd services/auth-service/src
dotnet run --urls http://localhost:5001

# Terminal 2: Generator Service
cd services/generator-service/src
dotnet run --urls http://localhost:5289

# Terminal 3: API Gateway
cd gateway/yarp-gateway/src
dotnet run --urls http://localhost:8000

# Terminal 4: Frontend
cd web-frontend/techbirdsfly-frontend-nextjs
npm install
npm run dev  # Runs on http://localhost:3000
```

### Access Points

| Component | URL | Purpose |
|-----------|-----|---------|
| Frontend | http://localhost:3000 | User interface |
| API Gateway | http://localhost:8000 | API endpoint |
| Auth Service | http://localhost:5001 | Authentication |
| Generator Service | http://localhost:5289 | Generation |
| Seq Logs | http://localhost:5341 | Logging dashboard |
| Jaeger Traces | http://localhost:16686 | Tracing dashboard |
| PostgreSQL | localhost:5432 | Database |
| Ollama | http://localhost:11434 | AI model |

---

## Troubleshooting

### Service Won't Start
```bash
# Check if port is available
lsof -i :5289

# Check logs
docker logs techbirdsfly-postgres
docker logs techbirdsfly-ollama

# Run migrations
cd services/generator-service/src
dotnet ef database update
```

### Generation Timeout
```bash
# Check Ollama status
curl http://localhost:11434/api/tags

# Restart Ollama
docker restart techbirdsfly-ollama

# Check available models
docker exec techbirdsfly-ollama ollama list
```

### Database Connection Issues
```bash
# Check PostgreSQL
docker exec techbirdsfly-postgres psql -U postgres -c "SELECT 1"

# Check password
# Default: Alisheikh@123

# View connection strings
grep "ConnectionString" appsettings.json
```

### Gateway Not Routing
```bash
# Test direct service
curl http://localhost:5289/health

# Test through gateway
curl http://localhost:8000/api/generator/v1/health

# Check gateway health
curl http://localhost:8000/health
```

---

## Next Steps (Phase 2 & Beyond)

### Immediate (Next 2 weeks)
- [ ] Implement User Service (profiles, quotas)
- [ ] Add Image Service (DALL·E integration)
- [ ] Setup Billing Service (Stripe integration)
- [ ] Create Admin Service (dashboard, templates)

### Short-term (1-2 months)
- [ ] Event-driven communication (Kafka)
- [ ] Caching layer optimization
- [ ] Rate limiting
- [ ] Advanced search & filtering
- [ ] Notification system

### Medium-term (2-4 months)
- [ ] Mobile app (React Native)
- [ ] GraphQL API option
- [ ] Advanced analytics
- [ ] Multi-tenancy support
- [ ] Custom domain support

### Long-term (6+ months)
- [ ] Machine learning for personalization
- [ ] Advanced AI models
- [ ] Marketplace for templates
- [ ] Agency features
- [ ] White-label solution

---

## Document Information

| Property | Value |
|----------|-------|
| **Created** | December 2025 |
| **Updated** | January 5, 2026 |
| **Status** | Production Ready |
| **Version** | 1.0 |
| **Author** | TechBirdsFly Team |
| **Review Cycle** | Monthly |

---

**For questions or updates, contact the TechBirdsFly development team.**
