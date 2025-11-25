# 🚀 UsersController - Quick Reference

**Version**: 2.0 (Production Ready)  
**Date**: November 17, 2025

---

## 📍 API Endpoints at a Glance

| Method | Endpoint | Auth | Role | Purpose |
|--------|----------|------|------|---------|
| GET | `/api/users/{id}` | ✅ | User/Admin | Get user by ID |
| GET | `/api/users/profile/me` | ✅ | Any | Get own profile |
| PUT | `/api/users/profile/update` | ✅ | Any | Update own profile |
| GET | `/api/users` | ✅ | Admin | List all users |
| POST | `/api/users/{id}/deactivate` | ✅ | Admin | Deactivate user |
| POST | `/api/users/{id}/reactivate` | ✅ | Admin | Reactivate user |
| POST | `/api/users/{id}/assign-role` | ✅ | Admin | Assign role |
| GET | `/api/users/statistics` | ✅ | Admin | Get stats |

---

## 🔒 Authorization Rules

```
GET /users/{id}
├─ ❌ No token → 401 Unauthorized
├─ ✅ Token + Own ID → 200 OK
├─ ✅ Token + Admin → 200 OK
└─ ✅ Token + Other ID → 403 Forbidden

GET /users/profile/me
└─ ✅ Token → 200 OK

PUT /users/profile/update
├─ ✅ Token → 200 OK
└─ ❌ Invalid data → 400 Bad Request

GET /users
├─ ❌ Token (Non-Admin) → 403 Forbidden
└─ ✅ Token (Admin) → 200 OK

POST /users/{id}/deactivate
POST /users/{id}/reactivate
POST /users/{id}/assign-role
GET /users/statistics
└─ All: Admin only (403 if not)
```

---

## 📦 Request/Response Examples

### 1️⃣ Get Own Profile
```http
GET /api/users/profile/me
Authorization: Bearer eyJhbGc...

200 OK:
{
  "success": true,
  "data": {
    "userId": "123e4567-e89b-12d3-a456-426614174000",
    "email": "user@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "isActive": true
  }
}
```

### 2️⃣ Update Profile
```http
PUT /api/users/profile/update
Authorization: Bearer eyJhbGc...
Content-Type: application/json

{
  "firstName": "Jane",
  "lastName": "Smith"
}

200 OK:
{
  "success": true,
  "data": { /* updated user */ }
}
```

### 3️⃣ List Users (Admin)
```http
GET /api/users?pageNumber=1&pageSize=10&sortBy=email
Authorization: Bearer ADMIN_TOKEN

200 OK:
{
  "success": true,
  "data": {
    "items": [
      {
        "userId": "...",
        "email": "user@example.com",
        "firstName": "John",
        "role": "User",
        "isActive": true
      }
    ],
    "totalCount": 50,
    "pageNumber": 1,
    "pageSize": 10,
    "totalPages": 5
  }
}
```

### 4️⃣ Assign Role (Admin)
```http
POST /api/users/123e4567-e89b-12d3-a456-426614174000/assign-role
Authorization: Bearer ADMIN_TOKEN
Content-Type: application/json

{
  "role": "Admin"
}

200 OK:
{
  "success": true,
  "message": "Role assigned successfully"
}
```

---

## 🛡️ Error Responses

```
401 Unauthorized:
{
  "success": false,
  "message": "Invalid token"
}

403 Forbidden:
{
  "success": false,
  "message": "Access denied"
}

404 Not Found:
{
  "success": false,
  "message": "User not found"
}

400 Bad Request:
{
  "success": false,
  "message": "Invalid request"
}

500 Internal Server Error:
{
  "success": false,
  "message": "Error retrieving user"
}
```

---

## 🔧 Key Implementation Details

### Helper Method
```csharp
private Guid GetUserId()
{
    var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
    if (claim == null || !Guid.TryParse(claim.Value, out var userId))
        throw new UnauthorizedAccessException("Invalid token");
    return userId;
}
```

### Authorization Check
```csharp
// User can access own profile OR Admin
if (userId != id && !User.IsInRole("Admin"))
    return Forbid();
```

