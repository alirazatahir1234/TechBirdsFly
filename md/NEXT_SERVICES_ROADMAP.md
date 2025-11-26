# 🛣️ NEXT SERVICES ROADMAP

## ✅ COMPLETED SERVICES

### 1. ✅ Auth Service (Complete)
- JWT token generation & validation
- User registration & login
- Password reset flow
- Email confirmation
- Token refresh mechanism
- Status: **Production Ready**

### 2. ✅ Export Service (Complete)
- Code export to HTML/React/Next.js
- ZIP packaging
- Local & Azure storage
- 5 REST endpoints
- Status: **Production Ready**

### 3. ✅ Generator Service (Complete)
- Ollama AI integration
- Multi-format output (HTML/React/Next.js)
- MinIO object storage
- Kafka event publishing
- PostgreSQL persistence
- Status: **Production Ready**

---

## 🚀 RECOMMENDED NEXT SERVICES (Priority Order)

### TIER 1: HIGH PRIORITY (Enables Core Functionality)

#### 1️⃣ Project Service ⭐ RECOMMENDED NEXT
**Purpose**: Centralized project metadata management  
**Dependencies**: Auth Service (JWT validation)

**Key Features**:
- Project CRUD operations
- Project settings & configuration
- Team & collaboration
- Version history tracking
- Webhook configuration

**Architecture**:
```
/services/ProjectService
  /src
    /Domain (Project, Team, ProjectSettings entities)
    /Application (Commands: CreateProject, UpdateSettings, etc.)
    /Infrastructure (PostgreSQL, caching)
    /Api (REST endpoints for CRUD)
```

**Estimated Implementation**: 4-6 hours

**API Endpoints**:
```
POST   /api/projects              # Create project
GET    /api/projects/{id}         # Get project
PUT    /api/projects/{id}         # Update project
DELETE /api/projects/{id}         # Delete project
GET    /api/projects              # List user's projects
GET    /api/projects/{id}/versions # Project versions
```

**Why First?**
- ✅ Needed by Export Service to track project versions
- ✅ Centralized project state management
- ✅ Foundation for other services

---

#### 2️⃣ Template Service
**Purpose**: Pre-built templates & themes  
**Dependencies**: Generator Service, Project Service

**Key Features**:
- Template library management
- Custom template upload
- Template versioning
- Preview generation
- Category management

**Estimated Implementation**: 3-5 hours

**API Endpoints**:
```
GET    /api/templates              # List templates
GET    /api/templates/{id}         # Get template
POST   /api/templates              # Create template
PUT    /api/templates/{id}         # Update template
DELETE /api/templates/{id}         # Delete template
GET    /api/templates/{id}/preview # Get preview
```

---

#### 3️⃣ Media Service ⭐ RECOMMENDED SECOND
**Purpose**: Image generation & asset management  
**Dependencies**: Ollama, MinIO, Project Service

**Key Features**:
- DALL·E image generation
- Image optimization & resizing
- Asset library management
- Image search & tagging
- CDN integration

**Architecture**:
```
/services/MediaService
  /src
    /Domain (Image, Asset, Metadata entities)
    /Application (Commands: GenerateImage, OptimizeImage)
    /Infrastructure (OpenAI/Ollama, ImageMagick, MinIO)
    /Api (Image endpoints + websocket for progress)
```

**Estimated Implementation**: 5-7 hours

**API Endpoints**:
```
POST   /api/images/generate       # Generate image from prompt
GET    /api/images/{id}           # Get image
DELETE /api/images/{id}           # Delete image
GET    /api/images                # List project images
POST   /api/images/{id}/resize    # Resize image
GET    /api/images/search         # Search images
```

---

### TIER 2: MEDIUM PRIORITY (Enhanced Features)

#### 4️⃣ Analytics Service
**Purpose**: Usage tracking & insights  
**Dependencies**: All services (event subscribers)

**Key Features**:
- Event tracking (generation, exports, etc.)
- Analytics dashboard
- User engagement metrics
- Performance monitoring
- Report generation

**API Endpoints**:
```
GET    /api/analytics/events      # Get events
GET    /api/analytics/dashboard   # Dashboard metrics
GET    /api/analytics/reports     # Generate report
POST   /api/analytics/events      # Record event
```

---

