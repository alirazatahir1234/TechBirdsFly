# Clean Architecture Implementation Guide

## 🏗️ Overview

This guide documents the **Clean Architecture** implementation applied to TechBirdsFly microservices. Each service follows a layered architecture pattern with clear separation of concerns, making the codebase maintainable, testable, and scalable.

---

## 📚 Architecture Layers

### 1. **Domain Layer** (Innermost)
Contains the core business logic and domain models that are independent of any external technology.

**Responsibility:**
- Define aggregate roots and entities
- Implement business rules
- Define domain events
- No dependencies on other layers

**Example (AuthService):**
```
Domain/
├── Entities/
│   └── User.cs                    # User aggregate root
└── Events/
    └── UserDomainEvents.cs        # Domain events
```

**Key Concepts:**
- **BaseEntity**: Common properties (Id, CreatedAt, UpdatedAt, IsDeleted)
- **IAggregateRoot**: Marker interface for aggregate roots
- **DomainEvents**: Immutable record of what happened in the domain

---

### 2. **Application Layer**
Implements use cases and orchestrates the business logic. Acts as a bridge between Domain and Infrastructure layers.

**Responsibility:**
- Implement application services
- Define DTOs for API contracts
- Define repository interfaces
- Coordinate between layers
- Handle cross-cutting concerns

**Example (AuthService):**
```
Application/
├── Interfaces/
│   └── IAuthRepositories.cs       # Repository and service interfaces
├── Services/
│   └── AuthApplicationService.cs  # Business logic orchestration
└── DTOs/
    └── AuthDtos.cs                # Request/Response contracts
```

**Key Concepts:**
- **ApplicationService**: Coordinates business operations
- **DTOs**: Data Transfer Objects for API communication
- **IRepository**: Abstraction for data access
- **IUnitOfWork**: Coordinates multiple repositories

---

### 3. **Infrastructure Layer**
Implements technical concerns and integrates with external systems.

**Responsibility:**
- EF Core DbContext and configurations
- Repository implementations
- External service implementations (caching, authentication, messaging)
- Database migrations
- Email, SMS, blob storage integrations

**Example (AuthService):**
```
Infrastructure/
├── Persistence/
│   └── AuthDbContext.cs           # EF Core context
├── Repositories/
│   ├── UserRepository.cs          # Repository implementation
│   └── UnitOfWork.cs              # Unit of Work pattern
└── ExternalServices/
    ├── PasswordService.cs         # Password hashing
    ├── JwtTokenService.cs         # Token generation
    └── RedisCacheService.cs       # Redis caching
```

**Key Concepts:**
- **DbContext**: Entity Framework configuration
- **Repository Pattern**: Abstraction over data access
- **Unit of Work**: Transaction management
- **External Services**: Third-party integrations

---

### 4. **WebAPI Layer** (Outermost)
Exposes application through HTTP endpoints.

**Responsibility:**
- ASP.NET Core controllers
- Dependency injection setup
- Request/response handling
- API documentation
- Error handling middleware

**Example (AuthService):**
```
WebAPI/
├── Controllers/
│   └── AuthController.cs          # HTTP endpoints
└── DI/
    └── DependencyInjectionExtensions.cs  # DI configuration
```

**Key Concepts:**
- **Controllers**: HTTP endpoints mapped to application services
- **DTOs**: Data contracts for API
- **Dependency Injection**: Services configuration
- **Error Handling**: Centralized exception handling

---

## 🧩 Shared Kernel

All services reference a common `Shared` project containing reusable components.

**Structure:**
```
src/Shared/
├── Kernel/
│   ├── BaseEntity.cs              # Base class for all entities
│   ├── IAggregateRoot.cs          # Aggregate root marker
│   ├── DomainEvent.cs             # Domain event base class
│   ├── Result.cs                  # Result pattern
│   └── Pagination.cs              # Pagination helpers
├── DTOs/
│   └── CommonDtos.cs              # Shared DTOs
├── Common/
│   └── AppConstants.cs            # Application constants
└── Infrastructure/
    └── Shared services
```

**Benefits:**
- Consistency across services
- Code reuse
- Shared domain models
- Reduced duplication

---

## 🔄 Data Flow Example

### User Registration Flow (AuthService)

```
HTTP Request (POST /api/auth/register)
       ↓
   AuthController
       ↓
   AuthApplicationService
       ↓
   IPasswordService (Hash password)
       ↓
   IUnitOfWork.UserRepository
       ↓
   User.Create() (Domain logic)
       ↓
   UserRepository (Save to DB)
       ↓
   Domain Events Raised
       ↓
   HTTP Response (RegisterResponseDto)
```

---

## 📁 Complete Service Structure

