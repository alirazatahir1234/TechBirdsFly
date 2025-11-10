# 🔧 Infrastructure Layer Quick Reference

## Files Created (6 total, ~990 lines)

### 1. Persistence Layer
```
Infrastructure/Persistence/AdminDbContext.cs (200 lines)
- PostgreSQL DbContext with EF Core mapping
- Seeded 3 system roles (SuperAdmin, Admin, Moderator)
- Fluent API configuration for all entities
- Indexes and constraints
```

### 2. Repository Layer (3 files, 400 lines total)
```
Infrastructure/Repositories/AdminUserRepository.cs (130 lines)
- CRUD for AdminUser
- GetByStatus() for filtering
- Email normalization

Infrastructure/Repositories/RoleRepository.cs (110 lines)
- CRUD for Role
- System role protection
- Prevents modification/deletion of system roles

Infrastructure/Repositories/AuditLogRepository.cs (160 lines)
- CRUD for AuditLog
- Complex filtering with multiple optional parameters
- Pagination support (capped at 100 items/page)
- Returns (Items, TotalCount) tuple
```

### 3. External Services
```
Infrastructure/ExternalServices/EventPublisher.cs (110 lines)
- HTTP-based Event Bus integration
- Publishes to Event Bus Service (http://localhost:5020)
- Graceful error handling and comprehensive logging
- Serializes events to JSON with metadata
```

### 4. Configuration
```
WebAPI/DI/DependencyInjection.cs (80 lines)
- AddAdminServices() extension method
- Registers all repositories, services, DbContext
- Configures HttpClient for EventPublisher
- Connection string and retry policy setup

Infrastructure/Configurations/EntityConfigurations.cs (200 lines)
- AdminUserConfiguration
- RoleConfiguration
- AuditLogConfiguration
- Fluent API mapping details
```

---

## 🔗 Service Registration in Program.cs (Usage)

```csharp
// Add this line in Program.cs
builder.Services.AddAdminServices(builder.Configuration);
```

This automatically registers:
- DbContext (PostgreSQL with retry policy)
- 3 Repositories (scoped lifetime)
- 3 Application Services (scoped lifetime)
- Event Publisher (HttpClient with 10-second timeout)

---

## 📊 Repository Methods Summary

### AdminUserRepository
```
GetByIdAsync(id) → AdminUser?
GetByEmailAsync(email) → AdminUser?
GetAllAsync() → IEnumerable<AdminUser>
GetByStatusAsync(status) → IEnumerable<AdminUser>
AddAsync(adminUser) → AdminUser
UpdateAsync(adminUser) → AdminUser
DeleteAsync(id) → void
SaveChangesAsync() → void
```

### RoleRepository
```
GetByIdAsync(id) → Role?
GetByNameAsync(name) → Role?
GetAllAsync() → IEnumerable<Role>
AddAsync(role) → Role [Prevents system role creation]
UpdateAsync(role) → Role [Prevents system role update]
DeleteAsync(id) → void [Prevents system role deletion]
SaveChangesAsync() → void
```

### AuditLogRepository
```
GetByIdAsync(id) → AuditLog?
GetByAdminUserIdAsync(userId) → IEnumerable<AuditLog>
GetByResourceAsync(type, id) → IEnumerable<AuditLog>
GetAllAsync() → IEnumerable<AuditLog>
GetAllAsync(filter) → (Items, TotalCount) [Complex query with filtering]
AddAsync(auditLog) → AuditLog
SaveChangesAsync() → void
```

### EventPublisher
```
PublishAsync<T>(event, cancellationToken) → void
- HTTP POST to Event Bus Service
- JSON serialization with metadata
- Graceful error handling
```

---

## 📋 AuditLogRepository Filter Support

```csharp
var filter = new AuditLogFilterRequest
{
    AdminUserId = userId,           // Optional
    Action = "UserCreated",         // Optional
    ResourceType = "User",          // Optional
    FromDate = DateTime.UtcNow.AddDays(-30),  // Optional
    ToDate = DateTime.UtcNow,       // Optional
    PageNumber = 1,
    PageSize = 20
};

var (items, totalCount) = await _auditLogRepository.GetAllAsync(filter);
```

---

## 🎲 System Roles (Pre-seeded)

```
1. SuperAdmin (ID: 00000000-0000-0000-0000-000000000001)
   - 12 permissions (all admin.* operations + system.configure)
   - Cannot be modified or deleted

2. Admin (ID: 00000000-0000-0000-0000-000000000002)
   - 6 permissions (user CRUD, role view, audit view)
   - Cannot be modified or deleted

3. Moderator (ID: 00000000-0000-0000-0000-000000000003)
   - 3 permissions (user view, user suspend, audit view)
   - Cannot be modified or deleted
```

---

## 🔌 Event Publisher Configuration

**Default:** `http://localhost:5020`

**Override in appsettings.json:**
```json
{
  "EventBusService": {
    "Url": "http://event-bus-service:5020"
  }
}
```

