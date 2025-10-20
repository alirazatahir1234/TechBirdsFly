# Project Structure Overview

Complete reorganized microservice architecture with production-ready layout.

## Directory Tree

```
TechBirdsFly/
├─ .github/
│  └─ copilot-instructions.md
│
├─ docs/
│  ├─ architecture.md
│  ├─ architecture_mermaid.md
│  └─ README.md
│
├─ services/                              # All microservices
│  ├─ auth-service/
│  │  ├─ src/                             # ✅ ACTIVE
│  │  │  ├─ Program.cs
│  │  │  ├─ AuthService.csproj
│  │  │  ├─ Models/
│  │  │  ├─ Services/
│  │  │  ├─ Controllers/
│  │  │  ├─ Data/
│  │  │  └─ Migrations/
│  │  ├─ Dockerfile
│  │  └─ README.md
│  │
│  ├─ user-service/
│  │  ├─ src/                             # 🟡 PHASE 2
│  │  │  └─ .gitkeep
│  │  ├─ Dockerfile
│  │  └─ README.md
│  │
│  ├─ generator-service/
│  │  ├─ src/                             # ✅ ACTIVE
│  │  │  ├─ Program.cs
│  │  │  ├─ GeneratorService.csproj
│  │  │  ├─ Models/
│  │  │  ├─ Services/
│  │  │  ├─ Controllers/
│  │  │  ├─ Data/
│  │  │  └─ Migrations/
│  │  ├─ Dockerfile
│  │  └─ README.md
│  │
│  ├─ image-service/
│  │  ├─ src/                             # 🟡 PHASE 2
│  │  │  └─ .gitkeep
│  │  ├─ Dockerfile
│  │  └─ README.md
│  │
│  ├─ billing-service/
│  │  ├─ src/                             # 🟡 PHASE 2
│  │  │  └─ .gitkeep
│  │  ├─ Dockerfile
│  │  └─ README.md
│  │
│  ├─ admin-service/
│  │  ├─ src/                             # 🟡 PHASE 2
│  │  │  └─ .gitkeep
│  │  ├─ Dockerfile
│  │  └─ README.md
│  │
│  └─ README.md                           # Services overview & registry
│
├─ gateway/
│  ├─ yarp-gateway/
│  │  ├─ src/                             # 🟡 PHASE 2
│  │  │  └─ .gitkeep
│  │  └─ Dockerfile
│  └─ README.md                           # YARP configuration
│
├─ infra/
│  ├─ docker-compose.yml                  # Local dev environment
│  └─ k8s/                                # Kubernetes manifests
│     ├─ namespace.yaml
│     ├─ configmap.yaml
│     ├─ secrets.yaml
│     ├─ ingress.yaml
│     ├─ services/
│     │  ├─ auth-deployment.yaml
│     │  └─ generator-deployment.yaml
│     └─ README.md
│
├─ web-frontend/
│  └─ react-app/
│     ├─ src/
│     ├─ public/
│     ├─ tailwind.config.js
│     ├─ tsconfig.json
│     ├─ package.json
│     └─ README.md
│
├─ frontend/  (legacy - being replaced by web-frontend)
│  └─ techbirdsfly-frontend/
│
├─ backend/  (legacy - being replaced by services/)
│  └─ TechBirdsFly.Api/
│
├─ TechBirdsFly.sln                       # Solution file
├─ README.md
├─ QUICK_START.md
└─ COMPLETION_SUMMARY.md
```

## What Was Reorganized

### ✅ Before & After

**Before:**
```
/services/auth-service/AuthService/     (code directly here)
/services/generator-service/GeneratorService/
/frontend/techbirdsfly-frontend/
```

**After:**
```
/services/auth-service/src/              (organized under src/)
/services/generator-service/src/
/services/[user|image|billing|admin]-service/src/
/web-frontend/react-app/                 (standardized naming)
/gateway/yarp-gateway/src/               (new gateway service)
/infra/k8s/                              (new K8s support)
```

## Service Registry (Ports)

| Service | Port | Status | Code Location |
|---------|------|--------|---------------|
| Auth | 5001 | ✅ Active | `/services/auth-service/src/` |
| User | 5002 | 🟡 Phase 2 | `/services/user-service/src/` |
| Generator | 5003 | ✅ Active | `/services/generator-service/src/` |
| Image | 5004 | 🟡 Phase 2 | `/services/image-service/src/` |
| Billing | 5005 | 🟡 Phase 2 | `/services/billing-service/src/` |
| Admin | 5006 | 🟡 Phase 2 | `/services/admin-service/src/` |
| Gateway | 5000 | 🟡 Phase 2 | `/gateway/yarp-gateway/src/` |
| Frontend | 3000 | ✅ Active | `/web-frontend/react-app/` |

## Project Structure Principles

### 1. **Microservices Pattern**
- Each service is independent
- Own database per service
- Separate `src/` directory for code
- Dockerfile at service root for easy building

