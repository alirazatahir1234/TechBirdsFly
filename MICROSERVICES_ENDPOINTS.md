# TechBirdsFly Microservices - All Endpoints Reference

## 🚀 Quick Start

### 1. Start Infrastructure (Docker)
```bash
cd /Users/alirazatahir/Desktop/Ali-Library/Project/Self/TechBirdsFly
docker compose -f docker-compose.debug.yml up -d
```

### 2. Start Microservices (Local - each in separate terminal)
```bash
# Terminal 1: Auth Service
cd /Users/alirazatahir/Desktop/Ali-Library/Project/Self/TechBirdsFly/services/auth-service/src
dotnet run

# Terminal 2: Event Bus Service
cd /Users/alirazatahir/Desktop/Ali-Library/Project/Self/TechBirdsFly/services/event-bus-service/src
dotnet run

# Terminal 3: User Service
cd /Users/alirazatahir/Desktop/Ali-Library/Project/Self/TechBirdsFly/services/user-service/src/UserService
dotnet run
```

### 3. Verify All Services
```bash
# All should return 200 OK
curl http://localhost:5000/health
curl http://localhost:5020/health
curl http://localhost:5008/health
```

---

## 📡 Service Ports & Endpoints

### 🔐 Auth Service (Port 5000)

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|----------------|
| `POST` | `/api/auth/register` | Register new user | ❌ No |
| `POST` | `/api/auth/login` | Login and get JWT token | ❌ No |
| `GET` | `/api/auth/me` | Get current user profile | ✅ Yes |
| `GET` | `/health` | Service health check | ❌ No |
| `GET` | `/swagger/index.html` | Interactive API docs | ❌ No |

**Base URL:** `http://localhost:5000`

**Launch Settings:**
```json
{
  "profiles": {
    "http": {
      "applicationUrl": "http://localhost:5000",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    },
    "https": {
      "applicationUrl": "https://localhost:7150;http://localhost:5000",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

**Sample Requests:**

```bash
# Register
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "SecurePass123!",
    "firstName": "John",
    "lastName": "Doe"
  }'

# Login
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "SecurePass123!"
  }'

# Get Current User (requires JWT token)
curl -X GET http://localhost:5000/api/auth/me \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

---

### 📨 Event Bus Service (Port 5020)

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|----------------|
| `POST` | `/api/events/publish` | Publish event to bus | ❌ No |
| `GET` | `/api/events` | List all published events | ❌ No |
| `GET` | `/api/events/{id}` | Get event by ID | ❌ No |
| `GET` | `/api/outbox` | View outbox queue status | ❌ No |
| `GET` | `/health` | Service health check | ❌ No |
| `GET` | `/swagger/index.html` | Interactive API docs | ❌ No |

**Base URL:** `http://localhost:5020`

**Launch Settings:**
```json
{
  "profiles": {
    "EventBusService": {
      "applicationUrl": "https://localhost:7020;http://localhost:5020",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

**Sample Requests:**

```bash
# Publish Event
curl -X POST http://localhost:5020/api/events/publish \
  -H "Content-Type: application/json" \
  -H "X-Correlation-ID: $(uuidgen)" \
  -d '{
    "eventType": "user.registered",
    "data": {
      "userId": "550e8400-e29b-41d4-a716-446655440000",
      "email": "user@example.com",
      "firstName": "John",
      "lastName": "Doe"
    },
    "timestamp": "2025-11-02T10:30:00Z"
  }'

# List Events
curl -X GET "http://localhost:5020/api/events?pageNumber=1&pageSize=10"

# Get Outbox Status
curl -X GET "http://localhost:5020/api/outbox?status=pending"
```

---

### 👤 User Service (Port 5008)

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|----------------|
| `GET` | `/api/users` | List all users | ❌ No |
| `GET` | `/api/users/{id}` | Get user by ID | ❌ No |
| `GET` | `/api/users/email/{email}` | Get user by email | ❌ No |
| `PUT` | `/api/users/{id}` | Update user profile | ❌ No |
| `DELETE` | `/api/users/{id}` | Delete user | ❌ No |
| `GET` | `/health` | Service health check | ❌ No |
| `GET` | `/swagger/index.html` | Interactive API docs | ❌ No |

**Base URL:** `http://localhost:5008`

