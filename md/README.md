# TechBirdsFly — AI-Powered Website Generator# TechBirdsFly.AI — AI-Powered Website Generator



A modern, full-stack microservices application that generates professional, ready-to-deploy websites from simple text prompts using AI.A modern, full-stack application that uses AI to generate professional, ready-to-deploy websites from simple text prompts.



**Status**: Phase 2 - Docker & Orchestration Complete ✅ | **Architecture**: Microservices (.NET 8 + Next.js) | **Deployment**: Docker Ready + Azure-Ready**Status**: MVP Phase 1 ✅ | **Architecture**: Microservices (.NET 8 + React) | **Deployment**: Azure-ready



---## 🎯 What It Does



## 🎯 Quick Overview1. User enters a prompt: *"Create a modern portfolio website for a photographer"*

2. Backend calls Azure OpenAI (GPT-4o-mini) for content & layout ideas

- **12+ Microservices** in .NET 83. Generates a complete React project with Tailwind CSS styling

- **Next.js Frontend** for user interface4. User previews the site live in the browser

- **API Gateway** (YARP) routing all requests5. Downloads as a ready-to-deploy ZIP file

- **Dockerized Infrastructure** (PostgreSQL, MongoDB, Redis, Kafka, Seq, Jaeger)

- **Docker Compose Manager** CLI for easy deployment## 🏗️ Architecture

- **Clean Architecture** throughout all services

- **Production Ready** with comprehensive documentation```

┌──────────────────────────────────────────────────────────────┐

---│                    React Frontend (Port 3000)                │

│              Tailwind CSS + shadcn/ui Components             │

## 🏗️ Current Architecture└────────────────────────┬─────────────────────────────────────┘

                         │ HTTP/REST

```                ┌────────▼────────────┐

Frontend (Next.js, Port 3000)                │  API Gateway (YARP) │

        ↓ HTTP/REST                │   Port 5500         │

API Gateway (YARP, Port 9000)                └────┬───────────────┬┤

        ↓ Routes         ┌──────────┬┤  Routes       ├┴──────────┐

┌───────────────────────────────────────────────────┐         │          │                │           │

│              12 MICROSERVICES                      │    ┌────▼──┐  ┌────▼─────┐  ┌───────▼──┐  ┌────▼───┐

├───────────────────────────────────────────────────┤    │ Auth  │  │  Billing  │  │   Image  │  │  Admin │

│                                                     │    │ 5001  │  │   5177    │  │   5007   │  │  5006  │

│  🔐 Auth (5001)      📁 Project (5009)            │    └───────┘  └───────────┘  └──────────┘  └────────┘

│  👤 User (5008)      ⚡ Cache (5021)              │         │           │            │            │

│  💳 Billing (5002)   🎬 Media (5022)              │    ┌────▼──────┐  ┌─────▼────┐  ┌───▼────┐      │

│  ⚙️  Generator (5003) 📨 EventBus (5020)          │    │PostgreSQL │  │PostgreSQL│  │MongoDB  │      │

│  📤 Export (5004)    🖼 Image (5007)              │    └───────────┘  └──────────┘  └─────────┘      │

│  🛠 Admin (5006)                                  │                                               │

│                                                     │            ┌──────────────────────────────────┤

└───────────────────────────────────────────────────┘            │                                  │

        ↓ Database/Cache/Queue      ┌─────▼─────┐  ┌─────────────┐  ┌────────▼───┐

┌───────────────────────────────────────────────────┐      │   User    │  │   EventBus  │  │   Cache    │

│           INFRASTRUCTURE SERVICES                  │      │   5005    │  │   5030      │  │   8100     │

├───────────────────────────────────────────────────┤      └───────────┘  └─────────────┘  └────────────┘

│                                                     │            │              │                   │

│  📦 PostgreSQL (5433)  - Primary DB              │      ┌─────▼──┐     ┌─────▼─────┐       ┌────▼───┐

│  📦 MongoDB (27017)    - Media storage            │      │PostgreSQL    │PostgreSQL │       │ Redis  │

│  📦 Redis (6379)       - Caching layer           │      └──────────┘   └───────────┘       └────────┘

│  📦 Kafka (9092)       - Message queue           │

│  📦 Seq (5341)         - Centralized logging     │Infrastructure:

│  📦 Jaeger (16686)     - Distributed tracing     │- PostgreSQL (5433): Auth, User, Billing, EventBus, Admin, Generator services

│                                                     │- MongoDB (27017): Image Service

└───────────────────────────────────────────────────┘- Redis (6379): Caching

```- Kafka (9092): Event streaming

```

---

### Microservices Overview

## 📋 Project Structure

| Service | Port | Purpose | Database | Status |

```|---------|------|---------|----------|--------|

TechBirdsFly/| **API Gateway** | 5500 | Route requests to services | - | ✅ Running |

├── md/                              ✅ ALL MARKDOWN DOCS HERE| **Auth Service** | 5001 | User registration, login, JWT tokens | PostgreSQL | ✅ Running |

│   ├── CONSOLIDATION_SUMMARY.md| **User Service** | 5005 | User profiles, settings management | PostgreSQL | ✅ Running |

│   ├── DOCUMENTATION_ORGANIZATION.md| **Billing Service** | 5177 | Billing, subscriptions, payments | PostgreSQL | ⏳ Ready |

│   ├── DOCKER_SETUP_GUIDE.md| **Image Service** | 5007 | Image processing, AI image generation | MongoDB | ⏳ Ready |

│   ├── DOCKER_QUICK_START.md| **EventBus Service** | 5030 | Async events, event publishing | PostgreSQL | ⏳ Ready |

│   ├── PROJECT_SERVICE_COMPARISON.md| **Admin Service** | 5006 | Admin dashboard, monitoring | PostgreSQL | ⏳ Ready |

│   ├── PROJECT_SERVICE_CONSOLIDATION_COMPLETE.md| **Generator Service** | 5003 | Website generation, project management | PostgreSQL | ⏳ Ready |

│   └── [other documentation...]| **Cache Service** | 8100 | Distributed caching layer | Redis | ⏳ Ready |

│| **Frontend** | 3000 | React SPA - User interface | - | ✅ Running |

├── docker/                          Docker orchestration

│   ├── docker-compose.debug.yml     Development setup (all 20 services)### Service Responsibilities

│   ├── docker-compose.prod.yml      Production setup (replicas + limits)

│   └── [docker configs...]- **Auth Service**: JWT token generation, user authentication, password management

│- **User Service**: User profiles, preferences, account settings

├── services/                        12 Microservices (Clean Architecture)- **Billing Service**: Subscription plans, payment processing, invoice generation

│   ├── auth-service/                🔐 Authentication & JWT- **Image Service**: Image upload, storage, AI-powered image generation via DALL·E

│   ├── user-service/                👤 User profiles & settings- **EventBus Service**: Async event handling, service-to-service communication

│   ├── billing-service/             💳 Subscriptions & payments- **Admin Service**: System monitoring, user management, analytics dashboard

│   ├── generator-service/           ⚙️  Website generation- **Generator Service**: Website project management, AI content generation, ZIP packaging

│   ├── export-service/              📤 Project export (HTML, React, Next.js, ZIP)- **Cache Service**: Distributed caching with Redis backend, shared across all services

│   ├── image-service/               🖼 Image processing- **API Gateway**: Request routing, load balancing, health checks, request/response logging

│   ├── admin-service/               🛠 Admin dashboard

│   ├── event-bus-service/           📨 Async event publishing## 📋 Project Structure

│   ├── cache-service/               ⚡ Redis caching layer

│   ├── media-service/               🎬 Media management```

│   ├── project-service/             📁 Project management (CONSOLIDATED)TechBirdsFly/

│   └── editor-service/              ✏️ Project editor├─ .github/

││  └─ copilot-instructions.md        # Development checklist

├── gateway/├─ docs/

│   └── yarp-gateway/                API Gateway (YARP)│  ├─ architecture.md                # Service design details

│       └── src/│  ├─ architecture_mermaid.md        # Diagrams & flows

│           ├── Program.cs│  └─ README.md

│           ├── appsettings.json     Routes for all services├─ infra/

│           └── Properties/│  ├─ docker-compose.yml             # Docker infrastructure

││  └─ k8s/                           # Kubernetes configs

├── web-frontend/├─ gateway/

│   └── techbirdsfly-frontend-nextjs/ Next.js 18 + TypeScript + Tailwind│  └─ yarp-gateway/

│       ├── pages/│     └─ src/

