# 🏗️ User Service - Architecture Diagram

**Date**: November 17, 2025  
**Version**: 2.0

---

## 📐 High-Level Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                     CLIENT APPLICATION                      │
│              (Web Browser, Mobile App, etc.)                │
└──────────────────────────┬──────────────────────────────────┘
                           │
                      HTTP/HTTPS
                           │
        ┌──────────────────▼──────────────────┐
        │      API Gateway (YARP)             │
        │  - Routing                          │
        │  - Rate Limiting                    │
        │  - Load Balancing                   │
        └──────────────────┬──────────────────┘
                           │
        ┌──────────────────▼──────────────────┐
        │    User Service                     │
        │  - Port: 5003 (typical)             │
        ├─────────────────────────────────────┤
        │                                     │
        │  ┌─────────────────────────────┐   │
        │  │   Authentication Layer      │   │
        │  │  (JWT Token Validation)     │   │
        │  └──────────────┬──────────────┘   │
        │                 │                  │
        │  ┌──────────────▼──────────────┐  │
        │  │    Controllers Layer        │  │
        │  ├──────────────────────────────┤ │
        │  │                              │ │
        │  │  AuthController             │ │
        │  │  ├─ Register                │ │
        │  │  ├─ Login                   │ │
        │  │  ├─ Verify Email            │ │
        │  │  ├─ Forgot Password         │ │
        │  │  ├─ Reset Password          │ │
        │  │  ├─ Validate Token          │ │
        │  │  └─ Logout                  │ │
        │  │                              │ │
        │  │  UsersController            │ │
        │  │  ├─ Get User                │ │
        │  │  ├─ Get Profile             │ │
        │  │  ├─ Update Profile          │ │
        │  │  ├─ List Users (Admin)      │ │
        │  │  ├─ Deactivate (Admin)      │ │
        │  │  ├─ Reactivate (Admin)      │ │
        │  │  ├─ Assign Role (Admin)     │ │
        │  │  └─ Statistics (Admin)      │ │
        │  │                              │ │
        │  └──────────────┬───────────────┘ │
        │                 │                  │
        │  ┌──────────────▼───────────────┐ │
        │  │    Application Layer        │  │
        │  │  (Services/Business Logic)  │  │
        │  ├──────────────────────────────┤ │
        │  │ • IAuthService              │ │
        │  │ • IUserService              │ │
        │  │ • IProfileService           │ │
        │  │ • IEmailService             │ │
        │  │ • ITokenService             │ │
        │  └──────────────┬───────────────┘ │
        │                 │                  │
        │  ┌──────────────▼───────────────┐ │
        │  │    Data Access Layer        │  │
        │  │  (Entity Framework Core)    │  │
        │  ├──────────────────────────────┤ │
        │  │ • UserRepository            │ │
        │  │ • RoleRepository            │ │
        │  │ • ProfileRepository         │ │
        │  └──────────────┬───────────────┘ │
        │                 │                  │
        │  ┌──────────────▼───────────────┐ │
        │  │    Database                 │  │
        │  │  (SQL Server/PostgreSQL)    │  │
        │  └──────────────────────────────┘ │
        │                                     │
        └─────────────────────────────────────┘