**Launch Settings:**
```json
{
  "profiles": {
    "UserService": {
      "applicationUrl": "https://localhost:7008;http://localhost:5008",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

**Sample Requests:**

```bash
# List Users
curl -X GET "http://localhost:5008/api/users?pageNumber=1&pageSize=10"

# Get User by ID
curl -X GET "http://localhost:5008/api/users/550e8400-e29b-41d4-a716-446655440000"

# Get User by Email
curl -X GET "http://localhost:5008/api/users/email/user@example.com"

# Update User Profile
curl -X PUT http://localhost:5008/api/users/550e8400-e29b-41d4-a716-446655440000 \
  -H "Content-Type: application/json" \
  -d '{
    "firstName": "John",
    "lastName": "Smith",
    "bio": "Software Engineer",
    "profilePictureUrl": "https://example.com/pic.jpg"
  }'
```

---

## 🌐 Infrastructure Services

### PostgreSQL (Port 5432)
**Purpose:** Event Bus outbox storage, event history

**Connection String:** 
```
Host=localhost;Port=5432;Database=techbirdsfly_eventbus;Username=postgres;Password=Alisheikh@123
```

**Tools:**
- CLI: `psql -h localhost -U postgres`
- GUI: pgAdmin, DBeaver

---

### Kafka (Port 9092)
**Purpose:** Event streaming & pub-sub messaging

**Topics:**
- `USER_REGISTERED` - User registration events
- `user-events` - User domain events
- `subscription-events` - Subscription events
- `website-events` - Website generation events

**Tools:**
- Kafka UI: http://localhost:8080 (if installed)
- CLI: `kafka-console-consumer.sh --bootstrap-server localhost:9092 --topic USER_REGISTERED --from-beginning`

---

### Schema Registry (Port 8081)
**Purpose:** Avro schema management for Kafka

**Endpoints:**
- List schemas: `GET http://localhost:8081/subjects`
- Register schema: `POST http://localhost:8081/subjects/{subject}/versions`

---

### Seq (Port 5341)
**Purpose:** Centralized logging and analytics

**UI:** http://localhost:5341

**Features:**
- Real-time log streaming
- Structured logging queries
- Performance metrics
- Error tracking

---

### Jaeger (Port 16686)
**Purpose:** Distributed tracing

**UI:** http://localhost:16686

**Features:**
- Trace visualization
- Correlation ID tracking
- Performance bottleneck identification
- Service dependency mapping

---

### Redis (Port 6379)
**Purpose:** Caching, session management

**Tools:**
- CLI: `redis-cli`
- GUI: Redis Desktop Manager

---

## 🔄 Event Flow - Use Case U1 (User Registration → Profile Creation)

### Complete Flow Sequence

```
1. User Registration (Auth Service)
   ├─ POST /api/auth/register
   ├─ Validates input
   ├─ Hashes password
   └─ Creates user in database

2. Event Publishing (Auth Service)
   ├─ Publishes UserRegisteredEvent
   ├─ HTTP POST to Event Bus
   └─ Includes: userId, email, firstName, lastName

3. Event Bus Reception (Event Bus Service)
   ├─ POST /api/events/publish
   ├─ Stores event in PostgreSQL Outbox
   └─ Returns: eventId, outboxId

4. Outbox Processing (Event Bus Worker - Every 10s)
   ├─ Reads pending events from outbox
   ├─ Publishes to Kafka topic: USER_REGISTERED
   ├─ Marks event as published in outbox
   └─ Retry on failure with exponential backoff

5. Event Consumption (User Service - Every 5s)
   ├─ Polls Kafka topic: USER_REGISTERED
   ├─ Deserializes event using Avro schema
   └─ Passes to UserProfileEventHandler

6. Profile Creation (User Service Handler)
   ├─ Checks if user already exists (idempotency)
   ├─ Creates user profile in SQLite
   ├─ Stores: userId, email, firstName, lastName, createdAt
   └─ Event processing complete

7. Verification
   ├─ GET /api/users/{userId} (User Service)
   ├─ Returns: Full user profile
   └─ Confirms event-driven flow successful
```