```
services/[ServiceName]/src/
│
├── Domain/
│   ├── Entities/
│   │   └── [AggregateRoot].cs
│   ├── ValueObjects/
│   │   └── [ValueObject].cs
│   └── Events/
│       └── [DomainEvent].cs
│
├── Application/
│   ├── Interfaces/
│   │   ├── I[Service].cs
│   │   └── I[Repository].cs
│   ├── Services/
│   │   └── [ApplicationService].cs
│   └── DTOs/
│       └── [ServiceDtos].cs
│
├── Infrastructure/
│   ├── Persistence/
│   │   └── [Service]DbContext.cs
│   ├── Repositories/
│   │   ├── [Repository].cs
│   │   └── UnitOfWork.cs
│   └── ExternalServices/
│       ├── [Service1Implementation].cs
│       └── [Service2Implementation].cs
│
├── WebAPI/
│   ├── Controllers/
│   │   └── [ServiceController].cs
│   └── DI/
│       └── DependencyInjectionExtensions.cs
│
└── Program.cs
```

---

## 🚀 Dependency Rules

**Key Rule:** Inner layers should NOT depend on outer layers.

```
     WebAPI (Highest)
        ↓ (depends on)
    Application
        ↓ (depends on)
   Infrastructure & Domain
        ↓ (depends on)
      Shared (Lowest)
```

**Valid Dependencies:**
- ✅ WebAPI → Application
- ✅ Application → Domain
- ✅ Infrastructure → Domain
- ✅ Application → Shared
- ✅ Domain → Shared

**Invalid Dependencies:**
- ❌ Domain → Application
- ❌ Domain → Infrastructure
- ❌ Domain → WebAPI

---

## 🏭 How to Add a New Feature

### Example: Add Admin Permissions to Users

**1. Domain Layer** (What is the business rule?)
```csharp
// Domain/Entities/User.cs
public class User : BaseEntity
{
    public List<string> Permissions { get; private set; } = new();
    
    public void AssignPermission(string permission)
    {
        if (!Permissions.Contains(permission))
            Permissions.Add(permission);
    }
}
```

**2. Application Layer** (How do we implement this use case?)
```csharp
// Application/Services/AuthApplicationService.cs
public async Task AssignPermissionAsync(Guid userId, string permission)
{
    var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);
    user.AssignPermission(permission);
    await _unitOfWork.UserRepository.UpdateAsync(user);
    await _unitOfWork.SaveChangesAsync();
}
```

**3. Infrastructure Layer** (How do we persist this?)
```csharp
// Infrastructure/Persistence/AuthDbContext.cs
modelBuilder.Entity<User>(builder =>
{
    builder.Property(u => u.Permissions)
        .HasConversion(
            v => string.Join(",", v),
            v => v.Split(",", StringSplitOptions.RemoveEmptyEntries).ToList()
        );
});
```

**4. WebAPI Layer** (How do we expose this?)
```csharp
// WebAPI/Controllers/AuthController.cs
[HttpPost("permissions/{userId}")]
public async Task<IActionResult> AssignPermission(
    Guid userId, 
    [FromBody] AssignPermissionRequest request)
{
    await _authService.AssignPermissionAsync(userId, request.Permission);
    return Ok(new { message = "Permission assigned" });
}
```

---

## 🔧 Setting Up a New Service

### Step 1: Create Folder Structure
```bash
mkdir -p services/MyService/src/{Domain,Application,Infrastructure,WebAPI,Tests}
mkdir -p services/MyService/src/{Domain/Entities,Domain/Events}
mkdir -p services/MyService/src/{Application/Interfaces,Application/Services,Application/DTOs}
mkdir -p services/MyService/src/{Infrastructure/Persistence,Infrastructure/Repositories,Infrastructure/ExternalServices}
mkdir -p services/MyService/src/{WebAPI/Controllers,WebAPI/DI}
```

### Step 2: Create Project Files

**Domain project:**
```xml
<!-- MyService.Domain.csproj -->
<PackageReference Include="TechBirdsFly.Shared" Version="1.0.0" />
```

**Application project:**
```xml
<!-- MyService.Application.csproj -->
<ProjectReference Include="../MyService.Domain/MyService.Domain.csproj" />
<ProjectReference Include="../../src/Shared/TechBirdsFly.Shared.csproj" />
```

**Infrastructure project:**
```xml
<!-- MyService.Infrastructure.csproj -->
<ProjectReference Include="../MyService.Application/MyService.Application.csproj" />
<ProjectReference Include="../MyService.Domain/MyService.Domain.csproj" />
```

**WebAPI project:**
```xml
<!-- MyService.WebAPI.csproj -->
<ProjectReference Include="../MyService.Infrastructure/MyService.Infrastructure.csproj" />
<ProjectReference Include="../MyService.Application/MyService.Application.csproj" />
```