```

---

## 🔄 Request Flow - User Login & Profile Access

```
Client
  │
  ├─→ POST /api/auth/login
  │   │
  │   └─→ AuthController.Login()
  │       ├─→ IAuthService.LoginAsync()
  │       │   ├─→ Validate credentials
  │       │   ├─→ Generate JWT token
  │       │   └─→ Store session
  │       │
  │       └─→ Return: { accessToken, refreshToken }
  │
  ├─→ GET /api/users/profile/me
  │   (Header: Authorization: Bearer <token>)
  │   │
  │   ├─→ JWT Validation Middleware
  │   │   └─→ Extract user ID from claims
  │   │
  │   └─→ UsersController.GetCurrentUser()
  │       ├─→ GetUserId() helper
  │       │   └─→ Parse claims → GUID
  │       │
  │       ├─→ IUserService.GetUserByIdAsync()
  │       │   ├─→ UserRepository.GetAsync()
  │       │   │   └─→ Database query
  │       │   │
  │       │   └─→ Map Entity → DTO
  │       │
  │       └─→ Return: ApiResponse<UserDto>
  │
  └─→ PUT /api/users/profile/update
      (Header: Authorization: Bearer <token>)
      │
      ├─→ JWT Validation
      │
      └─→ UsersController.UpdateProfile()
          ├─→ GetUserId()
          ├─→ Validate ModelState
          ├─→ IProfileService.UpdateProfileAsync()
          │   ├─→ Validate request
          │   ├─→ Update database
          │   └─→ Return updated entity
          │
          └─→ Return: ApiResponse<UserDto>
```

---

## 🔐 Authorization & Access Control Flow

```
Request arrives with JWT Token
│
├─→ Token Validation Middleware
│   ├─ Token exists?          → NO → 401 Unauthorized
│   ├─ Token valid?           → NO → 401 Unauthorized
│   ├─ Token expired?         → YES → 401 Unauthorized
│   └─ Extract claims         → YES, proceed
│       └─ Store in User.Principal
│
├─→ Endpoint Authorization Attribute
│   ├─ [Authorize] only?           → Check token present
│   └─ [Authorize(Roles="Admin")]? → Check roles
│       ├─ User has Admin role?    → NO → 403 Forbidden
│       └─ User has Admin role?    → YES → Proceed
│
├─→ Method-Level Authorization
│   ├─ Check ownership (for /users/{id})
│   │   ├─ User ID == {id}?         → YES, allow
│   │   ├─ User has Admin role?     → YES, allow
│   │   └─ Neither?                 → 403 Forbidden
│   │
│   └─ Check admin operations (for admin endpoints)
│       ├─ GetUserId() extracted    → Get current user ID
│       ├─ Check role               → Must be Admin
│       └─ Log admin action
│
└─→ Action executes with full authorization context
```

---

## 📊 Entity Relationship Diagram

```
┌─────────────┐
│   User      │
├─────────────┤
│ • UserId    │◄────────┐
│ • Email     │         │ 1:1
│ • FirstName │         │
│ • LastName  │    ┌────▼──────┐
│ • Password  │    │   Profile  │
│ • IsActive  │    ├────────────┤
│ • CreatedAt │    │ • ProfileId│
│ • UpdatedAt │    │ • Phone    │
│             │    │ • Address  │
└─────────────┘    │ • Avatar   │
        │          └────────────┘
        │
        │ 1:N
        ├────────────┐
        │            │
    ┌───▼────┐   ┌──▼──┐
    │ UserRole│  │Role  │
    ├────────┤  ├──────┤
    │ UserId ├─►│ Id   │
    │ RoleId │  │ Name │
    └────────┘  └──────┘
```

---

## 🔄 UsersController Endpoint Flow

```
┌─────────────────────────────────────────────────────────────┐
│                 REQUEST ARRIVES                             │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  1. Authentication Middleware                               │
│     └─ Validates JWT Token                                  │
│        ├─ Token present?                                    │
│        ├─ Token valid?                                      │
│        └─ Extract claims                                    │
│                                                              │
│  2. Authorization Attribute                                 │
│     └─ [Authorize] / [Authorize(Roles="Admin")]             │
│        ├─ User authenticated?                               │
│        └─ User has required role?                           │
│                                                              │
│  3. UsersController Method                                  │
│     ├─ Get UserId() from claims                             │
│     ├─ Validate ModelState                                  │
│     ├─ Check authorization (ownership/admin)                │
│     └─ Call IUserService                                    │
│                                                              │
│  4. Application Service                                     │
│     ├─ Validate request data                                │
│     ├─ Apply business logic                                 │
│     └─ Call repository                                      │
│                                                              │
│  5. Repository/Entity Framework                             │
│     ├─ Build query                                          │
│     ├─ Execute query                                        │
│     └─ Map entity to DTO                                    │
│                                                              │
│  6. Response                                                │
│     └─ Return ApiResponse<T>                                │
│        ├─ 200 OK (success)                                  │
│        ├─ 400 Bad Request (validation)                      │
│        ├─ 401 Unauthorized (auth)                           │
│        ├─ 403 Forbidden (authorization)                     │
│        ├─ 404 Not Found (resource)                          │
│        └─ 500 Internal Server Error                         │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

