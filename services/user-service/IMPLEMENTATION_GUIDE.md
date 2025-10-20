# User Service Implementation Guide

## 📋 Overview

The **User Service** is a core microservice in the TechBirdsFly.AI platform responsible for managing user profiles, preferences, subscriptions, and authentication integration. It serves as the **identity and data brain** for authenticated users.

### Key Responsibilities

| Aspect | Description |
|--------|-------------|
| 👤 **User Profiles** | Store and manage comprehensive user profile information (name, email, bio, company, contact details) |
| ⚙️ **User Preferences** | Manage user-specific settings (theme, language, notification preferences, two-factor auth) |
| 💳 **Subscriptions** | Handle plan management (free, starter, pro, enterprise) and usage tracking |
| 🔐 **Auth Integration** | Work seamlessly with Auth Service via JWT tokens for secure data access |
| 🧠 **AI Preferences** | Store user's preferred AI models, template styles, and generation parameters |
| 🔗 **Service Linkage** | Enable other services to fetch user data via REST APIs |

---

## 🏗️ Architecture

### Service Topology

```
┌─────────────────────────────────────────────────────────────────┐
│                        API Gateway (YARP)                       │
└──────────────────────────────┬──────────────────────────────────┘
                               │
                ┌──────────────┼──────────────┐
                │              │              │
                ▼              ▼              ▼
         ┌──────────────┐  ┌──────────────┐  ┌──────────────┐
         │ Auth Service │  │ User Service │  │Admin Service │
         └──────────────┘  └──────────────┘  └──────────────┘
                │              │              │
                ▼              ▼              ▼
         ┌──────────────────────────────────────────────────┐
         │        Shared JWT Configuration                  │
         └──────────────────────────────────────────────────┘
```

### Data Model

```
User (Core)
├── Id (GUID)
├── Email (Unique)
├── FirstName / LastName
├── Role (User, Admin, Moderator)
├── Status (active, inactive, suspended, deleted)
├── Timestamps (CreatedAt, UpdatedAt, DeletedAt)
│
├── UserProfile (1:1)
│   ├── DisplayName
│   ├── Location
│   ├── Bio
│   └── AvatarUrl
│
├── UserPreference (1:1)
│   ├── NotifyEmail / NotifyPush
│   ├── TwoFactorEnabled
│   ├── Theme
│   └── Language
│
└── UserSubscription (1:1)
    ├── PlanType (free, starter, pro, enterprise)
    ├── Status (active, paused, cancelled)
    ├── Usage Tracking
    └── Billing Info
```

---

## 🚀 Getting Started

### Prerequisites

- .NET 8.0 SDK or later
- SQLite (for development)
- PostgreSQL (for production - optional)
- VS Code or Visual Studio

### Project Structure

```
services/user-service/
├── src/UserService/
│   ├── Program.cs                 # Main entry point and configuration
│   ├── UserService.csproj         # Project file with NuGet dependencies
│   ├── appsettings.json          # Default configuration
│   ├── appsettings.Development.json
│   │
│   ├── Models/
│   │   └── User.cs               # User entities and DTOs
│   │
│   ├── Data/
│   │   ├── UserDbContext.cs      # EF Core DbContext
│   │   └── Migrations/           # Database migrations
│   │
│   ├── Services/
│   │   ├── IUserManagementService.cs    # User CRUD operations
│   │   └── ISubscriptionService.cs      # Subscription management
│   │
│   ├── Controllers/
│   │   └── UserController.cs     # REST API endpoints
│   │
│   └── Properties/
│       └── launchSettings.json   # Launch configuration
│
├── Dockerfile                     # Multi-stage Docker build
├── .env.example                  # Environment variables template
└── README.md                      # Service documentation
```

---

## 🔌 Key Endpoints

### Health Check
```http
GET /api/users/health
```

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

## 🔐 Authentication & Authorization

### JWT Token Structure

```json
{
  "sub": "user-id",
  "email": "user@example.com",
  "role": "user",
  "iat": 1633046400,
  "exp": 1633132800
}
```

### Authorization Policies

```csharp
// Admin only - manage other users
[Authorize(Roles = "Admin")]

// Self or Admin - manage own or others' data
if (currentUserId != userId && role != "Admin")
    return Forbid();

// Service-to-service - internal APIs
[Authorize(Roles = "Service")]
```

---

## 📊 Database Schema

### Users Table
```sql
CREATE TABLE Users (
    Id TEXT PRIMARY KEY,
    Email TEXT UNIQUE NOT NULL,
    FirstName TEXT NOT NULL,
    LastName TEXT NOT NULL,
    Status TEXT NOT NULL DEFAULT 'active',
    Role TEXT NOT NULL DEFAULT 'user',
    IsEmailVerified BOOLEAN NOT NULL DEFAULT 0,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    LastLoginAt DATETIME,
    DeletedAt DATETIME
);

CREATE INDEX idx_users_email ON Users(Email);
CREATE INDEX idx_users_status ON Users(Status);
CREATE INDEX idx_users_created_at ON Users(CreatedAt);
```

