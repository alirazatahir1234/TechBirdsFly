# Postman Import & Setup Guide

## 📦 What's Included

This package contains two Postman files for testing TechBirdsFly microservices:

1. **postman-collection.json** - Complete API collection (60+ endpoints)
2. **postman-environment.json** - Environment variables for local development

---

## 🚀 Quick Start (2 minutes)

### Step 1: Import Collection

1. Open **Postman** (download from https://postman.com if needed)
2. Click **"Import"** button (top-left)
3. Select **postman-collection.json** from this folder
4. Click **"Import"**

✅ **Collection imported!** You'll see all endpoints organized by service.

---

### Step 2: Import Environment

1. Click the **"Environments"** icon (left sidebar)
2. Click **"Import"**
3. Select **postman-environment.json**
4. Click **"Import"**

✅ **Environment imported!** Now set it as active:

1. Look for the **environment dropdown** (top-right)
2. Select **"TechBirdsFly - Local Development"**

---

### Step 3: Verify Services Are Running

Before testing, ensure all services are running:

```bash
# Terminal 1: Auth Service (Port 5000)
cd /Users/alirazatahir/Desktop/Ali-Library/Project/Self/TechBirdsFly/services/auth-service/src
dotnet run --project AuthService.csproj

# Terminal 2: Event Bus Service (Port 5020)
cd /Users/alirazatahir/Desktop/Ali-Library/Project/Self/TechBirdsFly/services/event-bus-service/src
dotnet run --project EventBusService.csproj

# Terminal 3: User Service (Port 5008)
cd /Users/alirazatahir/Desktop/Ali-Library/Project/Self/TechBirdsFly/services/user-service/src/UserService
dotnet run --project UserService.csproj
```

---

## 📋 Collection Organization

### 1️⃣ **Auth Service (Port 5000)**
- ✅ Register New User
- ✅ Login User
- ✅ Get Current User Profile
- ✅ Health Check
- ✅ Swagger UI

**Key Feature:** Auto-publishes UserRegisteredEvent to Event Bus

---

### 2️⃣ **Event Bus Service (Port 5020)**
- ✅ Publish Event (Direct)
- ✅ Get Event by ID
- ✅ Get All Events
- ✅ Get Outbox Items (pending/published/failed)
- ✅ Health Check
- ✅ Swagger UI

**Key Feature:** Guaranteed delivery via PostgreSQL Outbox + Kafka

---

### 3️⃣ **User Service (Port 5008)**
- ✅ Get User Profile by ID
- ✅ Get All Users
- ✅ Get User by Email
- ✅ Update User Profile
- ✅ Health Check
- ✅ Swagger UI

**Key Feature:** Consumes USER_REGISTERED events and creates profiles

---

### 4️⃣ **End-to-End Testing Workflow**
Complete 5-step workflow demonstrating Use Case U1:

1. **Step 1:** Register New User (Auth Service)
2. **Step 2:** Wait for Event Processing (5-10 seconds)
3. **Step 3:** Check Event Bus Outbox Status
4. **Step 4:** Retrieve User Profile (User Service)
5. **Step 5:** Test Idempotency (Retry Registration)

---

## 🔄 Auto-Response Variables

Postman automatically captures these from API responses:

| Variable | Captured From | Used In |
|----------|---------------|---------|
| `auth_token` | Register/Login response | Authorization header |
| `user_id` | Register/Login response | Profile queries |
| `user_email` | Register response | Email queries |
| `event_id` | Event publish response | Event detail queries |
| `outbox_id` | Event publish response | Outbox tracking |

**Example Flow:**
1. Register → `user_id` captured
2. Swagger uses `{{user_id}}` in subsequent requests
3. No manual copy-paste needed! ✨

---

## 🧪 Testing Scenarios

### Scenario A: Quick Health Check
```
1. Auth Service Health → 200 OK
2. Event Bus Health → 200 OK
3. User Service Health → 200 OK
```

**Time:** ~5 seconds  
**Files:** No side effects

---

### Scenario B: User Registration → Profile Creation
```
1. Register user in Auth Service
2. Check Event Bus Outbox (should be "published")
3. Wait 5-10 seconds for event processing
4. Retrieve profile from User Service
5. Verify profile data matches registration
```

**Time:** ~20 seconds  
**Tests:** Complete end-to-end flow with data verification

---

### Scenario C: Event Publishing (Direct)
```
1. Manually publish UserRegisteredEvent to Event Bus
2. Check outbox status
3. Verify Kafka routing
4. Check User Service for profile creation
```

**Time:** ~15 seconds  
**Tests:** Event bus isolation without Auth Service

---

### Scenario D: Idempotency Test
```
1. Register user
2. Register same user again (should fail with 400)
3. Verify only one profile created in User Service
```

**Time:** ~15 seconds  
**Tests:** Duplicate prevention mechanisms

---

## 🔐 Authentication

### JWT Bearer Token
1. Use "Register New User" or "Login User" endpoint
2. Token is auto-captured in `auth_token` variable
3. Subsequent endpoints automatically include header:
   ```
   Authorization: Bearer {{auth_token}}
   ```

### Correlation Tracking
Every request includes unique correlation ID:
```
X-Correlation-ID: {{$guid}}
```

This flows through all services for distributed tracing.

---

## 🐛 Troubleshooting

### Error: `Cannot GET /health`
**Cause:** Service not running  
**Fix:**
```bash
# Check if service is running
curl http://localhost:5000/health

# If not, start it:
cd /services/auth-service/src
dotnet run --project AuthService.csproj
```

---

### Error: `401 Unauthorized`
**Cause:** Invalid or missing JWT token  
**Fix:**
1. Run "Register New User" request first
2. Check environment variable: `auth_token`
3. Use endpoints that don't require auth (Health, Swagger)

---

### Error: `404 Not Found` (User Profile)
**Cause:** Event not yet processed  
**Fix:**
1. Wait 10-15 seconds
2. Run "Get Outbox Items" to check status
3. Verify User Service is running
4. Check logs for EventConsumerService errors

---

### Error: `Redis Connection Failed`
**Cause:** Redis not running (non-critical)  
**Fix:** Health check currently requires Redis. Two options:

**Option A (Recommended for local dev):**
Edit appsettings.Development.json:
```json
{
  "ConnectionStrings": {
    "Redis": "localhost:6379,abortConnect=false"
  }
}
```

**Option B (Full Docker):**
```bash
cd /infra
docker compose up -d redis
```

---

## 📊 Sample Request Flow

### Request 1: Register User
```json
POST http://localhost:5000/api/auth/register

{
  "email": "john@example.com",
  "password": "SecurePass123!",
  "firstName": "John",
  "lastName": "Doe"
}

Response (201 Created):
{
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "email": "john@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "token": "eyJhbGc..."
  }
}
```

**Variables captured:**
- ✅ `auth_token` = "eyJhbGc..."
- ✅ `user_id` = "550e8400-e29b-41d4-a716-446655440000"
- ✅ `user_email` = "john@example.com"

---

### Request 2: Check User Profile (After ~10s)
```json
GET http://localhost:5008/api/users/550e8400-e29b-41d4-a716-446655440000

Response (200 OK):
{
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "email": "john@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "createdAt": "2025-11-02T10:30:00Z",
    "bio": null,
    "profilePictureUrl": null
  }
}
```

✅ **Profile created by event consumer!**

---

## 🎯 Key Endpoints by Use Case

| Use Case | Auth | Event Bus | User Service |
|----------|------|-----------|--------------|
| Register User | ✅ POST /api/auth/register | - | - |
| Get Token | ✅ POST /api/auth/login | - | - |
| Publish Event | - | ✅ POST /api/events/publish | - |
| Check Outbox | - | ✅ GET /api/outbox | - |
| Get Profile | - | - | ✅ GET /api/users/{id} |
| Track Flow | ✅ X-Correlation-ID | ✅ X-Correlation-ID | ✅ X-Correlation-ID |

---

## 🚀 Advanced Usage

### Custom Headers
Add custom headers to any request:
1. Click **"Headers"** tab
2. Add new header:
   ```
   X-Custom-Header: value
   ```

### Pre-Request Scripts
Modify requests before sending:
1. Click **"Pre-request Script"** tab
2. Example: Generate timestamp
   ```javascript
   pm.environment.set('timestamp', new Date().toISOString());
   ```

### Tests/Assertions
Verify responses automatically:
1. Click **"Tests"** tab
2. Example: Check status code
   ```javascript
   pm.test("Status is 200", () => {
     pm.response.to.have.status(200);
   });
   ```

---

## 📞 Support

For issues or questions:

1. Check **Postman Console** (View → Show Postman Console)
2. Review service logs in terminal windows
3. Verify all services are running on correct ports
4. Check **STEP7_END_TO_END_TEST_PLAN.md** for detailed documentation

---

## 📚 Related Documentation

- **PHASE3_3_PROFILE_SERVICE_CONSUMER.md** - Event consumer implementation
- **STEP7_END_TO_END_TEST_PLAN.md** - Comprehensive testing guide
- **QUICK_REFERENCE_STEP7.md** - Quick reference cheat sheet

---

**Happy Testing! 🚀**
