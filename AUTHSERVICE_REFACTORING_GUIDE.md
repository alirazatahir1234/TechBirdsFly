# AuthService Clean Architecture Refactoring Guide

**Status:** Ready for Implementation  
**Date:** October 31, 2025  
**Purpose:** Reorganize existing AuthService code into Clean Architecture layers without deleting anything

---

## 🎯 Quick Summary

Reorganize the existing AuthService project from flat structure into layered Clean Architecture:
- **Domain** (business logic, entities)
- **Application** (use cases, DTOs, interfaces)
- **Infrastructure** (data access, external services)
- **WebAPI** (controllers, middleware, DI)
- **Tests** (unit & integration tests)

**Nothing gets deleted** — only moved and refactored.

---

## 📋 Phase 1: File Movement Map

### Directory Structure to Create

```
services/auth-service/src/
├── Domain/
│   ├── Entities/
│   ├── ValueObjects/
│   └── Events/
│
├── Application/
│   ├── Interfaces/
│   ├── Services/
│   ├── DTOs/
│   ├── Commands/
│   └── Queries/
│
├── Infrastructure/
│   ├── Persistence/
│   ├── Repositories/
│   ├── Cache/
│   ├── ExternalServices/
│   └── Configurations/
│
├── WebAPI/
│   ├── Controllers/
│   ├── Middlewares/
│   └── DI/
│
├── Tests/
│   ├── UnitTests/
│   └── IntegrationTests/
│
├── Program.cs          (stays at root)
├── appsettings.json    (stays at root)
└── AuthService.csproj  (stays at root)
```

---

## 🔄 Detailed File Movement Guide

### 1️⃣ DOMAIN LAYER

**Location:** `services/auth-service/src/Domain/`

#### Move to `Domain/Entities/`
```
From: Current location → To: Domain/Entities/
─────────────────────────────────────────────

User.cs                     → Domain/Entities/User.cs
Role.cs                     → Domain/Entities/Role.cs
RefreshToken.cs             → Domain/Entities/RefreshToken.cs
Permission.cs               → Domain/Entities/Permission.cs
(any other entity models)   → Domain/Entities/
```

**Namespace Change:**
```csharp
// Before:
namespace AuthService.Models;

// After:
namespace AuthService.Domain.Entities;
```

#### Move to `Domain/ValueObjects/` (if they exist)
```
Email.cs                    → Domain/ValueObjects/Email.cs
PasswordHash.cs             → Domain/ValueObjects/PasswordHash.cs
PhoneNumber.cs              → Domain/ValueObjects/PhoneNumber.cs
```

**Namespace Change:**
```csharp
// Before:
namespace AuthService.Models;

// After:
namespace AuthService.Domain.ValueObjects;
```

#### Move to `Domain/Events/`
```
UserCreatedEvent.cs         → Domain/Events/UserCreatedEvent.cs
UserLoginEvent.cs           → Domain/Events/UserLoginEvent.cs
PasswordResetEvent.cs       → Domain/Events/PasswordResetEvent.cs
(any other domain events)   → Domain/Events/
```

**Namespace Change:**
```csharp
// Before:
namespace AuthService.Models;
// or: namespace AuthService.Events;

// After:
namespace AuthService.Domain.Events;
```

---

### 2️⃣ APPLICATION LAYER

**Location:** `services/auth-service/src/Application/`

#### Move to `Application/Interfaces/`
```
From: Current location → To: Application/Interfaces/
──────────────────────────────────────────────────────

IUserRepository.cs          → Application/Interfaces/IUserRepository.cs
IAuthService.cs             → Application/Interfaces/IAuthService.cs
ITokenService.cs            → Application/Interfaces/ITokenService.cs
ICacheService.cs            → Application/Interfaces/ICacheService.cs
IPasswordHasher.cs          → Application/Interfaces/IPasswordHasher.cs
IEmailService.cs            → Application/Interfaces/IEmailService.cs
ISmsService.cs              → Application/Interfaces/ISmsService.cs
IUnitOfWork.cs              → Application/Interfaces/IUnitOfWork.cs
(any other service interfaces) → Application/Interfaces/
```

**Namespace Change:**
```csharp
// Before:
namespace AuthService.Interfaces;
// or: namespace AuthService.Services;

// After:
namespace AuthService.Application.Interfaces;
```