### Step 3: Define Domain Model
```csharp
// Domain/Entities/MyAggregate.cs
public class MyAggregate : BaseEntity, IAggregateRoot
{
    // Properties and methods
}
```

### Step 4: Create Repository Interface
```csharp
// Application/Interfaces/IMyRepository.cs
public interface IMyRepository
{
    Task<MyAggregate?> GetByIdAsync(Guid id);
    Task AddAsync(MyAggregate entity);
    // etc.
}
```

### Step 5: Implement Repository
```csharp
// Infrastructure/Repositories/MyRepository.cs
public class MyRepository : IMyRepository
{
    // Implementation
}
```

### Step 6: Create Application Service
```csharp
// Application/Services/MyApplicationService.cs
public class MyApplicationService
{
    private readonly IUnitOfWork _unitOfWork;
    // Implementation
}
```

### Step 7: Create Controller
```csharp
// WebAPI/Controllers/MyController.cs
[ApiController]
[Route("api/[controller]")]
public class MyController : ControllerBase
{
    // Endpoints
}
```

### Step 8: Setup Dependency Injection
```csharp
// WebAPI/DI/DependencyInjectionExtensions.cs
public static IServiceCollection AddMyServiceServices(this IServiceCollection services)
{
    // Register services
}
```

---

## 📝 Best Practices

### 1. **Entities Should Encapsulate Business Logic**
```csharp
// ❌ Bad - Anemic entity
public class User
{
    public string Email { get; set; }
    public string Password { get; set; }
}

// ✅ Good - Rich entity with behavior
public class User : BaseEntity
{
    public string Email { get; private set; }
    private string PasswordHash { get; set; }
    
    public void SetPassword(string password, IPasswordService svc)
    {
        PasswordHash = svc.Hash(password);
    }
}
```

### 2. **Use DTOs for API Communication**
```csharp
// ✅ Good - API doesn't expose domain model directly
[HttpPost]
public async Task<IActionResult> CreateUser(CreateUserDto dto)
{
    var user = User.Create(dto.Email, dto.Password);
    // ...
}
```

### 3. **Raise Domain Events for Side Effects**
```csharp
// ✅ Good - Domain event triggers notifications
public class User : BaseEntity
{
    public void Register()
    {
        RaiseDomainEvent(new UserRegisteredDomainEvent(this.Id));
    }
}
```

### 4. **Use Result Pattern for Errors**
```csharp
// ✅ Good - Explicit error handling
public async Task<Result<LoginResponseDto>> LoginAsync(LoginRequestDto request)
{
    var user = await _repo.GetByEmailAsync(request.Email);
    if (user == null)
        return Result<LoginResponseDto>.Failure("Invalid credentials");
    // ...
}
```

### 5. **Inject Abstractions, Not Implementations**
```csharp
// ❌ Bad
public class AuthService
{
    private UserRepository _repo;  // Wrong: concrete class
}

// ✅ Good
public class AuthService
{
    private IUserRepository _repo;  // Right: interface
}
```

---

## 🧪 Testing Strategy

### Unit Tests (Domain Logic)
```csharp
[Fact]
public void User_Create_ShouldRaiseDomainEvent()
{
    var user = User.Create("user@test.com", "hash", "John", "Doe");
    
    var @event = user.DomainEvents.First();
    Assert.IsType<UserCreatedDomainEvent>(@event);
}
```

### Integration Tests (Infrastructure)
```csharp
[Fact]
public async Task UserRepository_Add_ShouldPersistToDatabase()
{
    var user = User.Create(...);
    await _repository.AddAsync(user);
    
    var persisted = await _repository.GetByIdAsync(user.Id);
    Assert.NotNull(persisted);
}
```

---

## 🔗 Cross-Service Communication

Services communicate through:
1. **HTTP/gRPC** for synchronous calls
2. **Message Queue** for asynchronous events
3. **Shared DTOs** for common models

---

## 📚 References

- Domain-Driven Design (Eric Evans)
- Clean Architecture (Robert C. Martin)
- Microservices Patterns (Chris Richardson)

---

## ✅ Implementation Checklist

- [ ] Create folder structure for new service
- [ ] Define domain entities and value objects
- [ ] Create repository interfaces
- [ ] Implement repositories
- [ ] Create application service
- [ ] Create DTOs
- [ ] Create controllers
- [ ] Setup dependency injection
- [ ] Create unit tests
- [ ] Create integration tests
- [ ] Add to solution file
- [ ] Update docker-compose.yml
- [ ] Create API documentation

---

Generated: October 31, 2025
Version: 1.0.0