│       ├── components/│        ├─ Program.cs

│       ├── lib/│        ├─ appsettings.json         # Routes for all services

│       ├── public/│        └─ Properties/

│       └── next.config.js├─ services/                         # Microservices

││  ├─ auth-service/

├── infra/│  │  ├─ AuthService/                # .NET 8 API

│   ├── docker-compose.yml           Infrastructure services│  │  │  ├─ Controllers/

│   ├── docker-compose.dev.yml       Development overrides│  │  │  ├─ Services/

│   └── k8s/                         Kubernetes configs (planned)│  │  │  ├─ Data/                    # EF Core DbContext

││  │  │  ├─ Models/

├── gateway/                         API Gateway setup│  │  │  └─ Migrations/

│   ├── GATEWAY_INTEGRATION_COMPLETE.md│  │  └─ Dockerfile

│   ├── QUICK_START.md│  ├─ user-service/

│   └── README.md│  │  ├─ UserService/                # .NET 8 API

││  │  └─ Dockerfile

├── docs/│  ├─ billing-service/

│   ├── architecture.md              System design details│  │  ├─ BillingService/             # .NET 8 API

│   ├── architecture_mermaid.md      Diagrams & flows│  │  └─ Dockerfile

│   └── README.md                    Documentation index│  ├─ image-service/

││  │  ├─ ImageService/               # .NET 8 API + MongoDB

├── .vscode/│  │  └─ Dockerfile

│   ├── launch.json                  Debug configurations (13 services + 4 compound configs)│  ├─ eventbus-service/

│   ├── tasks.json                   Build tasks (7 services + all)│  │  ├─ EventBusService/            # .NET 8 API + Kafka

│   └── settings.json                Editor settings│  │  └─ Dockerfile

││  ├─ admin-service/

├── .github/                         CI/CD workflows│  │  ├─ AdminService/               # .NET 8 API

├── TechBirdsFly.sln                 Visual Studio solution│  │  └─ Dockerfile

├── docker-compose-manager.sh        ✨ CLI for Docker management (400+ lines)│  ├─ generator-service/

├── README.md                        This file│  │  ├─ GeneratorService/           # .NET 8 API

├── .gitignore│  │  └─ Dockerfile

└── .env.example│  ├─ cache-service/

```│  │  ├─ CacheService/               # .NET 8 API + Redis

│  │  └─ Dockerfile

---│  └─ README.md                      # Services overview

├─ web-frontend/

## 🚀 Quick Start Guide│  └─ techbirdsfly-frontend-nextjs/  # React 18 TypeScript

│     ├─ pages/

### Prerequisites│     ├─ components/

│     ├─ lib/

- Docker & Docker Compose installed│     ├─ auth.ts

- .NET 8 SDK (for local development)│     └─ next.config.js

- Node.js 18+ (for frontend development)├─ TechBirdsFly.sln                  # Visual Studio solution

- Optional: VS Code with C# & JavaScript extensions└─ README.md (this file)

```

### Option 1: Docker Compose (Recommended) ⭐

## 🚀 Quick Start

**Fastest way to get everything running:**

### Prerequisites

```bash- .NET 8 SDK

# 1. Navigate to project- Node.js 18+

cd /Users/alirazatahir/Desktop/Ali-Library/Project/Self/TechBirdsFly- PostgreSQL 12+ (EnterpriseDB or Docker)

- MongoDB (Docker recommended)

# 2. Build all Docker images (5-10 minutes first time)- Optional: Docker & Docker Compose

./docker-compose-manager.sh build

### Option 1: Local Development (Recommended)

# 3. Start all services (wait 60-90 seconds for health checks)

./docker-compose-manager.sh up#### Step 1: Start Infrastructure (Docker)

```bash

# 4. Verify everything runningdocker compose -f infra/docker-compose.yml up -d

./docker-compose-manager.sh status# Starts: PostgreSQL, MongoDB, Redis, Kafka, Zookeeper, Schema Registry

```

# 5. Access the application

🌐 Frontend:    http://localhost:3000#### Step 2: Start Backend Services (4 terminals)

🚪 API Gateway: http://localhost:9000

📊 Logs (Seq):  http://localhost:5341**Terminal 1 - Auth Service** (Port 5001)

🔍 Traces:      http://localhost:16686```bash