#### 5️⃣ Notification Service
**Purpose**: Email, SMS, push notifications  
**Dependencies**: Auth Service, Email provider (SendGrid)

**Key Features**:
- Email notifications
- SMS alerts
- Push notifications
- Notification templates
- User preferences

**Estimated Implementation**: 3-4 hours

---

#### 6️⃣ Billing & Subscription Service
**Purpose**: Payment processing & quotas  
**Dependencies**: Project Service, Analytics Service, Stripe

**Key Features**:
- Subscription management
- Payment processing
- Usage quotas & limits
- Invoice generation
- Subscription analytics

**Estimated Implementation**: 6-8 hours

---

### TIER 3: OPTIONAL FEATURES (Advanced)

#### 7️⃣ Collaboration Service
**Purpose**: Real-time collaboration features  
**Dependencies**: Project Service, WebSocket

**Key Features**:
- Real-time editing
- Comments & annotations
- Version control
- Conflict resolution
- Activity feeds

---

#### 8️⃣ Import/Migration Service
**Purpose**: Import from other platforms  
**Dependencies**: Storage services

**Key Features**:
- Import from Figma
- Import from other builders
- Format conversion
- Data validation
- Migration tracking

---

## 📊 DEPENDENCY GRAPH

```
┌─────────────────────────────────────────────────────┐
│                    Auth Service ✅                  │
│           (Foundation - JWT, User Auth)              │
└──────────────────────┬──────────────────────────────┘
                       │
        ┌──────────────┼──────────────┐
        ▼              ▼              ▼
   ┌──────────┐ ┌──────────┐ ┌──────────────┐
   │ Project  │ │Generator │ │Export Service│
   │Service⭐ │ │Service✅ │ │     ✅       │
   └────┬─────┘ └──────┬───┘ └──────┬───────┘
        │              │             │
   ┌────┴──────────────┴─────────────┘
   │
   ▼
┌──────────────┐
│Media Service │
│  ⭐ NEXT 2   │
└──────────────┘
   │
   ├─── Analytics Service
   ├─── Notification Service
   ├─── Billing Service
   └─── Collaboration Service
```

---

## 🎯 IMPLEMENTATION STRATEGY

### Recommended Implementation Order

1. **Week 1**: Project Service (enables project tracking)
2. **Week 1**: Media Service (adds image generation)
3. **Week 2**: Analytics Service (insights & monitoring)
4. **Week 2**: Notification Service (user engagement)
5. **Week 3**: Billing Service (monetization)
6. **Week 4+**: Optional services as needed

### Per-Service Pattern (Consistent)

```
1. Create folder: /services/{ServiceName}
2. Create .csproj files:
   - {Service}.Domain.csproj
   - {Service}.Application.csproj
   - {Service}.Infrastructure.csproj
   - {Service}.Api.csproj
   - {Service}.sln

3. Implement layers:
   - Domain: Entities, Value Objects
   - Application: DTOs, Commands/Queries, Handlers
   - Infrastructure: Repositories, External Services
   - API: Controllers, Program.cs, Middleware

4. Add to stack:
   - Update docker-compose.yml
   - Configure YARP routes
   - Add Kafka topics
   - Register in frontend

5. Documentation:
   - README.md
   - QUICK_START.md
   - API.md
   - INTEGRATION.md
```

---

## 🔌 GATEWAY CONFIGURATION TEMPLATE

```json
{
  "RouteId": "service-name",
  "ClusterId": "serviceNameCluster",
  "Match": {
    "Path": "/service-name/{**catch-all}"
  },
  "Transforms": [
    {
      "PathPattern": "/api/{**catch-all}"
    }
  ]
}
```

---

## 📝 FRONTEND INTEGRATION TEMPLATE

```typescript
// lib/store/{service}Store.ts
import { create } from 'zustand';
import { persist } from 'zustand/middleware';

export const use{Service}Store = create(
  persist((set) => ({
    // State
    items: [],
    loading: false,
    error: null,
    
    // Actions
    fetchItems: async () => {
      set({ loading: true });
      try {
        const res = await fetch('/api/{service}/api/items');
        const data = await res.json();
        set({ items: data, error: null });
      } catch (err) {
        set({ error: (err as Error).message });
      } finally {
        set({ loading: false });
      }
    }
  }), {
    name: '{service}-store'
  })
);
```

---

## 💡 QUICK REFERENCE: WHAT TO BUILD FIRST