#### Move to `Application/Services/`
```
From: Current location → To: Application/Services/
───────────────────────────────────────────────────

AuthService.cs              → Application/Services/AuthService.cs
TokenService.cs             → Application/Services/TokenService.cs
CacheService.cs             → Application/Services/CacheService.cs
PasswordHasher.cs           → Application/Services/PasswordHasher.cs
(any other application services) → Application/Services/
```

**Namespace Change:**
```csharp
// Before:
namespace AuthService.Services;

// After:
namespace AuthService.Application.Services;

// Update constructor dependencies:
// Before: public AuthService(IUserRepository repo) { }
// After: public AuthService(IUserRepository repo) { }
// (Same - interfaces moved to Application/Interfaces/)
```

#### Move to `Application/DTOs/`
```
From: Current location → To: Application/DTOs/
──────────────────────────────────────────────

LoginRequest.cs             → Application/DTOs/LoginRequest.cs
LoginResponse.cs            → Application/DTOs/LoginResponse.cs
RegisterRequest.cs          → Application/DTOs/RegisterRequest.cs
RegisterResponse.cs         → Application/DTOs/RegisterResponse.cs
UserDto.cs                  → Application/DTOs/UserDto.cs
TokenDto.cs                 → Application/DTOs/TokenDto.cs
(any other DTO files)       → Application/DTOs/
```

**Namespace Change:**
```csharp
// Before:
namespace AuthService.Models;
// or: namespace AuthService.DTOs;
// or: namespace AuthService.Requests;

// After:
namespace AuthService.Application.DTOs;
```

#### Move to `Application/Commands/` (if using CQRS)
```
From: Current location → To: Application/Commands/
──────────────────────────────────────────────────

RegisterUserCommand.cs      → Application/Commands/RegisterUserCommand.cs
LoginCommand.cs             → Application/Commands/LoginCommand.cs
ResetPasswordCommand.cs     → Application/Commands/ResetPasswordCommand.cs
(if you use CQRS pattern)   → Application/Commands/
```

**Namespace Change:**
```csharp
// Before:
namespace AuthService.Commands;

// After:
namespace AuthService.Application.Commands;
```

#### Move to `Application/Queries/` (if using CQRS)
```
From: Current location → To: Application/Queries/
─────────────────────────────────────────────────

GetUserByIdQuery.cs         → Application/Queries/GetUserByIdQuery.cs
GetUserByEmailQuery.cs      → Application/Queries/GetUserByEmailQuery.cs
(if you use CQRS pattern)   → Application/Queries/
```

**Namespace Change:**
```csharp
// Before:
namespace AuthService.Queries;

// After:
namespace AuthService.Application.Queries;
```

---

### 3️⃣ INFRASTRUCTURE LAYER

**Location:** `services/auth-service/src/Infrastructure/`

#### Move to `Infrastructure/Persistence/`
```
From: Current location → To: Infrastructure/Persistence/
─────────────────────────────────────────────────────────

AuthDbContext.cs            → Infrastructure/Persistence/AuthDbContext.cs
Migrations/                 → Infrastructure/Persistence/Migrations/
DesignTimeDbContextFactory.cs → Infrastructure/Persistence/DesignTimeDbContextFactory.cs
(all migration files)       → Infrastructure/Persistence/Migrations/
```

**Namespace Change:**
```csharp
// Before:
namespace AuthService.Data;
// or: namespace AuthService.Context;

// After:
namespace AuthService.Infrastructure.Persistence;
```

#### Move to `Infrastructure/Repositories/`
```
From: Current location → To: Infrastructure/Repositories/
──────────────────────────────────────────────────────────

UserRepository.cs           → Infrastructure/Repositories/UserRepository.cs
RoleRepository.cs           → Infrastructure/Repositories/RoleRepository.cs
UnitOfWork.cs               → Infrastructure/Repositories/UnitOfWork.cs
(all EF Core repository implementations) → Infrastructure/Repositories/
```

**Namespace Change:**
```csharp
// Before:
namespace AuthService.Repositories;
// or: namespace AuthService.Data.Repositories;

// After:
namespace AuthService.Infrastructure.Repositories;

// Update constructor:
// Before: public UserRepository(AuthDbContext context) { }
// After: public UserRepository(AuthDbContext context) { }
// Note: AuthDbContext namespace changed, update the using statement
```

#### Move to `Infrastructure/Cache/`
```
From: Current location → To: Infrastructure/Cache/
──────────────────────────────────────────────────

RedisCacheService.cs        → Infrastructure/Cache/RedisCacheService.cs
MemoryCacheService.cs       → Infrastructure/Cache/MemoryCacheService.cs
CacheKeyGenerator.cs        → Infrastructure/Cache/CacheKeyGenerator.cs
```