```cd services/auth-service/AuthService

dotnet run --urls http://localhost:5001

**That's it!** All 20 services (12 microservices + 8 infrastructure) running ✅```



### Option 2: Local Development (Multiple Terminals)**Terminal 2 - User Service** (Port 5005)

```bash

**For active development with hot-reload:**cd services/user-service/UserService

dotnet run --urls http://localhost:5005

```bash```

# Terminal 1: Start infrastructure

cd /Users/alirazatahir/Desktop/Ali-Library/Project/Self/TechBirdsFly**Terminal 3 - API Gateway** (Port 5500)

docker-compose -f docker/docker-compose.debug.yml up -d```bash

cd gateway/yarp-gateway/src

# Terminal 2: Start Frontenddotnet run --urls http://localhost:5500

cd web-frontend/techbirdsfly-frontend-nextjs```

npm install  # First time only

npm run dev**Terminal 4 - Frontend** (Port 3000)

```bash

# Terminal 3-5: Start microservices individuallycd web-frontend/techbirdsfly-frontend

# See VS Code debug configurations (F5) for easier startupnpm install  # First time only

```npm start    # Opens http://localhost:3000

```

### Option 3: VS Code Debug (Most Convenient)

#### Step 3: Verify All Services Running

**All debug configurations pre-configured:**```bash

# Check Gateway health

```bashcurl http://localhost:5500/health

# 1. Open VS Code

code /Users/alirazatahir/Desktop/Ali-Library/Project/Self/TechBirdsFly# Check Auth Service

curl http://localhost:5001/health

# 2. Press F5 to see debug options:

#    - Individual services (13 options)# Check User Service

#    - Compound configurations (4 options)curl http://localhost:5005/health

#      • 🔵 All .NET Services + Frontend

#      • 🔧 Core Services Only (Auth, User, Billing, Gateway)# Check Frontend

#      • 📊 Data & Infrastructure Servicesopen http://localhost:3000

#      • ⚙️ Processing Services```



# 3. Select desired configuration and start debugging**All 4 services running** ✅ → Ready for end-to-end testing!

```

### Option 2: Full Stack with Docker Compose

---

```bash

## 📊 Services Overview# Start all infrastructure

docker compose -f infra/docker-compose.yml up -d

### Core Services (Essential)

# Start backend services (as above, 4 terminals)

| Service | Port | Purpose | Database | Status |# Or use Docker Compose override file for full containerization

|---------|------|---------|----------|--------|```

| **API Gateway** | 9000 | Route & balance load | - | ✅ Ready |

| **Auth Service** | 5001 | JWT tokens, registration | PostgreSQL | ✅ Ready |### Option 3: Quick Gateway Test

| **User Service** | 5008 | User profiles & settings | PostgreSQL | ✅ Ready |

| **Frontend** | 3000 | Next.js UI | - | ✅ Ready |```bash

# Get Auth token

### Feature Services (Extended Functionality)curl -X POST http://localhost:5001/api/auth/login \

  -H "Content-Type: application/json" \

| Service | Port | Purpose | Database | Status |  -d '{"email": "test@example.com", "password": "Password123!"}'

|---------|------|---------|----------|--------|

| **Billing Service** | 5002 | Subscriptions, payments | PostgreSQL | ✅ Ready |# Extract token from response, then test Gateway routing

| **Generator Service** | 5003 | Website generation | PostgreSQL | ✅ Ready |curl http://localhost:5500/api/auth/me \

| **Export Service** | 5004 | Export to HTML/React/Next/ZIP | PostgreSQL | ✅ Ready |  -H "Authorization: Bearer <YOUR_TOKEN>"

| **Image Service** | 5007 | Image processing | MongoDB | ✅ Ready |```

| **Admin Service** | 5006 | Admin dashboard | PostgreSQL | ✅ Ready |

| **Project Service** | 5009 | Project management (Clean Architecture) | PostgreSQL | ✅ Ready |### Connection Strings



### Infrastructure Services (Data & Events)Ensure these match your environment:



| Service | Port | Purpose | Status |**PostgreSQL Databases (Local)**

|---------|------|---------|--------|```

| **EventBus Service** | 5020 | Async event publishing | ✅ Ready |Auth Service:        Host=localhost;Port=5432;Database=techbirdsfly_auth;Username=postgres;Password=postgres123