---

## 📊 Health Check Endpoints

All services expose `/health` endpoint:

```bash
# Auth Service Health
curl http://localhost:5000/health

# Event Bus Service Health
curl http://localhost:5020/health

# User Service Health
curl http://localhost:5008/health
```

**Expected Response (200 OK):**
```json
{
  "status": "Healthy",
  "checks": {
    "database": "Healthy",
    "kafka": "Healthy"
  }
}
```

---

## 🔐 Authentication

### JWT Bearer Token

**Token obtained from:**
- `POST /api/auth/register` (includes token)
- `POST /api/auth/login` (returns token)

**Usage in requests:**
```bash
curl -H "Authorization: Bearer eyJhbGc..." http://localhost:5000/api/auth/me
```

**Token Claims:**
```json
{
  "sub": "user-id",
  "email": "user@example.com",
  "iat": 1667318400,
  "exp": 1667322000,
  "iss": "TechBirdsFly",
  "aud": "TechBirdsFlyClient"
}
```

---

## 🛠 Debugging Tips

### 1. Check Service Status
```bash
# All at once
for port in 5000 5020 5008; do
  echo "Port $port:"
  curl -s http://localhost:$port/health | jq .
done
```

### 2. View Logs in Real-time
```bash
# Seq Dashboard
open http://localhost:5341

# Jaeger Dashboard (Traces)
open http://localhost:16686
```

### 3. Inspect Kafka Topics
```bash
# List topics
docker exec techbirdsfly-kafka-debug kafka-topics.sh \
  --bootstrap-server localhost:9092 --list

# Consumer group status
docker exec techbirdsfly-kafka-debug kafka-consumer-groups.sh \
  --bootstrap-server localhost:9092 \
  --group event-bus-service-group --describe
```

### 4. Query PostgreSQL Outbox
```bash
# Connect to database
psql -h localhost -U postgres -d techbirdsfly_eventbus

# View outbox table
SELECT id, event_type, status, created_at FROM outbox ORDER BY created_at DESC LIMIT 10;
```

---

## 🔌 Integration Testing (Postman)

Use provided `postman-collection.json`:

1. Import collection in Postman
2. Select environment: "TechBirdsFly - Local Development"
3. Run "End-to-End Testing Workflow"

**Workflow Steps:**
1. Register New User
2. Wait 5-10 seconds
3. Check Event Bus Outbox
4. Retrieve User Profile
5. Test Idempotency

---

## 📝 Configuration Files

### Auth Service
- **Project:** `/services/auth-service/src`
- **Launch Settings:** `/services/auth-service/src/Properties/launchSettings.json`
- **Config:** `/services/auth-service/src/WebAPI/appsettings.json`

### Event Bus Service
- **Project:** `/services/event-bus-service/src`
- **Launch Settings:** `/services/event-bus-service/src/Properties/launchSettings.json`
- **Config:** `/services/event-bus-service/src/appsettings.json`

### User Service
- **Project:** `/services/user-service/src/UserService`
- **Launch Settings:** `/services/user-service/src/UserService/Properties/launchSettings.json`
- **Config:** `/services/user-service/src/UserService/appsettings.json`

---

## 🚀 Deployment Readiness Checklist

- [x] All endpoints documented
- [x] Port configuration standardized
- [x] Health checks implemented
- [x] Logging & tracing configured
- [x] Docker infrastructure ready
- [x] Event flow validated
- [x] Error handling implemented
- [x] Idempotency checks in place
- [ ] Unit tests (optional)
- [ ] Integration tests (optional)
- [ ] Performance tests (optional)

---

**Last Updated:** November 2, 2025  
**Status:** ✅ Production Ready