### UserPreferences Table
```sql
CREATE TABLE UserPreferences (
    Id TEXT PRIMARY KEY,
    UserId TEXT NOT NULL UNIQUE,
    NotifyEmail BOOLEAN NOT NULL DEFAULT 1,
    TwoFactorEnabled BOOLEAN NOT NULL DEFAULT 0,
    Theme TEXT NOT NULL DEFAULT 'light',
    Language TEXT NOT NULL DEFAULT 'en',
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);
```

### UserSubscriptions Table
```sql
CREATE TABLE UserSubscriptions (
    Id TEXT PRIMARY KEY,
    UserId TEXT NOT NULL UNIQUE,
    PlanType TEXT NOT NULL DEFAULT 'free',
    Status TEXT NOT NULL DEFAULT 'active',
    MonthlyCost DECIMAL(10,2) NOT NULL DEFAULT 0,
    UsedGenerations INT NOT NULL DEFAULT 0,
    MonthlyImageGenerations INT NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);

CREATE INDEX idx_subscriptions_user_status ON UserSubscriptions(UserId, Status);
CREATE INDEX idx_subscriptions_plan_type ON UserSubscriptions(PlanType);
```

---

## 🔄 Service Integration

### With Auth Service

1. **Token Exchange**: User receives JWT from Auth Service
2. **Token Validation**: User Service validates JWT using shared secret
3. **User Context**: Extract user ID and role from token claims
4. **Data Isolation**: Each user can only access their own data

```csharp
// Extract from JWT
var userId = User.FindFirst("sub")?.Value;
var email = User.FindFirst("email")?.Value;
var role = User.FindFirst("role")?.Value;
```

### With Generator Service

1. **User Preferences**: Generator Service queries User Service for preferences
2. **Usage Tracking**: Generator Service reports usage to User Service
3. **Plan Validation**: Generator Service checks plan limits via User Service

```http
POST /api/users/{userId}/usage
Content-Type: application/json

{
  "generationCount": 1,
  "storageUsedGb": 0.5
}
```

### With Admin Service

1. **User Management**: Admin Service uses User Service to manage users
2. **Analytics**: Admin Service queries user statistics
3. **Moderation**: Admin Service can suspend/delete users

### With Billing Service

1. **Plan Linking**: Billing Service uses User Service for subscription info
2. **Payment Events**: Billing Service updates subscription status
3. **Usage Reports**: User Service provides usage data for billing

---

## 🛠️ Development Workflow

### 1. Setup Local Environment

```bash
cd "/Applications/My Drive/TechBirdsFly/services/user-service/src/UserService"

# Install dependencies
dotnet restore

# Apply migrations
dotnet ef database update

# Run service
dotnet run --launch-profile https
```