| **Cache Service** | 5021 | Redis caching layer | ✅ Ready |User Service:        Host=localhost;Port=5432;Database=techbirdsfly_user;Username=postgres;Password=postgres123

| **Media Service** | 5022 | Media management | ✅ Ready |Billing Service:     Host=localhost;Port=5432;Database=techbirdsfly_billing;Username=postgres;Password=postgres123

EventBus Service:    Host=localhost;Port=5432;Database=techbirdsfly_eventbus;Username=postgres;Password=postgres123

---Admin Service:       Host=localhost;Port=5432;Database=techbirdsfly_admin;Username=postgres;Password=postgres123

Generator Service:   Host=localhost;Port=5432;Database=techbirdsfly_generator;Username=postgres;Password=postgres123

## 🛠️ Docker Compose Manager CLI```



**Powerful CLI for all Docker operations:****PostgreSQL Databases (Docker)**

```

```bashAuth Service:        Host=localhost;Port=5433;Database=techbirdsfly_auth;Username=postgres;Password=postgres123

# View all commandsUser Service:        Host=localhost;Port=5433;Database=techbirdsfly_user;Username=postgres;Password=postgres123

./docker-compose-manager.sh helpBilling Service:     Host=localhost;Port=5433;Database=techbirdsfly_billing;Username=postgres;Password=postgres123

EventBus Service:    Host=localhost;Port=5433;Database=techbirdsfly_eventbus;Username=postgres;Password=postgres123

# Start everythingAdmin Service:       Host=localhost;Port=5433;Database=techbirdsfly_admin;Username=postgres;Password=postgres123

./docker-compose-manager.sh upGenerator Service:   Host=localhost;Port=5433;Database=techbirdsfly_generator;Username=postgres;Password=postgres123

```

# Stop everything

./docker-compose-manager.sh down**MongoDB**

```

# View logs (all or specific)mongodb://localhost:27017

./docker-compose-manager.sh logs```

./docker-compose-manager.sh logs auth-service

**Redis**

# Build Docker images```

./docker-compose-manager.sh buildlocalhost:6379

```

# Rebuild without cache

./docker-compose-manager.sh rebuild## 📚 API Documentation



# Show running containers### API Gateway Routing

./docker-compose-manager.sh ps

All services are accessed through the API Gateway at `http://localhost:5500`:

# Show health status