**Event Structure (JSON):**
```json
{
  "eventType": "AdminUserCreatedEvent",
  "eventId": "550e8400-e29b-41d4-a716-446655440000",
  "timestamp": "2025-11-11T12:00:00Z",
  "data": {
    "adminUserId": "550e8400-e29b-41d4-a716-446655440001",
    "email": "admin@techbirdsfly.com",
    "fullName": "Admin User"
  }
}
```

---

## ✅ Entity Relationships

```
AdminUser (1) ──────────────────────────── (M) AuditLog
    ├── Many-to-Many ──── Role
    └── One-to-Many ────── AuditLog

Role (1) ──────────────────────────── (M) AdminUser

AuditLog (M) ──────────────────────────── (1) AdminUser
```

---

## 🗄️ Database Constraints

**AdminUser:**
- ✅ Email: UNIQUE, MAX 256 chars
- ✅ FullName: REQUIRED, MAX 256 chars
- ✅ Status: REQUIRED, MAX 50 chars, DEFAULT 'Active'
- ✅ CreatedAt: DEFAULT CURRENT_TIMESTAMP
- ✅ UpdatedAt: DEFAULT CURRENT_TIMESTAMP
- ✅ INDEX: (Email), (Status), (CreatedAt)

**Role:**
- ✅ Name: UNIQUE, MAX 128 chars
- ✅ Description: MAX 500 chars
- ✅ IsSystem: DEFAULT false
- ✅ Permissions: JSONB array, DEFAULT '[]'
- ✅ CreatedAt: DEFAULT CURRENT_TIMESTAMP
- ✅ UpdatedAt: DEFAULT CURRENT_TIMESTAMP
- ✅ INDEX: (Name), (IsSystem)

**AuditLog:**
- ✅ Action: REQUIRED, MAX 100 chars
- ✅ ResourceType: REQUIRED, MAX 100 chars
- ✅ ResourceId: MAX 256 chars
- ✅ IpAddress: MAX 45 chars (IPv6 support)
- ✅ UserAgent: MAX 500 chars
- ✅ Details, OldValues, NewValues: JSONB
- ✅ CreatedAt: DEFAULT CURRENT_TIMESTAMP
- ✅ INDEX: (AdminUserId), (Action), (ResourceType), (CreatedAt), (AdminUserId, CreatedAt)

---

## 🚀 Migration Commands (When Ready)

```bash
# Create initial migration
cd services/admin-service
dotnet ef migrations add InitialCreate -o Infrastructure/Migrations

# Apply migration
dotnet ef database update

# Add migration after schema changes
dotnet ef migrations add [MigrationName] -o Infrastructure/Migrations

# Rollback last migration
dotnet ef database update [PreviousMigrationName]
```

---

## 📦 NuGet Dependencies Required

```xml
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.0" />
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.Configuration" Version="8.0.0" />
```

---

## 🔍 Error Handling

**AdminUserRepository:**
- Returns null for empty GUIDs
- Throws if updating non-existent user
- Normalizes emails to lowercase

**RoleRepository:**
- Prevents system role creation
- Prevents system role modification
- Prevents system role deletion
- Clear error messages

**AuditLogRepository:**
- Returns empty enumerable for invalid parameters
- Paginates with configurable page size
- Caps max page size at 100 items

**EventPublisher:**
- Logs but doesn't throw on HTTP failures
- Handles timeouts gracefully
- Handles serialization errors
- Service remains operational if Event Bus unavailable

---

## 🧬 Architecture Diagram

```
Program.cs
    ↓
DependencyInjection.AddAdminServices()
    ├─→ Register DbContext
    ├─→ Register Repositories
    │   ├─→ AdminUserRepository
    │   ├─→ RoleRepository
    │   └─→ AuditLogRepository
    ├─→ Register Application Services
    │   ├─→ AdminUserApplicationService
    │   ├─→ RoleApplicationService
    │   └─→ AuditLogApplicationService
    └─→ Register EventPublisher (HttpClient)

AdminDbContext
    ├─→ PostgreSQL
    ├─→ Entity Configurations
    ├─→ System Roles Seeded
    └─→ Migration History

Controllers (WebAPI)
    ↓
Application Services
    ↓
Repositories
    ↓
AdminDbContext (EF Core)
    ↓
PostgreSQL Database

Domain Events
    ↓
EventPublisher
    ↓
Event Bus Service (HTTP)
    ↓
Kafka Topics
    ↓
Other Microservices
```

---

## ✨ Next Phase (WebAPI Layer)

Ready to create:
1. **AdminUsersController** (150 lines) - User management endpoints
2. **RolesController** (120 lines) - Role management endpoints
3. **AuditLogsController** (100 lines) - Audit log querying endpoints
4. **Update Program.cs** (200 lines) - Serilog, OpenTelemetry, Health checks
5. **appsettings files** (100 lines) - Configuration

**Total remaining:** ~650 lines for complete WebAPI

---

**Infrastructure Status:** ✅ COMPLETE - Ready for WebAPI controllers
