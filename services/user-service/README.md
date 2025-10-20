# User Service 👥

**User Profile and Subscription Management Microservice for TechBirdsFly.AI**

---

## 🎯 Quick Overview

The **User Service** manages user profiles, preferences, subscriptions, and connections with authentication and admin services. It's the **identity and data brain** for authenticated users in the TechBirdsFly.AI platform.

### What It Does

✅ **User Profile Management** — Store/retrieve user information
✅ **Subscription Handling** — Manage plans (free, starter, pro, enterprise)
✅ **Preference Storage** — User settings (theme, language, notifications)
✅ **Usage Tracking** — Monitor resource consumption per user
✅ **Auth Integration** — JWT-based secure data access
✅ **Service Communication** — REST APIs for other microservices

---

## 🚀 Getting Started

### Prerequisites

- .NET 8.0 SDK or later
- SQLite (development) / PostgreSQL (production)
- Docker (optional, for containerization)

### Local Setup

```bash
# Clone/navigate to project
cd services/user-service/src/UserService

# Restore dependencies
dotnet restore

# Build project
dotnet build

# Run migrations
dotnet ef database update

# Start service
dotnet run
```

**Service runs on:** `https://localhost:5008` (or `http://localhost:5007` without HTTPS)

### Verify Health

```bash
curl http://localhost:5008/api/users/health
```

Expected response:
```json
{
  "status": "healthy",
  "service": "user-service",
  "timestamp": "2025-10-17T10:30:00Z",
  "version": "1.0.0"
}
```

---

## 📡 API Endpoints

### User Management

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/api/users/me` | Get current user | Bearer |
| GET | `/api/users/{id}` | Get user by ID | Admin |
| GET | `/api/users/email/{email}` | Get user by email | Public |
| GET | `/api/users` | List all users | Admin |
| PUT | `/api/users/{id}` | Update user profile | Bearer |
| DELETE | `/api/users/{id}` | Delete user account | Bearer |

### Subscription Management

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/api/users/{id}/subscription` | Get subscription | Bearer |
| POST | `/api/users/{id}/subscription/upgrade` | Upgrade plan | Bearer |
| POST | `/api/users/{id}/subscription/cancel` | Cancel subscription | Bearer |
| POST | `/api/users/{id}/usage` | Update usage stats | Service |
| POST | `/api/users/{id}/login` | Record login event | Public |

---

## 🗄️ Data Models

### User Entity
- **Id**: Unique identifier (GUID)
- **Email**: Unique email address
- **FirstName, LastName**: User's name
- **Status**: active, inactive, suspended, deleted
- **Role**: user, admin, moderator
- **Timestamps**: CreatedAt, UpdatedAt, LastLoginAt, DeletedAt
- **Stats**: LoginCount, ProjectCount, ImageGenerationCount

### UserSubscription Entity
- **PlanType**: free, starter, pro, enterprise
- **Status**: active, paused, cancelled, expired
- **Usage**: MonthlyImageGenerations, MonthlyStorageGb
- **Tracking**: UsedGenerations, UsedStorageGb
- **Dates**: StartDate, RenewalDate, EndDate

### Subscription Plans

| Plan | Cost | Generations | Storage | Target |
|------|------|-------------|---------|--------|
| Free | $0 | 10/mo | 1 GB | Hobbyists |
| Starter | $9.99 | 100/mo | 10 GB | Small teams |
| Pro | $29.99 | 500/mo | 50 GB | Growing business |
| Enterprise | $99.99 | 5,000/mo | 500 GB | Large orgs |

---

## 🔐 Authentication & Authorization

### JWT Claims
```json
{
  "sub": "user-id",
  "email": "user@example.com",
  "role": "user"
}
```

### Authorization Levels
- **Public**: No token (health checks, email lookup)
- **Bearer**: User token (own profile, own subscription)
- **Admin**: Admin token (all users, all data)
- **Service**: Service token (internal APIs, usage updates)

---

## 🔄 Integration Points

### With Auth Service
- Validates JWT tokens
- Extracts user ID and role from claims
- Supports email-based user lookup

### With Generator Service
- Receives usage updates (images, storage)
- Provides user preferences
- Checks plan limits

### With Admin Service
- Enables admin user management
- Provides statistics
- Allows subscription management

### With Billing Service
- Tracks subscription changes
- Reports usage for billing
- Manages plan upgrades/downgrades

---

## 📊 Database

### Connection Strings

**Development:**
```
Data Source=user.db
```

**Production:**
```
Server=postgres-server;Database=users;User Id=postgres;Password=SecurePassword
```

### Migrations

```bash
# Create migration
dotnet ef migrations add MigrationName

# Apply migrations
dotnet ef database update

# Rollback
dotnet ef database update PreviousMigration
```

---

## 🐳 Docker

### Build & Run

```bash
# Build image
docker build -t techbirdsfly/user-service:latest .

# Run container
docker run -d \
  -p 5008:5008 \
  -e ASPNETCORE_ENVIRONMENT=Development \
  techbirdsfly/user-service:latest
```

### Docker Compose

See project's `docker-compose.yml` for multi-service setup.

---

## ⚙️ Configuration

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=user.db"
  },
  "JwtSettings": {
    "SecretKey": "your-secret-key",
    "Issuer": "TechBirdsFly",
    "Audience": "TechBirdsFly-Users"
  },
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:3000",
      "http://localhost:3001"
    ]
  }
}
```

---

## 📚 Detailed Documentation

For comprehensive implementation details, architecture diagrams, and advanced topics, see:

**👉 [IMPLEMENTATION_GUIDE.md](./IMPLEMENTATION_GUIDE.md)**

---

## 📝 License

MIT License

---

**Status:** ✅ Production Ready  
**Version:** 1.0.0  
**Last Updated:** October 17, 2025