./docker-compose-manager.sh status```

/api/auth/**      → Auth Service (5001)

# Clean up (containers + volumes)/api/users/**     → User Service (5005)

./docker-compose-manager.sh clean/api/billing/**   → Billing Service (5177)

```/api/images/**    → Image Service (5007)

/api/events/**    → EventBus Service (5030)

---/api/admin/**     → Admin Service (5006)

```

## 📚 Documentation Location

### Auth Service (`/api/auth`)

**✨ All markdown documentation is organized in `/md/` folder:**

**Direct**: `http://localhost:5001`  

| Document | Purpose | Location |**Via Gateway**: `http://localhost:5500/api/auth`

|----------|---------|----------|

| CONSOLIDATION_SUMMARY.md | Project service consolidation overview | md/ || Endpoint | Method | Purpose | Auth |

| PROJECT_SERVICE_CONSOLIDATION_COMPLETE.md | Detailed consolidation report | md/ ||----------|--------|---------|------|

| PROJECT_SERVICE_COMPARISON.md | Service comparison & analysis | md/ || `/register` | POST | Register new user | ❌ |

| DOCUMENTATION_ORGANIZATION.md | Documentation guidelines | md/ || `/login` | POST | Login & get JWT | ❌ |

| DOCKER_SETUP_GUIDE.md | Complete Docker setup & configuration | md/ || `/refresh` | POST | Refresh access token | ✅ JWT |

| DOCKER_SETUP_COMPLETE.md | Docker setup completion details | md/ || `/verify-email` | GET | Verify email link | ❌ |

| DOCKER_QUICK_START.md | 5-minute Docker quick start | md/ || `/me` | GET | Get current user | ✅ JWT |

| GATEWAY_INTEGRATION_COMPLETE.md | Gateway setup documentation | gateway/ || `/health` | GET | Health check | ❌ |

| EXPORT_PROJECT_FEATURE_COMPLETE.md | Export feature documentation | md/ |

| SEO_SETTINGS_FEATURE_COMPLETE.md | SEO settings feature docs | md/ |**Example: Register**

| THEME_SETTINGS_FEATURE_COMPLETE.md | Theme settings feature docs | md/ |```bash

curl -X POST http://localhost:5500/api/auth/register \

---  -H "Content-Type: application/json" \

  -d '{

## 🔄 Recent Accomplishments    "fullName": "Ali Raza",

    "email": "ali@example.com",

### Session Summary (November 27, 2025)    "password": "SecurePass123!"

  }'

1. **✅ Project Service Consolidation**```

   - Identified 2 duplicate Project Service implementations

   - Deleted monolithic version (`services/project-service/`)**Response:**

   - Kept clean architecture version (`services/ProjectService/`)```json

   - Updated all references (launch.json, docker-compose){

   - Recreated project solution file  "id": "550e8400-e29b-41d4-a716-446655440000",

   - Eliminated ~500 lines of dead code  "email": "ali@example.com",

  "fullName": "Ali Raza",

2. **✅ Documentation Organization**  "createdAt": "2025-01-01T10:00:00Z"

   - Moved all markdown files to `md/` folder}

   - Created organization guidelines```

   - Centralized project documentation

   - Established naming conventions### User Service (`/api/users`)



3. **✅ Features Delivered (Previous Sessions)****Direct**: `http://localhost:5005`  

   - Feature C: Thumbnail Generation (14 files)**Via Gateway**: `http://localhost:5500/api/users`

   - Feature D: SEO Settings (9 files)

   - Feature E: Theme Settings (11 files)| Endpoint | Method | Purpose | Auth |

   - Feature F: Project Export - 4 formats (11 files)|----------|--------|---------|------|

   - Total: 2,100+ LOC across 4 features| `/profile` | GET | Get user profile | ✅ JWT |

| `/profile` | PUT | Update user profile | ✅ JWT |

4. **✅ Docker & Orchestration Setup**| `/settings` | GET | Get user settings | ✅ JWT |

   - Complete docker-compose.debug.yml (1,000+ lines, 20 services)| `/settings` | PUT | Update settings | ✅ JWT |

   - Complete docker-compose.prod.yml (850+ lines, replicas + limits)| `/health` | GET | Health check | ❌ |

   - Created docker-compose-manager.sh (400+ lines, 8 commands)

   - Created 3 Dockerfiles (event-bus, cache, frontend)### Billing Service (`/api/billing`)

   - Comprehensive Docker documentation (900+ lines)

**Direct**: `http://localhost:5177`  

---**Via Gateway**: `http://localhost:5500/api/billing`



## 🎯 Project Milestones| Endpoint | Method | Purpose | Auth |

|----------|--------|---------|------|

| Milestone | Completion | Status || `/plans` | GET | List subscription plans | ❌ |

|-----------|-----------|--------|| `/subscriptions` | GET | Get user subscription | ✅ JWT |

| **Core Architecture** | 100% | ✅ Complete || `/subscribe` | POST | Create subscription | ✅ JWT |

| **12 Microservices** | 100% | ✅ Complete || `/invoices` | GET | List invoices | ✅ JWT |

| **4 Key Features** (C, D, E, F) | 100% | ✅ Complete || `/health` | GET | Health check | ❌ |

| **Docker Orchestration** | 100% | ✅ Complete |

| **API Gateway** | 100% | ✅ Complete |### Image Service (`/api/images`)

| **Frontend** | 100% | ✅ Complete |

| **Documentation** | 100% | ✅ Complete |**Direct**: `http://localhost:5007`  

| **Production Readiness** | 95% | ⏳ In Progress |**Via Gateway**: `http://localhost:5500/api/images`



---| Endpoint | Method | Purpose | Auth |

|----------|--------|---------|------|

## 🚀 Getting Started (5-Minute Setup)| `/` | POST | Upload image | ✅ JWT |

| `/{id}` | GET | Get image | ✅ JWT |

```bash| `/{id}` | DELETE | Delete image | ✅ JWT |

# 1. Clone/open project| `/generate` | POST | Generate image via DALL·E | ✅ JWT |

cd /Users/alirazatahir/Desktop/Ali-Library/Project/Self/TechBirdsFly| `/health` | GET | Health check | ❌ |



# 2. Build Docker images### EventBus Service (`/api/events`)

./docker-compose-manager.sh build

**Direct**: `http://localhost:5030`  

# 3. Start all services**Via Gateway**: `http://localhost:5500/api/events`

./docker-compose-manager.sh up

| Endpoint | Method | Purpose | Auth |

# 4. Wait 60-90 seconds for health checks ⏳|----------|--------|---------|------|

| `/` | GET | Get recent events | ✅ JWT |

# 5. Open browser| `/subscribe` | POST | Subscribe to event type | ✅ JWT |

# Frontend:    http://localhost:3000| `/health` | GET | Health check | ❌ |

# API Gateway: http://localhost:9000

# Logs:        http://localhost:5341### Admin Service (`/api/admin`)

# Traces:      http://localhost:16686

**Direct**: `http://localhost:5006`  

# ✅ You're ready to use TechBirdsFly!**Via Gateway**: `http://localhost:5500/api/admin`

```

| Endpoint | Method | Purpose | Auth |

---|----------|--------|---------|------|

| `/users` | GET | List all users | ✅ Admin |

## 📖 API Endpoints Reference| `/users/{id}` | DELETE | Delete user | ✅ Admin |

| `/stats` | GET | System statistics | ✅ Admin |

### Gateway Routes (http://localhost:9000)| `/logs` | GET | System logs | ✅ Admin |

| `/health` | GET | Health check | ❌ |

```

/api/auth/**          → Auth Service (5001)## 🔐 Authentication

/api/users/**         → User Service (5008)

/api/billing/**       → Billing Service (5002)- **JWT Tokens**: 60-minute access + refresh tokens

/api/generator/**     → Generator Service (5003)- **Claims**: `sub` (userId), `email`, `name`

/api/export/**        → Export Service (5004)- **Header**: `Authorization: Bearer <token>`

/api/images/**        → Image Service (5007)- **Validation**: Gateway validates all protected routes

/api/admin/**         → Admin Service (5006)

/api/projects/**      → Project Service (5009)## 📊 Tech Stack

/api/events/**        → EventBus Service (5020)

/api/cache/**         → Cache Service (5021)### Backend Services

/api/media/**         → Media Service (5022)| Technology | Version | Purpose |

```|------------|---------|---------|

| ASP.NET Core | 8.0 | Web API framework |

### Health Checks| Entity Framework Core | 8.0 | ORM for database access |

| AutoMapper | 13.0 | Object mapping |

```bash| MediatR | 12.0 | CQRS & request handling |

# Gateway health| Serilog | 3.0 | Structured logging |

curl http://localhost:9000/health

### Databases

# Individual services (example: Auth)| Technology | Version | Purpose |

curl http://localhost:5001/health|------------|---------|---------|

| PostgreSQL | 12+ | All microservices (Auth, User, Billing, EventBus, Admin, Generator) |

# All services have /health endpoint| MongoDB | 5+ | Image Service storage |

```| Redis | 6+ | Distributed caching |



---### Message Queue & Events

| Technology | Version | Purpose |

## 🔐 Authentication|------------|---------|---------|

| Kafka | 3.0 | Event streaming |

- **JWT Tokens** issued by Auth Service| Zookeeper | 3.0 | Kafka coordination |

- **60-minute** access token lifetime

- **Refresh tokens** for renewal### Frontend

- **Gateway validation** on protected routes| Technology | Version | Purpose |

- **Header format**: `Authorization: Bearer <token>`|------------|---------|---------|

| React | 18.0 | UI library |

---| TypeScript | 5.0 | Type safety |

| Next.js | 14.0 | Framework & routing |

## 💾 Database Connections| Tailwind CSS | 3.4 | Styling |

| shadcn/ui | Latest | Component library |

### PostgreSQL (Primary)| TanStack Query | 5.0 | Data fetching |

```

Host: localhost (or docker service name)### API Gateway & Reverse Proxy

Port: 5433 (Docker) / 5432 (Local)| Technology | Version | Purpose |

Databases: techbirdsfly_auth, techbirdsfly_user, techbirdsfly_projects, etc.|------------|---------|---------|

User: postgres| YARP | 2.0 | API Gateway (Yet Another Reverse Proxy) |

Password: postgres123

```### Observability

| Technology | Version | Purpose |

### MongoDB|------------|---------|---------|

```| Seq | Latest | Centralized logging |

Connection: mongodb://localhost:27017| Jaeger | Latest | Distributed tracing |

Database: techbirdsfly_media| Prometheus | Latest | Metrics (planned) |

```| Grafana | Latest | Visualization (planned) |



### Redis### Deployment & DevOps

```| Technology | Version | Purpose |

Connection: localhost:6379|------------|---------|---------|

```| Docker | Latest | Containerization |

| Kubernetes | 1.24+ | Orchestration (planned) |

---| Azure Container Registry | - | Image registry |

| Azure App Service | - | Hosting (planned) |

## 🎨 Tech Stack

## 🎨 Development Workflow

### Backend

- **.NET 8** - Web API framework### Common Tasks

- **Entity Framework Core** - ORM

- **PostgreSQL** - Relational DB#### Adding a New API Endpoint

- **MongoDB** - Document DB

- **Redis** - Cache layer1. Create controller action in service (e.g., `UserController.cs`)

- **Kafka** - Message queue```csharp

- **MediatR** - CQRS pattern[HttpGet("{id}")]

- **Serilog** - Structured loggingpublic async Task<IActionResult> GetUser(Guid id)

{

### Frontend    var user = await _userService.GetUserAsync(id);

- **Next.js 18** - React framework    return Ok(user);

- **TypeScript** - Type safety}

- **Tailwind CSS** - Styling```

- **shadcn/ui** - Component library

- **TanStack Query** - Data fetching2. Add service method in `Services/` folder

```csharp

### Infrastructure & Observabilitypublic async Task<UserDto> GetUserAsync(Guid id)

- **Docker** - Containerization{

- **Docker Compose** - Orchestration    var user = await _context.Users.FindAsync(id);

- **Seq** - Centralized logging    return _mapper.Map<UserDto>(user);

- **Jaeger** - Distributed tracing}

- **YARP** - API Gateway```



---3. Update Gateway routes (if new service route needed)

   - Edit `/gateway/yarp-gateway/src/appsettings.json`

## 📞 Support   - Add route + cluster



- 📚 **Documentation**: Check `/md/` folder4. Test via Gateway

- 🐛 **Issues**: Check logs with docker-compose-manager.sh```bash

- 💬 **Questions**: Review relevant documentation filescurl http://localhost:5500/api/users/{id} \

- 🔍 **Debugging**: Use VS Code debug configurations  -H "Authorization: Bearer <TOKEN>"

```

---

#### Running Database Migrations

## 📊 Current Status Dashboard

```bash

```# Auth Service

✅ Architecture:           Completecd services/auth-service/AuthService

✅ 12 Microservices:       Complete  dotnet ef database update

✅ Docker Orchestration:   Complete

✅ API Gateway:            Complete# EventBus Service (uses PostgreSQL)

✅ Frontend:               Completecd services/eventbus-service/EventBusService

✅ Database Setup:         Completedotnet ef database update

✅ Documentation:          Complete

✅ Debug Configurations:   Complete# Other services

✅ Project Consolidation:  Completedotnet ef database update

✅ Code Organization:      Complete```

⏳ Production Deployment:  Ready

⏳ Kubernetes Deployment:  Planned#### Adding a New Database Model

```

1. Create model in `Models/`

---```csharp

public class UserProfile

## 🎉 Key Achievements{

    public Guid Id { get; set; }

- **4 Features** delivered with 2,100+ LOC    public string Bio { get; set; }

- **12 Microservices** fully operational    public DateTime CreatedAt { get; set; }

- **20 Services** orchestrated (12 micro + 8 infrastructure)}

- **8 Docker commands** via CLI manager```

- **100% Documentation** of setup and usage

- **Project consolidation** complete (eliminated duplicates)2. Add to DbContext

- **Clean Architecture** throughout```csharp

- **Production Ready** infrastructurepublic DbSet<UserProfile> UserProfiles { get; set; }

```

---

3. Create migration

**TechBirdsFly — Built to scale. Ready to deploy. 🚀**```bash

dotnet ef migrations add AddUserProfile

*Last Updated: November 27, 2025*  dotnet ef database update

*Status: Phase 2 Complete - Docker & Orchestration ✅*  ```

*Next: Production Deployment & Kubernetes Integration*

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