**Namespace Change:**
```csharp
// Before:
namespace AuthService.Services;
// or: namespace AuthService.Cache;

// After:
namespace AuthService.Infrastructure.Cache;

// Update DI registration (in WebAPI/DI/DependencyInjection.cs)
// Before: services.AddScoped<ICacheService, RedisCacheService>();
// After: services.AddScoped<ICacheService, RedisCacheService>();
// (Same interface reference, updated using statement)
```

#### Move to `Infrastructure/ExternalServices/`
```
From: Current location → To: Infrastructure/ExternalServices/
──────────────────────────────────────────────────────────────

EmailService.cs             → Infrastructure/ExternalServices/EmailService.cs
SmsService.cs               → Infrastructure/ExternalServices/SmsService.cs
GoogleAuthService.cs        → Infrastructure/ExternalServices/GoogleAuthService.cs
JwtTokenGenerator.cs        → Infrastructure/ExternalServices/JwtTokenGenerator.cs
PasswordHasher.cs           → Infrastructure/ExternalServices/PasswordHasher.cs
(any external integrations) → Infrastructure/ExternalServices/
```

**Namespace Change:**
```csharp
// Before:
namespace AuthService.Services;
// or: namespace AuthService.ExternalServices;

// After:
namespace AuthService.Infrastructure.ExternalServices;
```

#### Move to `Infrastructure/Configurations/`
```
From: Current location → To: Infrastructure/Configurations/
────────────────────────────────────────────────────────────

UserConfiguration.cs        → Infrastructure/Configurations/UserConfiguration.cs
RoleConfiguration.cs        → Infrastructure/Configurations/RoleConfiguration.cs
EntityConfiguration.cs      → Infrastructure/Configurations/EntityConfiguration.cs
(EF Core Fluent API configs) → Infrastructure/Configurations/
```

**Namespace Change:**
```csharp
// Before:
namespace AuthService.Data.Configurations;
// or: namespace AuthService.Models.Configurations;

// After:
namespace AuthService.Infrastructure.Configurations;
```

---

### 4️⃣ WEBAPI LAYER

**Location:** `services/auth-service/src/WebAPI/`

#### Move to `WebAPI/Controllers/`
```
From: Current location → To: WebAPI/Controllers/
────────────────────────────────────────────────

AuthController.cs           → WebAPI/Controllers/AuthController.cs
TokenController.cs          → WebAPI/Controllers/TokenController.cs
UserController.cs           → WebAPI/Controllers/UserController.cs
(any other controllers)     → WebAPI/Controllers/
```

**Namespace Change:**
```csharp
// Before:
namespace AuthService.Controllers;

// After:
namespace AuthService.WebAPI.Controllers;

// Update constructor dependencies:
// Before: public AuthController(IAuthService authService) { }
// After: public AuthController(IAuthService authService) { }
// (Same - services are in Application.Services, interfaces in Application.Interfaces)
```

#### Move to `WebAPI/Middlewares/`
```
From: Current location → To: WebAPI/Middlewares/
────────────────────────────────────────────────

JwtMiddleware.cs            → WebAPI/Middlewares/JwtMiddleware.cs
ExceptionHandlingMiddleware.cs → WebAPI/Middlewares/ExceptionHandlingMiddleware.cs
RequestLoggingMiddleware.cs → WebAPI/Middlewares/RequestLoggingMiddleware.cs
CorrelationIdMiddleware.cs  → WebAPI/Middlewares/CorrelationIdMiddleware.cs
(any other middlewares)     → WebAPI/Middlewares/
```

**Namespace Change:**
```csharp
// Before:
namespace AuthService.Middleware;
// or: namespace AuthService.Middlewares;

// After:
namespace AuthService.WebAPI.Middlewares;
```

#### Move to `WebAPI/DI/` (Keep existing DI setup)
```
From: Current location → To: WebAPI/DI/
──────────────────────────────────────

DependencyInjection.cs      → WebAPI/DI/DependencyInjection.cs
(any extension methods for DI) → WebAPI/DI/
```

**Namespace Change:**
```csharp
// Before:
namespace AuthService.Extensions;
// or: namespace AuthService;

// After:
namespace AuthService.WebAPI.DI;

// This file is critical - it registers all services
// Update all the using statements to reference new namespaces:
// using AuthService.Application.Services;
// using AuthService.Application.Interfaces;
// using AuthService.Infrastructure.Repositories;
// using AuthService.Infrastructure.Cache;
// using AuthService.Infrastructure.Persistence;
```

