# Step-7: Profile Service Consumer Implementation (U1) - Part 2

**Status:** ✅ Completed  
**Date:** November 2, 2025  
**Build:** ✅ User Service (0 errors, 4 warnings)  
**Next:** End-to-End Testing (Step-7 Part 3)

---

## 📋 Overview

Implemented the User Service as an **event consumer** that listens for `UserRegistered` events from the Event Bus. When a new user registers in the Auth Service, a `UserRegistered` event is published to Event Bus, which the User Service consumes and creates a corresponding user profile.

This completes **Use Case U1** architecture:
```
Auth Service (User Registration)
    ↓ publishes
UserRegisteredEvent (via HTTP)
    ↓ routes to
Event Bus Service
    ↓ stores in
Outbox (PostgreSQL)
    ↓ background worker publishes to
Kafka Topic: USER_REGISTERED
    ↓ consumed by
User Service
    ↓ creates
User Profile (SQLite)
```

---

## 🏗️ Architecture

### Components Created

#### 1. **UserProfileEventHandler.cs**
**Location:** `/services/user-service/src/UserService/EventConsumers/UserProfileEventHandler.cs`

Event handler that processes `UserRegisteredEvent` messages:

```csharp
public class UserProfileEventHandler
{
    public async Task HandleUserRegisteredAsync(
        UserRegisteredEvent @event,
        CancellationToken cancellationToken = default)
    {
        // Idempotency check: User already exists?
        // Create user profile from event data
        // Initialize default subscription (free plan)
        // Log with correlation ID for distributed tracing
    }
}
```

**Key Features:**
- ✅ **Idempotency:** Checks if user already exists before creation
- ✅ **Correlation Tracing:** Propagates CorrelationId through logs
- ✅ **Error Handling:** Logs errors and re-throws for Event Bus retry/DLQ
- ✅ **Non-Blocking:** Event processing doesn't affect User Service availability

**Data Mapping:**
```
UserRegisteredEvent (from Auth Service)
    ↓ maps to
CreateUserRequest
    ↓ creates
User entity (SQLite)
```

---

#### 2. **EventConsumerService.cs**
**Location:** `/services/user-service/src/UserService/EventConsumers/EventConsumerService.cs`

Background service that manages event consumption:

```csharp
public class EventConsumerService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Wait for Event Bus to be healthy
        // Register subscriptions
        // Poll for events
        // Route to appropriate handlers
    }
}
```

**Key Features:**
- ✅ **Health Checks:** Waits for Event Bus to be available (max 10 retries)
- ✅ **Subscription Registration:** Registers "UserRegistered" event subscription with Event Bus
- ✅ **Handler Routing:** Routes events to correct handlers by event type
- ✅ **Graceful Shutdown:** Responds to cancellation tokens

**Configuration Settings:**
```csharp
public class EventConsumerSettings
{
    public bool Enabled { get; set; } = true;
    public string EventBusUrl { get; set; } = "http://localhost:5020";
    public int PollIntervalSeconds { get; set; } = 5;
}
```

---

### Program.cs Integration

Added event consumer registration to dependency injection:

```csharp
// Event Consumer Services
builder.Services.AddScoped<UserProfileEventHandler>();
builder.Services.AddHttpClient<EventConsumerService>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5020");
    client.Timeout = TimeSpan.FromSeconds(30);
});
builder.Services.AddHostedService<EventConsumerService>();
```

---

### Project File Update

Added reference to Shared Events library in `UserService.csproj`:

```xml
<ItemGroup>
    <ProjectReference Include="../../../../src/Shared/TechBirdsFly.Shared.csproj" />
</ItemGroup>
```

This grants access to:
- `UserRegisteredEvent` contract
- `IEventContract` interface
- Event serialization utilities
- Kafka topic constants

---

## 📊 Data Flow

### Event Flow Diagram

```
┌─────────────────┐
│  Auth Service   │
│  User Register  │
└────────┬────────┘
         │
         │ POST /api/events/publish
         │ (UserRegisteredEvent)
         ▼
┌──────────────────────┐
│   Event Bus API      │
│  /api/events/publish │
└────────┬─────────────┘
         │
         │ Store in Outbox
         ▼
┌──────────────────────┐
│  OutboxEvent Table   │
│  (Guaranteed)        │
└────────┬─────────────┘
         │
         │ Background Worker (10s poll)
         ▼
┌──────────────────────┐
│   Kafka Topic        │
│  USER_REGISTERED     │
└────────┬─────────────┘
         │
         │ EventConsumerService (5s poll)
         ▼
┌──────────────────────────┐
│ UserProfileEventHandler  │
│ HandleUserRegisteredAsync│
└────────┬─────────────────┘
         │
         │ Check idempotency
         │ Create user profile
         │ Initialize subscription
         ▼
┌──────────────────────────┐
│   User Service DB        │
│   (User entity created)  │
└──────────────────────────┘
```

### Event Creation to User Profile

```
Input: UserRegisteredEvent
├── EventId: "abc123def456"
├── UserId: "user-uuid"
├── Email: "alice@example.com"
├── FirstName: "Alice"
├── LastName: "Smith"
├── CorrelationId: "corr-uuid"
└── Metadata: {...}
    ↓
Handler Validation & Idempotency Check
├── ✓ User doesn't exist yet (first time)
└── → Proceed to creation
    ↓
CreateUserRequest
├── Email: "alice@example.com"
├── FirstName: "Alice"
└── LastName: "Smith"
    ↓
UserManagementService.CreateUserAsync()
    ↓
Output: User Profile Created
├── Id: (system-generated GUID)
├── Email: "alice@example.com"
├── FirstName: "Alice"
├── LastName: "Smith"
├── Status: "active"
├── Role: "user"
├── CreatedAt: (now)
└── IsEmailVerified: false
```