---

## 🧩 Component Interaction Diagram

```
┌──────────────────────────────────────────────────────────────┐
│                    USERS CONTROLLER                          │
├──────────────────────────────────────────────────────────────┤
│                                                               │
│  Dependencies Injected:                                      │
│  - IUserService                                              │
│  - IProfileService                                           │
│  - ILogger<UsersController>                                  │
│                                                               │
│  Public Methods:                                             │
│  ├─ GetUser(id)           ──────┐                            │
│  ├─ GetCurrentUser()            │                            │
│  ├─ UpdateProfile(request) ─────┤                            │
│  ├─ GetUsers(query)       ──────┼──→ IUserService            │
│  ├─ DeactivateUser(id)   ───────┤    (8 operations)         │
│  ├─ ReactivateUser(id)   ───────┤                            │
│  ├─ AssignRole(id, role) ───────┤                            │
│  └─ GetStatistics()       ──────┤                            │
│                                  │                            │
│  Private Methods:                └──→ IProfileService        │
│  └─ GetUserId()                     (2 operations)          │
│     (Claims extraction)                                     │
│                                      └──→ ILogger            │
│                                          (Audit logs)       │
│                                                               │
└──────────────────────────────────────────────────────────────┘
```

---

## 🌐 API Endpoint Tree

```
/api
├── /auth                          (AuthController)
│   ├── POST /register
│   ├── POST /login
│   ├── POST /verify-email
│   ├── POST /forgot-password
│   ├── POST /reset-password
│   ├── POST /validate-token
│   └── POST /logout
│
└── /users                          (UsersController)
    ├── GET /{id}                   (Get user by ID)
    │   └─ Auth: JWT, Owner/Admin
    │
    ├── GET /profile/me             (Get current user)
    │   └─ Auth: JWT
    │
    ├── PUT /profile/update         (Update profile)
    │   └─ Auth: JWT, Self only
    │
    ├── GET                         (List users - Admin)
    │   ├─ Query: ?pageNumber=1
    │   ├─ Query: ?pageSize=20
    │   ├─ Query: ?sortBy=email
    │   ├─ Query: ?filterByRole=Admin
    │   ├─ Query: ?search=john
    │   └─ Auth: JWT, Admin only
    │
    ├── POST /{id}/deactivate      (Deactivate - Admin)
    │   └─ Auth: JWT, Admin only
    │
    ├── POST /{id}/reactivate      (Reactivate - Admin)
    │   └─ Auth: JWT, Admin only
    │
    ├── POST /{id}/assign-role     (Assign role - Admin)
    │   └─ Auth: JWT, Admin only
    │
    └── GET /statistics             (Get stats - Admin)
        └─ Auth: JWT, Admin only
```

---

## 🔐 Security Layers

```
Layer 1: Transport Security
├─ HTTPS/TLS
└─ Encryption in transit

Layer 2: Authentication
├─ JWT Token validation
├─ Token expiration check
└─ Claim extraction

Layer 3: Authorization
├─ Role-based access (RBAC)
├─ Ownership verification
├─ Admin-only operations
└─ Fine-grained permissions

Layer 4: Input Validation
├─ Model state validation
├─ GUID format validation
├─ String length limits
└─ Email format validation

Layer 5: Application Logic
├─ Business rule validation
├─ Data consistency checks
├─ Audit logging
└─ Error handling

Layer 6: Data Storage
├─ SQL parameterization
├─ Entity Framework LINQ
├─ Database constraints
└─ Encryption at rest (optional)
```

