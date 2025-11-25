# TechBirdsFly Auth Service - API Documentation

**Version**: 1.0.0  
**Last Updated**: November 17, 2025  
**Base URL**: `http://localhost:5001`

---

## 📋 Table of Contents

1. [Authentication Endpoints](#authentication-endpoints)
2. [User Profile Endpoints](#user-profile-endpoints)
3. [Health & Status](#health--status)
4. [Request/Response Examples](#requestresponse-examples)
5. [Error Handling](#error-handling)
6. [Postman Collection](#postman-collection)

---

## 🔐 Authentication Endpoints

### 1. Register User

**Endpoint**: `POST /api/auth/register`

**Description**: Create a new user account

**Request Headers**:
```
Content-Type: application/json
```

**Request Body**:
```json
{
  "email": "user@example.com",
  "password": "SecurePassword123!",
  "confirmPassword": "SecurePassword123!",
  "firstName": "John",
  "lastName": "Doe"
}
```

**Request Parameters**:
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `email` | string | ✅ | User email address (must be unique) |
| `password` | string | ✅ | Password (minimum 6 characters) |
| `confirmPassword` | string | ✅ | Must match password field |
| `firstName` | string | ✅ | User's first name |
| `lastName` | string | ✅ | User's last name |

**Response (201 Created)**:
```json
{
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "email": "user@example.com"
}
```

**Response (400 Bad Request)**:
```json
{
  "message": "Email is required"
}
```

**Response (409 Conflict)**:
```json
{
  "message": "Email already exists"
}
```

**Status Codes**:
- `201 Created` - User successfully registered
- `400 Bad Request` - Validation error (invalid email, weak password, etc.)
- `409 Conflict` - Email already exists

---

### 2. Login User

**Endpoint**: `POST /api/auth/login`

**Description**: Authenticate user and receive JWT tokens

**Request Headers**:
```
Content-Type: application/json
```

**Request Body**:
```json
{
  "email": "user@example.com",
  "password": "SecurePassword123!"
}
```

**Request Parameters**:
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `email` | string | ✅ | User email address |
| `password` | string | ✅ | User password |

**Response (200 OK)**:
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

**Response (400 Bad Request)**:
```json
{
  "message": "Email and password are required"
}
```

**Response (401 Unauthorized)**:
```json
{
  "message": "Invalid credentials"
}
```

**Status Codes**:
- `200 OK` - Authentication successful
- `400 Bad Request` - Missing email or password
- `401 Unauthorized` - Invalid email/password combination

---

### 3. Validate Token

**Endpoint**: `POST /api/auth/validate-token`

**Description**: Validate JWT token and check cache status

**Request Headers**:
```
Content-Type: application/json
```

**Request Body**:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

**Request Parameters**:
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `token` | string | ✅ | JWT token to validate |

**Response (200 OK)**:
```json
{
  "valid": true,
  "fromCache": false
}
```

**Response (200 OK - Cached)**:
```json
{
  "valid": true,
  "fromCache": true
}
```

**Response (400 Bad Request)**:
```json
{
  "message": "Token is required"
}
```

**Status Codes**:
- `200 OK` - Token validation result (see response body for valid/invalid)
- `400 Bad Request` - Token is required

---

### 4. Logout User

**Endpoint**: `POST /api/auth/logout`

**Description**: Logout user and invalidate session

**Query Parameters**:
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `email` | string | ✅ | User email address |

**Request URL**:
```
POST /api/auth/logout?email=user@example.com
```

**Response (200 OK)**:
```json
{
  "message": "Logged out"
}
```

**Response (400 Bad Request)**:
```json
{
  "message": "Email is required"
}
```

**Status Codes**:
- `200 OK` - Successfully logged out (idempotent)
- `400 Bad Request` - Email is required

---

## 👤 User Profile Endpoints

### 1. Get User Profile

**Endpoint**: `GET /api/auth/profile/{userId}`

**Description**: Retrieve user profile information

**Path Parameters**:
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `userId` | UUID | ✅ | User ID (from register/login response) |

**Request Headers** (Optional):
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Request URL**:
```
GET /api/auth/profile/550e8400-e29b-41d4-a716-446655440000
```

**Response (200 OK)**:
```json
{
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "email": "user@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "emailConfirmed": false,
  "isActive": true
}
```

**Response (404 Not Found)**:
```json
{
  "message": "User not found"
}
```

**Status Codes**:
- `200 OK` - User profile retrieved
- `404 Not Found` - User does not exist

---

### 2. Confirm Email

**Endpoint**: `POST /api/auth/confirm-email/{userId}`

**Description**: Mark user's email as confirmed

**Path Parameters**:
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `userId` | UUID | ✅ | User ID |

**Request URL**:
```
POST /api/auth/confirm-email/550e8400-e29b-41d4-a716-446655440000
```

**Response (200 OK)**:
```json
{
  "message": "Email confirmed"
}
```

**Response (404 Not Found)**:
```json
{
  "message": "User not found"
}
```

**Status Codes**:
- `200 OK` - Email confirmed successfully
- `404 Not Found` - User not found

---

### 3. Deactivate Account

**Endpoint**: `POST /api/auth/deactivate/{userId}`

**Description**: Deactivate user account and clear cache

**Path Parameters**:
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `userId` | UUID | ✅ | User ID |

**Request URL**:
```
POST /api/auth/deactivate/550e8400-e29b-41d4-a716-446655440000
```

**Response (200 OK)**:
```json
{
  "message": "Account deactivated"
}
```

**Response (404 Not Found)**:
```json
{
  "message": "User not found"
}
```

**Status Codes**:
- `200 OK` - Account deactivated successfully
- `404 Not Found` - User not found

---

## 💚 Health & Status

### Health Check

**Endpoint**: `GET /health`

**Description**: Check Auth Service health and database connectivity

**Request URL**:
```
GET /health
```

**Response (200 OK)**:
```
Healthy
```

**Response (503 Service Unavailable)**:
```
Unhealthy
```

**Status Codes**:
- `200 OK` - Service is healthy
- `503 Service Unavailable` - Service or database is down

---

## 📝 Request/Response Examples

### Complete Registration & Login Flow

#### Step 1: Register User
```bash
curl -X POST http://localhost:5001/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "john.doe@example.com",
    "password": "SecurePass123!",
    "confirmPassword": "SecurePass123!",
    "firstName": "John",
    "lastName": "Doe"
  }'
```

**Response**:
```json
{
  "userId": "123e4567-e89b-12d3-a456-426614174000",
  "email": "john.doe@example.com"
}
```

#### Step 2: Login
```bash
curl -X POST http://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "john.doe@example.com",
    "password": "SecurePass123!"
  }'
```

**Response**:
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjNlNDU2Ny1lODliLTEyZDMtYTQ1Ni00MjY2MTQxNzQwMDAiLCJuYW1lIjoiSm9obiBEb2UiLCJpYXQiOjE1MTYyMzkwMjJ9.eoaDVGTClbum59PlMK4rnKc_TNiUi2gagRB_-5kIiGM",
  "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjNlNDU2Ny1lODliLTEyZDMtYTQ1Ni00MjY2MTQxNzQwMDAiLCJleHAiOjE1MTYzMjUwMjJ9.K7wGWMtqBl"
}
```

#### Step 3: Get Profile
```bash
curl -X GET http://localhost:5001/api/auth/profile/123e4567-e89b-12d3-a456-426614174000 \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

**Response**:
```json
{
  "userId": "123e4567-e89b-12d3-a456-426614174000",
  "email": "john.doe@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "emailConfirmed": false,
  "isActive": true
}
```

---

## ⚠️ Error Handling

### Common Error Responses

#### 400 Bad Request
```json
{
  "message": "Validation error message"
}
```

#### 401 Unauthorized
```json
{
  "message": "Invalid credentials"
}
```

#### 404 Not Found
```json
{
  "message": "User not found"
}
```

#### 409 Conflict
```json
{
  "message": "Email already exists"
}
```

#### 500 Internal Server Error
```json
{
  "message": "An unexpected error occurred"
}
```

---

## 📦 Postman Collection

### Import Instructions

1. **Download the collection file**:
   - File: `AuthService-API.postman_collection.json`
   - Location: `services/auth-service/`

2. **Download the environment file**:
   - File: `AuthService-Environment.postman_environment.json`
   - Location: `services/auth-service/`

3. **Import in Postman**:
   - Open Postman
   - Click `Import` (top-left)
   - Select both files
   - Click `Import`

4. **Set Environment**:
   - Click environment selector (top-right)
   - Select `TechBirdsFly Auth Service - Local`
   - Variables will auto-populate after each request

### Available Variables

| Variable | Default | Type | Auto-Populated |
|----------|---------|------|---|
| `baseUrl` | http://localhost:5001 | string | ❌ |
| `userId` | (empty) | string | ✅ Register/Login |
| `userEmail` | (empty) | string | ✅ Register/Login |
| `accessToken` | (empty) | string | ✅ Login |
| `refreshToken` | (empty) | string | ✅ Login |

---

## 🚀 Quick Start

### Workflow Example

1. **Register a new user**
   ```
   POST /api/auth/register
   ```

2. **Login to get tokens**
   ```
   POST /api/auth/login
   ```

3. **Retrieve user profile**
   ```
   GET /api/auth/profile/{userId}
   ```

4. **Validate token**
   ```
   POST /api/auth/validate-token
   ```

5. **Logout**
   ```
   POST /api/auth/logout
   ```

---

## 📞 Support

For issues or questions about the Auth Service API:
- Check the logs at: `services/auth-service/logs/`
- Review the application configuration: `services/auth-service/src/appsettings.json`
- Run tests: `dotnet test` in `services/auth-service/tests/`

---

**Generated on**: November 17, 2025  
**Service Status**: ✅ Running on http://localhost:5001
