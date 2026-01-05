# TechBirdsFly Architecture Summary - January 2026

## 🎯 Current Status

**Status:** ✅ **Production Ready (Phase 1 Complete)**  
**Last Updated:** January 5, 2026  
**Active Services:** 4/6 (Phase 1 Complete + Phase 2 Scaffolded)

---

## 🏗️ System Architecture

```
┌─────────────────────────────────────────────────────┐
│         Frontend (Next.js React App)                │
│         Port 3000 - Interactive UI                 │
└────────────────────┬────────────────────────────────┘
                     │ HTTP REST + JWT
                     ▼
┌─────────────────────────────────────────────────────┐
│      API Gateway (YARP Reverse Proxy)              │
│      Port 8000 - Central Router & Auth             │
└─┬──────────┬──────────┬──────────┬────────────────┘
  │          │          │          │
  ▼          ▼          ▼          ▼
┌────┐  ┌────┐  ┌────┐  ┌──────┐
│Auth│  │User│  │Gen │  │Image │ ... (Phase 2)
│5001│  │5002│  │5289│  │5004  │
└────┘  └────┘  └────┘  └──────┘
  │        │       │        │
  └────────┴───────┴────────┘
           │
           ▼
    PostgreSQL 17 (5432)
    + Ollama (11434)
    + Redis (6379)
```

---

## ✅ Phase 1 Services (Production Ready)

### 1. **Auth Service** (Port 5001)
- **Technology:** .NET 8 + SQLite
- **Endpoints:** /auth/register, /auth/login, /auth/refresh
- **Features:**
  - User registration with email validation
  - Secure JWT token generation
  - Token refresh mechanism
  - Password hashing with bcrypt
- **Status:** ✅ Running & Tested

### 2. **Generator Service** (Port 5289)
- **Technology:** .NET 8 + PostgreSQL + Ollama AI
- **Database:** `techbirdsfly_generator_service`
- **AI Model:** Llama 3.1 (4.9 GB)
- **Endpoints:** /generator/v1/generate, /projects, /projects/{id}
- **Features:**
  - ✨ AI-powered website generation (Llama 3.1)
  - HTML/CSS/JavaScript generation
  - Project management (CRUD)
  - ZIP export functionality
  - Generated website persistence
- **Status:** ✅ Generating websites with AI

### 3. **API Gateway** (Port 8000)
- **Technology:** YARP (Yet Another Reverse Proxy)
- **Features:**
  - Centralized request routing
  - JWT authentication validation
  - Path-based routing with transforms
  - Service health monitoring
  - Request/response logging