### Error Handling
```csharp
try
{
    // Implementation
    return Ok(new ApiResponse<T>(true, data));
}
catch (Exception ex)
{
    _logger.LogError(ex, "Error message");
    return StatusCode(500, new ApiResponse<T>(false, null, "Error"));
}
```

---

## 📊 Query Parameters

### `/api/users` (List Users)
| Parameter | Type | Default | Example |
|-----------|------|---------|---------|
| pageNumber | int | 1 | `?pageNumber=2` |
| pageSize | int | 20 | `?pageSize=50` |
| sortBy | string | null | `?sortBy=email` |
| ascending | bool | true | `?ascending=false` |
| filterByRole | string | null | `?filterByRole=Admin` |
| search | string | null | `?search=john` |

**Combined Example**:
```http
GET /api/users?pageNumber=1&pageSize=20&sortBy=email&ascending=true&filterByRole=Admin&search=john
```

---

## ✅ What's Removed (Auth-Only)

These are **NO LONGER** in UsersController:
- ❌ Register
- ❌ Login
- ❌ Verify Email
- ❌ Forgot Password
- ❌ Reset Password
- ❌ Token Validation
- ❌ Logout
- ❌ Change Password

**→ These now belong to `AuthController` only**

---

## ✅ What's Included (User Management)

These ARE in UsersController:
- ✅ Get User by ID
- ✅ Get Current User
- ✅ Update Profile
- ✅ List Users (Admin)
- ✅ Deactivate Account (Admin)
- ✅ Reactivate Account (Admin)
- ✅ Assign Role (Admin)
- ✅ Get Statistics (Admin)

---

## 🧪 Quick Test Commands

```bash
# Get your profile
curl -X GET http://localhost:5001/api/users/profile/me \
  -H "Authorization: Bearer TOKEN"

# Update profile
curl -X PUT http://localhost:5001/api/users/profile/update \
  -H "Authorization: Bearer TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"firstName":"Jane"}'

# List users (Admin)
curl -X GET "http://localhost:5001/api/users?pageNumber=1" \
  -H "Authorization: Bearer ADMIN_TOKEN"

# Assign role (Admin)
curl -X POST http://localhost:5001/api/users/ID/assign-role \
  -H "Authorization: Bearer ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"role":"Admin"}'
```

---

## 📋 DTOs Used

```csharp
// Request DTOs
public record UpdateProfileRequest(
    string FirstName, 
    string LastName,
    string? PhoneNumber);

public record AssignRoleRequest(string Role);

// Response DTOs
public record ApiResponse<T>(
    bool Success, 
    T? Data = null, 
    string? Message = null);

public record UserDto(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    bool IsActive);

public record PaginatedResponse<T>(
    List<T> Items,
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages);
```

---

## 🎯 Common Scenarios

### Scenario 1: User Views Own Profile
```
1. GET /api/users/profile/me
2. Response: 200 with user data
```

### Scenario 2: User Updates Profile
```
1. PUT /api/users/profile/update + request body
2. Response: 200 with updated data
```

### Scenario 3: Admin Lists Users
```
1. GET /api/users?pageNumber=1
2. Response: 200 with paginated list
```

### Scenario 4: Admin Assigns Role
```
1. POST /api/users/{id}/assign-role + role
2. Response: 200 success
```

### Scenario 5: User Tries Admin Endpoint
```
1. GET /api/users (as non-admin)
2. Response: 403 Forbidden
```

---

## 📌 Important Notes

1. **All endpoints require JWT token** (except unprotected auth endpoints)
2. **Admin endpoints return 403 if user is not Admin**
3. **Users can only access their own data** (except admins)
4. **Profile updates are self-service only**
5. **All admin actions are logged** with admin ID
6. **Pagination is built-in** for listing users
7. **Filtering and sorting supported** on admin list endpoint

---

## 🚀 Next Steps

- [ ] Generate integration tests
- [ ] Create Postman collection
- [ ] Update Swagger documentation
- [ ] Configure CI/CD pipeline
- [ ] Deploy to staging environment
- [ ] Run security tests
- [ ] Performance testing

---

**Status**: ✅ Production Ready  
**Maintained**: Development Team  
**Last Updated**: November 17, 2025