### If Focus is User Experience
**Order**: Project → Media → Analytics → Notifications

### If Focus is Monetization  
**Order**: Project → Billing → Analytics → Notifications

### If Focus is Collaboration
**Order**: Project → Collaboration → Notifications → Analytics

### If Focus is Content
**Order**: Project → Media → Templates → Analytics

---

## 📦 SHARED INFRASTRUCTURE

### Services Using Same Stack

```
All services share:
✅ .NET 8.0
✅ PostgreSQL
✅ Kafka (events)
✅ Serilog (logging)
✅ OpenTelemetry (tracing)
✅ MediatR (CQRS)
✅ Entity Framework Core
✅ JWT authentication
```

### Shared NuGet Packages

```xml
<ItemGroup>
  <PackageReference Include="MediatR" Version="12.0.0" />
  <PackageReference Include="Serilog" Version="3.0.0" />
  <PackageReference Include="OpenTelemetry" Version="1.6.0" />
  <PackageReference Include="EntityFrameworkCore" Version="8.0.0" />
  <PackageReference Include="Kafka.Client" Version="latest" />
</ItemGroup>
```

---

## 🚀 COMMAND REFERENCE

### Generate Project Service

```bash
# Create solution
dotnet new globaljson --sdk-version 8.0.0 --roll-forward latestMinor

# Create projects
dotnet new classlib -n ProjectService.Domain
dotnet new classlib -n ProjectService.Application
dotnet new classlib -n ProjectService.Infrastructure
dotnet new webapi -n ProjectService.Api

# Create solution
dotnet new sln -n ProjectService

# Add projects to solution
dotnet sln ProjectService.sln add ProjectService.Domain/ProjectService.Domain.csproj
dotnet sln ProjectService.sln add ProjectService.Application/ProjectService.Application.csproj
dotnet sln ProjectService.sln add ProjectService.Infrastructure/ProjectService.Infrastructure.csproj
dotnet sln ProjectService.sln add ProjectService.Api/ProjectService.Api.csproj

# Build
dotnet build
```

---

## 📈 ESTIMATED TIMELINE

| Service | Complexity | Hours | After |
|---------|-----------|-------|-------|
| Project Service | Medium | 4-6 | Auth ✅ |
| Media Service | Medium-High | 5-7 | Project |
| Analytics Service | Medium | 3-4 | Generator ✅ |
| Notification Service | Low | 3-4 | Project |
| Billing Service | High | 6-8 | Analytics |
| Collaboration | High | 8-10 | Project |
| **Total** | - | **30-40** | - |

---

## ✨ RECOMMENDATION

### Start with Project Service
**Why?**
1. ✅ Dependencies on it from multiple services
2. ✅ Enables proper project tracking
3. ✅ Straightforward implementation (CRUD + settings)
4. ✅ Unblocks Media Service
5. ✅ Sets pattern for other services

**Then Media Service**
- Completes the core feature set
- Highly valuable to users
- Showcases AI capabilities

**Then Expand Based on Goals**
- Analytics if you need insights
- Billing if monetizing
- Notifications for engagement

---

## 🔗 INTEGRATION CHECKLIST (Per Service)

```
□ Create service folder & projects
□ Implement all 4 layers (Domain, Application, Infrastructure, API)
□ Create Database & migrations
□ Add Dockerfile & docker-compose config
□ Configure YARP routes in Gateway
□ Create Zustand store in Frontend
□ Add frontend components
□ Update documentation
□ Test end-to-end
□ Load test with k6
□ Deploy to staging
□ Deploy to production
```

---

## 📚 RESOURCES

- **Domain-Driven Design**: https://martinfowler.com/bliki/DomainDrivenDesign.html
- **CQRS Pattern**: https://martinfowler.com/bliki/CQRS.html
- **Microservices**: https://martinfowler.com/articles/microservices.html
- **.NET Best Practices**: https://docs.microsoft.com/en-us/dotnet/fundamentals/
- **Entity Framework Core**: https://docs.microsoft.com/en-us/ef/core/
- **MediatR**: https://github.com/jbogard/MediatR
- **Kafka**: https://kafka.apache.org/
- **PostgreSQL**: https://www.postgresql.org/

---

**Last Updated**: November 25, 2025  
**Next Service**: Project Service ⭐  
**Estimated Start**: Immediately