---

## 📈 Request Volume Scaling

```
Estimated Request Distribution:
(Typical SaaS application)

Auth Service:        35% of requests
├─ Register/Login    25%
├─ Token validation  7%
└─ Password reset    3%

User Service:        65% of requests
├─ Get user data     30%
│   ├─ Get profile
│   ├─ Get current user
│   └─ List users
│
├─ Update data       20%
│   └─ Update profile
│
├─ Admin ops         10%
│   ├─ Deactivate
│   ├─ Assign role
│   └─ Statistics
│
└─ Other             5%


Scaling Strategy:
├─ Load Balancing (API Gateway)
├─ Caching (Redis)
│   ├─ Cache user profiles (TTL: 5 min)
│   ├─ Cache role assignments
│   └─ Cache statistics
│
├─ Database
│   ├─ Connection pooling
│   ├─ Query optimization
│   └─ Indexing
│
└─ Async Operations
    ├─ Background jobs
    └─ Event publishing
```

---

## 🎯 Deployment Architecture

```
┌─────────────────────────────────────┐
│         Docker Container            │
├─────────────────────────────────────┤
│                                      │
│  ┌─────────────────────────────┐   │
│  │   User Service Image        │   │
│  ├─────────────────────────────┤   │
│  │                              │   │
│  │  • UserService.dll          │   │
│  │  • All dependencies         │   │
│  │  • Configuration files      │   │
│  │  • Database migrations      │   │
│  │                              │   │
│  └──────────────┬───────────────┘  │
│                 │                   │
│  ┌──────────────▼───────────────┐  │
│  │    .NET Runtime 9.0          │  │
│  └──────────────┬───────────────┘  │
│                 │                   │
│  ┌──────────────▼───────────────┐  │
│  │    Port: 5003 (exposed)      │  │
│  └──────────────────────────────┘  │
│                                      │
└─────────────────────────────────────┘
         │
         ├─→ Kubernetes Cluster
         │   ├─ Service
         │   ├─ Pod
         │   ├─ ReplicaSet
         │   └─ ConfigMap/Secret
         │
         └─→ Docker Compose (Dev)
             ├─ user-service
             ├─ postgres
             └─ redis (optional)
```

---

## ✅ Architecture Summary

| Layer | Component | Purpose |
|-------|-----------|---------|
| **Presentation** | UsersController | HTTP endpoints |
| **Authentication** | JWT Middleware | Token validation |
| **Authorization** | RBAC | Access control |
| **Business Logic** | Services (IUserService, IProfileService) | Operations |
| **Data Access** | Entity Framework | Database queries |
| **Database** | SQL Server/PostgreSQL | Data storage |
| **Caching** | Redis (optional) | Performance |
| **Logging** | Serilog | Audit trail |
| **Security** | HTTPS, Encryption | Data protection |

---

## 📌 Key Design Decisions

1. **Separation of Concerns**
   - AuthController: Authentication only
   - UsersController: User management only

2. **Dependency Injection**
   - Services injected into controller
   - Loosely coupled components

3. **JWT-based Authentication**
   - Stateless authentication
   - Scalable across instances

4. **Role-Based Access Control**
   - Admin: Full access
   - User: Own data access
   - Guest: No access (401)

5. **Async/Await Pattern**
   - Non-blocking I/O
   - Better performance
   - Scalability

6. **Unified Response Format**
   - Consistent API contract
   - Easy client integration

7. **Comprehensive Logging**
   - Audit trail
   - Debugging support
   - Compliance

---

**Architecture Version**: 2.0  
**Last Updated**: November 17, 2025  
**Status**: ✅ Production Ready