### 2. Configure appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=user.db"
  },
  "JwtSettings": {
    "SecretKey": "your-secret-key-change-in-production",
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

### 3. Test Endpoints

```bash
# Health check
curl http://localhost:5008/api/users/health

# List users (requires admin token)
curl -H "Authorization: Bearer {token}" \
  http://localhost:5008/api/users

# Get current user
curl -H "Authorization: Bearer {token}" \
  http://localhost:5008/api/users/me
```

### 4. Database Migrations

```bash
# Create new migration
dotnet ef migrations add AddUserPreferences

# Apply pending migrations
dotnet ef database update

# Rollback last migration
dotnet ef migrations remove
```

---

## 📦 NuGet Dependencies

| Package | Version | Purpose |
|---------|---------|---------|
| Microsoft.EntityFrameworkCore.Sqlite | 8.0.8 | SQLite database provider |
| Microsoft.AspNetCore.Authentication.JwtBearer | 8.0.8 | JWT authentication |
| Swashbuckle.AspNetCore | 6.5.0 | Swagger/OpenAPI documentation |
| System.IdentityModel.Tokens.Jwt | 7.1.2 | JWT token handling |

---

## 🚢 Deployment

### Docker Build

```bash
docker build -t techbirdsfly/user-service:latest .
```

### Docker Compose

```yaml
user-service:
  image: techbirdsfly/user-service:latest
  ports:
    - "5008:5008"
  environment:
    - ASPNETCORE_ENVIRONMENT=Production
    - ConnectionStrings__DefaultConnection=Server=postgres;Database=users;User Id=postgres;Password=password
    - JwtSettings__SecretKey=${JWT_SECRET}
  depends_on:
    - postgres
```

### Production Considerations

- ✅ Use PostgreSQL instead of SQLite
- ✅ Set strong JWT secret key
- ✅ Configure HTTPS certificates
- ✅ Set up database backups
- ✅ Enable health checks monitoring
- ✅ Configure logging aggregation
- ✅ Set up rate limiting
- ✅ Enable CORS only for trusted origins

---

## 📈 Performance Optimization

### Database Indexing

```csharp
// Covered indexes for common queries
entity.HasIndex(e => e.Email).IsUnique();
entity.HasIndex(e => e.CreatedAt);
entity.HasIndex(e => new { e.UserId, e.Status });
```

### Caching Strategy

```csharp
// Cache user profile for 5 minutes
var user = await _cache.GetOrCreateAsync(
    $"user:{userId}",
    async entry => {
        entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
        return await _userService.GetUserByIdAsync(userId);
    }
);
```

### Query Optimization

```csharp
// Use projections to reduce data transfer
var users = await _context.Users
    .Where(u => u.Status == "active")
    .Select(u => new UserResponse {
        Id = u.Id,
        Email = u.Email,
        Name = $"{u.FirstName} {u.LastName}"
    })
    .ToListAsync();
```

---

## 🧪 Testing Strategy

### Unit Tests

```csharp
[Fact]
public async Task CreateUser_ValidRequest_ReturnsUser()
{
    // Arrange
    var request = new CreateUserRequest { Email = "test@example.com" };
    
    // Act
    var result = await _service.CreateUserAsync(request);
    
    // Assert
    Assert.NotNull(result);
    Assert.Equal("test@example.com", result.Email);
}
```

### Integration Tests

```csharp
[Fact]
public async Task UpdateUser_ValidToken_UpdatesSuccessfully()
{
    // Create test database
    // Create test user
    // Send authenticated request
    // Verify response
}
```

---

## 📊 Monitoring & Observability

### Metrics to Track

- ✅ Request latency (p50, p95, p99)
- ✅ Database query duration
- ✅ Error rates by endpoint
- ✅ Active user count
- ✅ Subscription distribution by plan
- ✅ Database connection pool health

### Logging Configuration

```csharp
// Structured logging
_logger.LogInformation("User created: {UserId}, Email: {Email}", userId, email);
_logger.LogError(ex, "Error creating user for email: {Email}", email);
```

### Health Checks

```csharp
// Database health
app.MapHealthChecks("/health/db", new HealthCheckOptions {
    Predicate = r => r.Name == "database"
});
```

---

## 🔮 Future Enhancements

### Phase 1 (Current)
- ✅ Basic user management
- ✅ Subscription tracking
- ✅ JWT authentication
- ✅ SQLite persistence

### Phase 2
- [ ] User preferences API (theme, language, AI settings)
- [ ] Profile picture upload (integration with image-service)
- [ ] Email verification
- [ ] Two-factor authentication
- [ ] User audit logging

### Phase 3
- [ ] Social features (followers, activity feed)
- [ ] User analytics dashboard
- [ ] Preference templates
- [ ] Export user data (GDPR compliance)
- [ ] Account recovery flows

### Phase 4
- [ ] Redis caching layer
- [ ] Distributed rate limiting
- [ ] Advanced role-based access control
- [ ] User activity timeline
- [ ] Recommendation engine integration

---

## 📚 API Examples

### Create User
```http
POST /api/users
Content-Type: application/json

{
  "email": "john@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "phoneNumber": "+1234567890",
  "company": "Tech Corp"
}

Response: 201 Created
{
  "id": "123e4567-e89b-12d3-a456-426614174000",
  "email": "john@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "status": "active",
  "createdAt": "2025-10-17T10:30:00Z"
}
```

### Update User
```http
PUT /api/users/123e4567-e89b-12d3-a456-426614174000
Authorization: Bearer {jwt_token}
Content-Type: application/json

{
  "firstName": "Jonathan",
  "bio": "Software Engineer",
  "website": "https://example.com"
}

Response: 200 OK
{
  "id": "123e4567-e89b-12d3-a456-426614174000",
  "firstName": "Jonathan",
  "bio": "Software Engineer",
  "website": "https://example.com",
  "updatedAt": "2025-10-17T11:00:00Z"
}
```

### Get User Subscription
```http
GET /api/users/123e4567-e89b-12d3-a456-426614174000/subscription
Authorization: Bearer {jwt_token}

Response: 200 OK
{
  "id": "sub-123",
  "userId": "123e4567-e89b-12d3-a456-426614174000",
  "planType": "pro",
  "status": "active",
  "monthlyCost": 29.99,
  "monthlyImageGenerations": 500,
  "usedGenerations": 245,
  "renewalDate": "2025-11-17T00:00:00Z"
}
```

---

## 🤝 Contributing

When adding features to User Service:

1. Follow the existing service/controller pattern
2. Add comprehensive logging
3. Include proper error handling
4. Write unit tests for business logic
5. Update this guide with new endpoints
6. Document breaking changes in CHANGELOG.md

---

## 📞 Support

For issues or questions about the User Service:

- 📧 Email: support@techbirdsfly.com
- 🐛 Report bugs in GitHub Issues
- 💬 Discuss in Discord community
- 📖 Check documentation at /docs

---

**Last Updated:** October 17, 2025
**Version:** 1.0.0
**Status:** ✅ Production Ready