### 2. **Standardized Layout**
```
service-name/
├─ src/                 (all code)
├─ Dockerfile          (build instructions)
└─ README.md           (service documentation)
```

### 3. **Infrastructure as Code**
```
infra/
├─ docker-compose.yml  (local development)
└─ k8s/                (production deployment)
   ├─ namespace.yaml
   ├─ configmap.yaml
   ├─ secrets.yaml
   ├─ ingress.yaml
   └─ services/
```

### 4. **Documentation Hierarchy**
```
/docs/              - Architecture & system design
/[service]/README.md - Service-specific docs
/infra/k8s/README.md - K8s deployment docs
/QUICK_START.md     - Quick start guide
/README.md          - Main project documentation
```

## Running Services

### From src/ directories

```bash
# Terminal 1: Auth Service
cd services/auth-service/src && dotnet run --urls http://localhost:5001

# Terminal 2: Generator Service
cd services/generator-service/src && dotnet run --urls http://localhost:5003

# Terminal 3: Frontend
cd web-frontend/react-app && npm start
```

### From Docker

```bash
cd infra && docker-compose up
```

### From Kubernetes

```bash
cd infra/k8s
kubectl create namespace techbirdsfly
kubectl apply -f namespace.yaml
kubectl apply -f configmap.yaml
kubectl apply -f secrets.yaml
kubectl apply -f services/
kubectl apply -f ingress.yaml
```

## File Changes Made

### Reorganized (Moved)
- ✅ `services/auth-service/AuthService/` → `services/auth-service/src/`
- ✅ `services/generator-service/GeneratorService/` → `services/generator-service/src/`

### Created (New Services - Phase 2)
- ✅ `services/user-service/`
- ✅ `services/image-service/`
- ✅ `services/billing-service/`
- ✅ `services/admin-service/`

### Created (Infrastructure)
- ✅ `gateway/yarp-gateway/`
- ✅ `infra/k8s/` with manifests
- ✅ `infra/k8s/services/` with deployments

### Created (Documentation)
- ✅ `/services/README.md` - Services overview
- ✅ Each service `README.md` with API endpoints
- ✅ `/gateway/README.md` - YARP configuration
- ✅ `/infra/k8s/README.md` - Kubernetes docs
- ✅ Individual K8s manifest files

## Next Steps

### Phase 2 Implementation Order (Recommended)

1. **User Service** (5002)
   - Profile management
   - Preferences & quotas
   - Add to K8s manifests

2. **Image Service** (5004)
   - DALL·E integration
   - Blob storage
   - Add to K8s manifests

3. **Billing Service** (5005)
   - Usage tracking
   - Stripe integration
   - Add to K8s manifests

4. **Admin Service** (5006)
   - Template management
   - Audit logging
   - Add to K8s manifests

5. **API Gateway** (5000)
   - Route all traffic
   - JWT validation
   - Rate limiting

### For Each Service, Follow This Pattern

```bash
# 1. Create project
cd services/[service-name]/src
dotnet new webapi -name [ServiceName]Service

# 2. Add packages
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package System.IdentityModel.Tokens.JsonWebTokenHandler

# 3. Copy from template (use auth-service/src as template)
# - Copy Program.cs
# - Create Models/
# - Create Services/
# - Create Controllers/
# - Create Data/

# 4. Create migrations
dotnet ef migrations add InitialCreate

# 5. Add K8s manifests
# - Copy auth-deployment.yaml as template
# - Customize for new service
# - Add to infra/k8s/services/

# 6. Test locally
dotnet run --urls http://localhost:500X

# 7. Build Docker image
docker build -t myregistry/[service-name]:latest .
```

## Configuration Files

### Development
```
appsettings.Development.json   (in each service src/)
docker-compose.yml             (infra/)
```

### Production
```
infra/k8s/configmap.yaml       (environment variables)
infra/k8s/secrets.yaml         (API keys, connection strings)
```

## Deployment Checklists

### Docker Compose (Local)
- [ ] Update `docker-compose.yml` with new service
- [ ] Test with `docker-compose up`

### Kubernetes (Production)
- [ ] Create service deployment file in `infra/k8s/services/`
- [ ] Update `infra/k8s/configmap.yaml`
- [ ] Update `infra/k8s/secrets.yaml`
- [ ] Update `infra/k8s/ingress.yaml`
- [ ] Apply manifests

## Current Status

✅ **Reorganization Complete**
- All existing services moved to `/src/` structure
- 4 new services scaffolded with templates
- Infrastructure files created (docker-compose, K8s)
- Documentation updated
- Ready for Phase 2 implementation

🟢 **Ready for**: User Service development (Phase 2)

## Related Documentation

- [Services Overview](/services/README.md)
- [Architecture Spec](/docs/architecture.md)
- [Quick Start](/QUICK_START.md)
- [Completion Summary](/COMPLETION_SUMMARY.md)

---

**Updated**: October 16, 2025  
**Structure Version**: 2.0 - Production Ready