#### Keep at Root
```
Program.cs                  → WebAPI/Program.cs (or keep at root)
appsettings.json            → (keep at root)
appsettings.Development.json → (keep at root)
```

**Update Program.cs:**
```csharp
// Add using statements for new namespaces:
using AuthService.WebAPI.DI;
using AuthService.Infrastructure.Persistence;
using AuthService.WebAPI.Middlewares;

// Register services using the new namespace:
builder.Services.AddAuthServiceDependencies(builder.Configuration);
```

---

### 5️⃣ TESTS LAYER

**Location:** `services/auth-service/src/Tests/`

#### Move to `Tests/UnitTests/`
```
From: Current location → To: Tests/UnitTests/
──────────────────────────────────────────────

AuthServiceTests.cs         → Tests/UnitTests/AuthServiceTests.cs
TokenServiceTests.cs        → Tests/UnitTests/TokenServiceTests.cs
PasswordHasherTests.cs      → Tests/UnitTests/PasswordHasherTests.cs
(any other unit tests)      → Tests/UnitTests/
```

**Namespace Change:**
```csharp
// Before:
namespace AuthService.Tests;

// After:
namespace AuthService.Tests.UnitTests;

// Update using statements:
// using AuthService.Application.Services;
// using AuthService.Application.Interfaces;
```

#### Move to `Tests/IntegrationTests/`
```
From: Current location → To: Tests/IntegrationTests/
────────────────────────────────────────────────────

AuthControllerTests.cs      → Tests/IntegrationTests/AuthControllerTests.cs
UserRepositoryTests.cs      → Tests/IntegrationTests/UserRepositoryTests.cs
(any integration tests)     → Tests/IntegrationTests/
```

**Namespace Change:**
```csharp
// Before:
namespace AuthService.Tests;

// After:
namespace AuthService.Tests.IntegrationTests;

// Update using statements:
// using AuthService.WebAPI.Controllers;
// using AuthService.Infrastructure.Repositories;
```

---

## 📝 Namespace Update Checklist

After moving files, verify these namespaces are updated:

### Domain Layer Namespaces
- [x] `Entities/` → `AuthService.Domain.Entities`
- [x] `ValueObjects/` → `AuthService.Domain.ValueObjects`
- [x] `Events/` → `AuthService.Domain.Events`

### Application Layer Namespaces
- [x] `Interfaces/` → `AuthService.Application.Interfaces`
- [x] `Services/` → `AuthService.Application.Services`
- [x] `DTOs/` → `AuthService.Application.DTOs`
- [x] `Commands/` → `AuthService.Application.Commands` (if CQRS)
- [x] `Queries/` → `AuthService.Application.Queries` (if CQRS)

### Infrastructure Layer Namespaces
- [x] `Persistence/` → `AuthService.Infrastructure.Persistence`
- [x] `Repositories/` → `AuthService.Infrastructure.Repositories`
- [x] `Cache/` → `AuthService.Infrastructure.Cache`
- [x] `ExternalServices/` → `AuthService.Infrastructure.ExternalServices`
- [x] `Configurations/` → `AuthService.Infrastructure.Configurations`

### WebAPI Layer Namespaces
- [x] `Controllers/` → `AuthService.WebAPI.Controllers`
- [x] `Middlewares/` → `AuthService.WebAPI.Middlewares`
- [x] `DI/` → `AuthService.WebAPI.DI`

### Tests Layer Namespaces
- [x] `UnitTests/` → `AuthService.Tests.UnitTests`
- [x] `IntegrationTests/` → `AuthService.Tests.IntegrationTests`

---

## 🔗 Critical Files to Update

These files have dependencies across multiple layers and MUST be updated:

### 1. `WebAPI/DI/DependencyInjection.cs` (CRITICAL)
```csharp
// This is the hub - it imports from ALL layers
using AuthService.Application.Services;      // ← Application
using AuthService.Application.Interfaces;    // ← Application
using AuthService.Infrastructure.Persistence; // ← Infrastructure
using AuthService.Infrastructure.Repositories; // ← Infrastructure
using AuthService.Infrastructure.Cache;      // ← Infrastructure
using AuthService.Infrastructure.ExternalServices; // ← Infrastructure

public static class DependencyInjection
{
    public static IServiceCollection AddAuthServiceDependencies(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register all services with updated namespaces
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<ICacheService, RedisCacheService>();
        // ... etc
        
        return services;
    }
}
```