- **Routes:** /api/auth/**, /api/generator/**, /api/users/**, etc.
- **Status:** ✅ Routing all requests

### 4. **Frontend** (Port 3000)
- **Technology:** Next.js 15 + React 18 + TypeScript
- **UI:** TailwindCSS
- **State:** Zustand (authStore)
- **Features:**
  - Multi-step website generation form
  - Project management dashboard
  - Authentication flows
  - Export/download functionality
- **Status:** ✅ Interactive and responsive

---

## 🟡 Phase 2 Services (Scaffolded & Ready)

| Service | Port | Purpose |
|---------|------|---------|
| **User Service** | 5002 | User profiles, preferences, quotas |
| **Image Service** | 5004 | AI image generation (DALL·E), CDN |
| **Billing Service** | 5005 | Usage tracking, invoicing, Stripe |
| **Admin Service** | 5006 | Admin dashboard, templates, audit |

---

## 🗄️ Infrastructure Stack

### Core Services (Docker)
```
PostgreSQL 17          (Port 5432) ✅ Running
├─ auth_service_db
├─ generator_service_db
├─ user_service_db (Phase 2)
├─ image_service_db (Phase 2)
├─ billing_service_db (Phase 2)
└─ admin_service_db (Phase 2)

Ollama AI              (Port 11434) ✅ Running
└─ Llama 3.1 Model (4.9 GB)

Redis                  (Port 6379) ✅ Running
├─ Caching
├─ Sessions
└─ Message Queues

Observability Stack    ✅ Running
├─ Seq Logging (5341)
├─ Jaeger Tracing (16686)
├─ Kafka (9092, 29092)
├─ Zookeeper (2181)
└─ MongoDB (27017)
```

---

## 📡 API Routes (Through Gateway)

```
Authentication:
  POST   /api/auth/register           → Register user
  POST   /api/auth/login              → User login
  POST   /api/auth/refresh            → Refresh token
  POST   /api/auth/logout             → Logout

Website Generation:
  POST   /api/generator/v1/generate   → Generate website
  GET    /api/generator/v1/projects   → List projects
  GET    /api/generator/v1/projects/{id}   → Get details
  PUT    /api/generator/v1/projects/{id}   → Update
  DELETE /api/generator/v1/projects/{id}   → Delete

User Management (Phase 2):
  GET    /api/users/{id}              → Get profile
  PUT    /api/users/{id}              → Update profile
  GET    /api/users/{id}/quotas       → Get quotas

Images (Phase 2):
  POST   /api/images/generate         → Generate image

Billing (Phase 2):
  GET    /api/billing/usage           → Get usage

Admin (Phase 2):
  GET    /api/admin/users             → List users
```

---

## 🚀 Quick Start

### 1. Start Infrastructure (Docker)
```bash
cd infra
docker-compose -f docker-compose.yml up -d
sleep 30  # Wait for containers to initialize
```

### 2. Build All Services
```bash
dotnet build TechBirdsFly.sln --configuration Debug
```

### 3. Start Services (Each in Separate Terminal)
```bash
# Terminal 1: Auth Service
cd services/auth-service/src && dotnet run

# Terminal 2: Generator Service
cd services/generator-service/src && dotnet run

# Terminal 3: API Gateway
cd gateway/yarp-gateway/src && dotnet run

# Terminal 4: Frontend
cd web-frontend/techbirdsfly-frontend-nextjs
npm install && npm run dev
```

### 4. Access Application
| Component | URL |
|-----------|-----|
| Frontend | http://localhost:3000 |
| API Gateway | http://localhost:8000/health |
| Seq Logs | http://localhost:5341 |
| Jaeger Traces | http://localhost:16686 |

---

## 🔐 Authentication Flow

```
1. User Registration / Login
   ↓
2. Auth Service validates credentials & creates JWT
   ↓
3. Frontend stores JWT in localStorage
   ↓
4. Frontend includes: Authorization: Bearer {token}
   ↓
5. Gateway validates JWT signature & expiry
   ↓
6. Gateway forwards to service with user context
   ↓
7. Service uses claims from JWT for authorization
```

### JWT Token Example
```json
{
  "sub": "user-uuid",
  "email": "user@example.com",
  "iat": 1704542400,
  "exp": 1704629000,
  "roles": ["user"]
}
```

---

## 🧠 AI Integration (Ollama Llama 3.1)

### How It Works
```
1. User submits form (company name, industry, style)
   ↓
2. Generator Service constructs AI prompt
   ↓
3. Ollama processes prompt (Llama 3.1 model)
   ↓
4. Service receives HTML/CSS/JS content
   ↓
5. Content validated and stored in PostgreSQL
   ↓
6. Frontend displays result & offers ZIP export
```

### Configuration
```json
{
  "Ollama": {
    "Endpoint": "http://localhost:11434",
    "Model": "llama3.1",
    "ApiKey": "optional"
  }
}
```

### Generation Time
- **Typical:** 60-120 seconds
- **Complex sites:** 120+ seconds
- **Timeout:** 100 seconds (configurable)

---

## 🛠️ Technology Stack

### Backend
```
Language:       C# 12.0
Runtime:        .NET 8.0 LTS
ORM:            Entity Framework Core 8
Database:       PostgreSQL 17 / SQLite 3
API Pattern:    REST + JSON
Architecture:   CQRS (MediatR) + Microservices
Validation:     FluentValidation
Mapping:        AutoMapper
Logging:        Serilog → Seq
Tracing:        OpenTelemetry → Jaeger
```

### Frontend
```
Language:       TypeScript 5.0
Framework:      Next.js 15 (React 18)
Styling:        TailwindCSS 4
State:          Zustand
HTTP:           Axios
Build:          Webpack
```

### Infrastructure
```
Containers:     Docker & Docker Compose
Gateway:        YARP
AI:             Ollama (Local LLM)
Cloud:          Azure (recommended)
Orchestration:  Kubernetes (production-ready)
```

---

## ✨ Key Achievements

✅ **Microservice Architecture**
- 6 independent services with clear responsibilities
- API Gateway for centralized routing
- Database per service pattern

✅ **Authentication System**
- Secure JWT-based authentication
- User registration and login
- Token refresh mechanism

✅ **AI Website Generation**
- Ollama Llama 3.1 AI integration
- Full HTML/CSS/JS generation
- Project persistence
- ZIP export

✅ **Frontend**
- Interactive multi-step form
- Project management UI
- Real-time status updates
- Export functionality

✅ **Observability**
- Structured logging (Seq)
- Distributed tracing (Jaeger)
- Service health checks
- Performance metrics

✅ **Full Documentation**
- Architecture docs
- API guides
- Troubleshooting
- Quick start

---

## 📚 Documentation

| Document | Location | Purpose |
|----------|----------|---------|
| **Full Architecture** | `docs/CURRENT_ARCHITECTURE.md` | Comprehensive architecture details |
| **Service Guide** | `services/README.md` | Service inventory and setup |
| **Quick Start** | `README.md` | Getting started guide |
| **This Summary** | `ARCHITECTURE_SUMMARY_JAN_2026.md` | Overview (you are here) |

---

## 🎯 Next Phase (Phase 2)

### Immediate (Next 2 weeks)
- [ ] User Service (profiles, quotas)
- [ ] Image Service (DALL·E)
- [ ] Billing Service (Stripe)
- [ ] Admin Service (dashboard)

### Infrastructure
- [ ] Kafka for event streaming
- [ ] Advanced caching
- [ ] Rate limiting
- [ ] Kubernetes manifests

### Features
- [ ] Mobile app (React Native)
- [ ] GraphQL API
- [ ] Multi-tenancy
- [ ] Custom domains
- [ ] Analytics dashboard

---

## 🐛 Troubleshooting

### Service Won't Start
```bash
# Check if port is available
lsof -i :5289

# Run migrations
cd services/generator-service/src
dotnet ef database update

# Check database
docker logs techbirdsfly-postgres
```

### Ollama Timeout
```bash
# Check Ollama status
curl http://localhost:11434/api/tags

# Restart
docker restart techbirdsfly-ollama

# Check models
docker exec techbirdsfly-ollama ollama list
```

### Gateway Not Routing
```bash
# Test direct service
curl http://localhost:5289/health

# Test through gateway
curl http://localhost:8000/api/generator/v1/health
```

---

## 📊 Service Status Dashboard

| Service | Port | Status | Health | Database |
|---------|------|--------|--------|----------|
| Auth | 5001 | ✅ Active | Healthy | SQLite |
| User | 5002 | 🟡 Ready | - | PostgreSQL |
| Generator | 5289 | ✅ Active | Healthy | PostgreSQL |
| Image | 5004 | 🟡 Ready | - | PostgreSQL |
| Billing | 5005 | 🟡 Ready | - | PostgreSQL |
| Admin | 5006 | 🟡 Ready | - | PostgreSQL |
| Gateway | 8000 | ✅ Active | Healthy | - |
| Frontend | 3000 | ✅ Active | Healthy | - |

---

## 🔗 Useful Links

- **Frontend:** http://localhost:3000
- **API Gateway:** http://localhost:8000/health
- **Logs Dashboard:** http://localhost:5341
- **Trace Visualization:** http://localhost:16686
- **Database:** localhost:5432 (admin credentials in .env)
- **AI Model:** http://localhost:11434/api/tags

---

## 📝 Notes

- All services are stateless and horizontally scalable
- Database per service pattern for independence
- JWT-based authentication with gateway validation
- Structured logging for debugging
- Distributed tracing for performance monitoring
- Ready for Kubernetes deployment
- Azure integration recommended for production

---

**Document Status:** ✅ Current as of January 5, 2026  
**Maintainer:** TechBirdsFly Team  
**Next Review:** February 5, 2026