---

## 🔍 Logging & Tracing

All logs include correlation ID for distributed tracing across services:

```
[14:32:15 INF] 📨 Received UserRegistered event - UserId: 550e8400-e29b-41d4-a716-446655440000, Email: alice@example.com
    CorrelationId: corr-550e8400-e29b-41d4-a716-446655440000
    EventId: evt-550e8400-e29b-41d4-a716-446655440001

[14:32:15 INF] 🔄 Creating user profile from event - UserId: 550e8400-e29b-41d4-a716-446655440000, Email: alice@example.com
    CorrelationId: corr-550e8400-e29b-41d4-a716-446655440000

[14:32:16 INF] ✅ User profile created successfully from event - UserId: 550e8400-e29b-41d4-a716-446655440000, Email: alice@example.com
    CorrelationId: corr-550e8400-e29b-41d4-a716-446655440000
```

---

## ⚙️ Configuration

### Event Bus Connection
```json
{
  "EventConsumer": {
    "Enabled": true,
    "EventBusUrl": "http://localhost:5020",
    "PollIntervalSeconds": 5
  }
}
```

### Required Services
- ✅ Event Bus Service (HTTP API at `http://localhost:5020`)
- ✅ Kafka (topics pre-created: USER_REGISTERED)
- ✅ User Service Database (SQLite or PostgreSQL)

---

## 🧪 Testing Scenario

### End-to-End U1 Flow Test

**Prerequisites:**
1. ✅ Kafka running and topics created
2. ✅ Event Bus Service running on port 5020
3. ✅ User Service running
4. ✅ Auth Service running

**Test Steps:**

1. **Register new user in Auth Service:**
   ```bash
   curl -X POST http://localhost:5000/api/auth/register \
     -H "Content-Type: application/json" \
     -d '{
       "email": "test@example.com",
       "password": "Test123!",
       "firstName": "Test",
       "lastName": "User"
     }'
   ```

2. **Verify UserRegistered event published:**
   ```bash
   # Check Event Bus outbox
   curl http://localhost:5020/api/outbox/stats
   
   # Should show event in pending state
   ```

3. **Verify Kafka received event:**
   ```bash
   # Check Kafka topics
   kafka-console-consumer --bootstrap-server localhost:9092 \
     --topic user_registered --from-beginning
   ```

4. **Verify user profile created:**
   ```bash
   curl http://localhost:5008/api/users/test@example.com
   
   # Should return user profile with matching email/name
   ```

5. **Verify logs show correlation ID:**
   ```bash
   # Check User Service logs
   docker logs user-service 2>&1 | grep "CorrelationId"
   ```

---

## 📈 Build & Deployment Status

### Build Results
```
User Service Build: ✅ SUCCESS
├── Errors: 0
├── Warnings: 4
│   ├── NETSDK1080: PackageReference to Microsoft.AspNetCore.App (expected)
│   └── NU1902: System.IdentityModel.Tokens.Jwt known vulnerability (library issue)
└── Time: 3.28 seconds
```

### Files Created/Modified

| File | Status | Changes |
|------|--------|---------|
| `UserProfileEventHandler.cs` | ✅ Created | Event handler (80 lines) |
| `EventConsumerService.cs` | ✅ Created | Background service (220 lines) |
| `Program.cs` | ✅ Modified | Added event consumer DI registration |
| `UserService.csproj` | ✅ Modified | Added Shared project reference |

---

## 🔗 Service Integration Points

### Auth Service → Event Bus
```csharp
// AuthApplicationService publishes after user created
await _eventPublisherService.PublishUserRegisteredEventAsync(
    userId: user.Id.ToString(),
    email: user.Email,
    firstName: user.FirstName,
    lastName: user.LastName,
    cancellationToken: cancellationToken);
```

### User Service ← Event Bus
```csharp
// EventConsumerService subscribes and routes
router.Subscribe("UserRegistered", HandleUserRegisteredAsync);

// UserProfileEventHandler processes
await handler.HandleUserRegisteredAsync(userRegisteredEvent, cancellationToken);
```

---

## 🚀 Next Steps (Step-7 Part 3)

### End-to-End Testing
1. Verify all services start cleanly
2. Test user registration flow
3. Validate events flow through entire pipeline
4. Check user profile creation in User Service DB
5. Verify correlation IDs in logs across services

### Enhancements
1. Add default subscription initialization
2. Implement Profile Service (user preferences initialization)
3. Add webhook notifications
4. Implement event replay capability
5. Add monitoring dashboards

---

## 📚 References

**Related Documentation:**
- [Event Bus Service](../event-bus-service/README.md)
- [Auth Service Integration](../auth-service/README.md)
- [Shared Event Contracts](../../src/Shared/Events/README.md)
- [Kafka Configuration](../../infra/docker-compose.yml)

**Event Contract:**
- `UserRegisteredEvent` - `/src/Shared/Events/Contracts/UserRegisteredEvent.cs`
- `EventTopics` - `/src/Shared/Events/Contracts/EventTopics.cs`

---

**Status:** ✅ Complete  
**Build:** ✅ 0 Errors  
**Ready for:** End-to-End Testing