### 2. `WebAPI/Program.cs` (CRITICAL)
```csharp
using AuthService.Infrastructure.Persistence;
using AuthService.WebAPI.DI;
using AuthService.WebAPI.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Add DI
builder.Services.AddAuthServiceDependencies(builder.Configuration);

// Add DbContext
builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseSqlite("Data Source=auth.db"));

var app = builder.Build();

// Add Middlewares
app.UseMiddleware<JwtMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.Run();
```

### 3. `Infrastructure/Repositories/UnitOfWork.cs`
```csharp
using AuthService.Application.Interfaces;
using AuthService.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly AuthDbContext _context;
    
    public UnitOfWork(AuthDbContext context)
    {
        _context = context;
    }
    
    public IUserRepository UserRepository => new UserRepository(_context);
}
```

### 4. `Infrastructure/Persistence/AuthDbContext.cs`
```csharp
using AuthService.Domain.Entities;
using AuthService.Infrastructure.Configurations;

public class AuthDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply configurations
        modelBuilder.ApplyConfiguration(new UserConfiguration());
    }
}
```

### 5. `WebAPI/Controllers/AuthController.cs`
```csharp
using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using AuthService.WebAPI.Middlewares;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    
    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }
    
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        // Implementation
    }
}
```

---

## ✅ Verification Checklist

After completing the refactoring:

### Build Verification
- [ ] Solution compiles without errors
- [ ] No "namespace not found" errors
- [ ] No "type or member not found" errors
- [ ] No circular reference warnings

### Namespace Verification
- [ ] Domain entities use `AuthService.Domain.Entities`
- [ ] Application services use `AuthService.Application.Services`
- [ ] Infrastructure repositories use `AuthService.Infrastructure.Repositories`
- [ ] Controllers use `AuthService.WebAPI.Controllers`
- [ ] All using statements updated

### Dependency Verification
- [ ] Controllers depend on Application layer (interfaces & services)
- [ ] Application layer depends on Domain layer
- [ ] Infrastructure layer depends on Domain layer
- [ ] No Infrastructure → WebAPI dependencies
- [ ] No Domain → Application/Infrastructure dependencies

### Runtime Verification
- [ ] DI container initializes without errors
- [ ] DbContext creates migrations correctly
- [ ] Middleware pipeline executes correctly
- [ ] API endpoints respond correctly
- [ ] No runtime namespace resolution errors

### Test Verification
- [ ] All unit tests still pass
- [ ] All integration tests still pass
- [ ] No test file namespace issues

---

## 🚀 Implementation Steps

### Step 1: Create Folder Structure
```bash
mkdir -p services/auth-service/src/{Domain/{Entities,ValueObjects,Events},Application/{Interfaces,Services,DTOs,Commands,Queries},Infrastructure/{Persistence,Repositories,Cache,ExternalServices,Configurations},WebAPI/{Controllers,Middlewares,DI},Tests/{UnitTests,IntegrationTests}}
```

### Step 2: Move Files
Move each file according to the **File Movement Guide** above.

### Step 3: Update Namespaces
Update all namespaces according to the **Namespace Update Checklist**.

### Step 4: Update Critical Files
Update DI, Program.cs, and all importing files.

### Step 5: Build & Verify
```bash
cd services/auth-service/src
dotnet build
```

### Step 6: Run Tests
```bash
dotnet test
```

### Step 7: Run Application
```bash
dotnet run
```

---

## 📚 File Count Summary

**Before:**
- Flat structure with mixed concerns
- Difficult to locate related files
- Unclear layer responsibilities

**After:**
- 5 distinct layers
- Clear responsibility separation
- Easy navigation and understanding
- Microservice-ready structure

---

## 💡 Notes

1. **Don't Delete Anything** - Only move files
2. **Update Using Statements** - Critical for compilation
3. **Test After Moving** - Run unit/integration tests
4. **Check DI Registration** - Ensure all services are registered
5. **Verify References** - Check that all project references are valid

---

## 🎯 Expected Outcome

After completion:
- ✅ Clean Architecture properly implemented
- ✅ All code compiles and runs
- ✅ All tests pass
- ✅ Ready for Phase 2 (other services)
- ✅ Microservice-ready structure

---

**Version:** 1.0.0  
**Last Updated:** October 31, 2025  
**Status:** Ready for Implementation
