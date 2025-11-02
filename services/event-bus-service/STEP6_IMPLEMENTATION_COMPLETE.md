# Step-6: Consumer & Event Routing - COMPLETE ✅

## Overview
Implemented the complete **event consumption and routing infrastructure**. Events published to Kafka are now consumed by a background service and routed to event handlers based on event type, enabling the Event Bus to distribute events to interested services.

## What's Implemented

### 1. IKafkaConsumer Interface & Implementation (~130 lines)
**Location**: 
- Interface: `Application/Interfaces/IKafkaConsumer.cs`
- Implementation: `Infrastructure/Kafka/KafkaConsumer.cs`

**Purpose**: Abstraction for consuming messages from Kafka topics

**Key Features**:
- Subscribe to single or multiple topics
- Message deserialization using `EventFactory.CreateFromJson()`
- Automatic partition EOF handling (doesn't spam logs)
- Event header parsing
- Consumer statistics tracking
- Graceful shutdown with unsubscribe
- Comprehensive error handling

**Interface Methods**:
```csharp
Task SubscribeAsync(string topic, Func<IEventContract?, CancellationToken, Task> handler, ...)
Task SubscribeAsync(IEnumerable<string> topics, Func<IEventContract?, CancellationToken, Task> handler, ...)
string GetConsumerGroup()
```

**Consumer Configuration**:
- Consumer group: `event-bus-service-group` (from settings)
- Auto offset reset: `Earliest`
- Auto commit: `true`
- Session timeout: 6 seconds
- Statistics interval: 5 seconds

### 2. EventRouter Service (~150 lines)
**Location**: `Application/Services/EventRouter.cs`

**Purpose**: Routes incoming events to registered event handlers based on event type

**Pattern**: Publisher-Subscriber with type-based routing

**Key Features**:
- Register handlers by event type: `Subscribe("UserRegistered", handler)`
- Route events to all handlers: `RouteAsync(event)`
- Multiple handlers per event type supported
- Partial failure handling (continues if one handler fails)
- Handler count tracking and statistics
- Clear separation of concerns

**Interface**:
```csharp
void Subscribe(string eventType, EventHandlerDelegate handler)
Task<int> RouteAsync(IEventContract @event, CancellationToken cancellationToken)
IEnumerable<string> GetRegisteredEventTypes()
int GetHandlerCount(string eventType)
int GetTotalHandlerCount()
```

**Handler Delegate**:
```csharp
public delegate Task EventHandlerDelegate(IEventContract @event, CancellationToken cancellationToken);
```

### 3. EventConsumerService (~80 lines)
**Location**: `Application/Services/EventConsumerService.cs`

**Purpose**: Orchestrates Kafka consumption and routes events

**Key Features**:
- Connects Kafka consumer to event router
- Validates handlers registered before consumption
- Comprehensive event processing logging
- Error handling with logging context
- Retrieves consumer info (group, handlers, event types)

**Main Method**: `StartConsumingAsync(topics, cancellationToken)`
- Starts consuming from specified topics
- Logs all events received
- Routes to handlers via EventRouter
- Reports handler execution results

### 4. EventConsumerBackgroundService (~180 lines)
**Location**: `Infrastructure/BackgroundServices/EventConsumerBackgroundService.cs`

**Purpose**: ASP.NET Core `IHostedService` that runs event consumption continuously

**Key Features**:
- Startup delay (5 seconds default) for graceful initialization
- Can be disabled via configuration
- Automatic event handler registration
- Topic configuration or defaults to all 5 event types
- Graceful cancellation support

**Included Event Handlers** (placeholder implementations):
- `HandleUserRegisteredAsync` - New user registration
- `HandleUserUpdatedAsync` - User profile updates
- `HandleUserDeactivatedAsync` - User account deactivation
- `HandleSubscriptionStartedAsync` - New subscriptions
- `HandleWebsiteGeneratedAsync` - Website generation completion

**Default Topics** (if not configured):
```csharp
- user-registered
- user-updated
- user-deactivated
- subscription-started
- website-generated
```

**Settings DTO**: `EventConsumerBackgroundSettings`
```csharp
public class EventConsumerBackgroundSettings
{
    public bool Enabled { get; set; } = true;
    public int StartupDelaySeconds { get; set; } = 5;
    public List<string> Topics { get; set; } = new();
}
```

### 5. SubscriptionsController - Monitoring API (~200 lines)
**Location**: `WebAPI/Controllers/SubscriptionsController.cs`

**Purpose**: REST endpoints for monitoring subscriptions and routing state

**Endpoints**:

#### GET `/api/subscriptions/info`
- Returns detailed subscription information
- Shows all registered event types with handler counts
- Public endpoint (no auth)

**Response**:
```json
{
  "timestamp": "2025-11-02T15:35:00Z",
  "registeredEventTypes": [
    "UserRegistered",
    "UserUpdated",
    "SubscriptionStarted"
  ],
  "totalHandlers": 8,
  "eventHandlerCounts": {
    "UserRegistered": 3,
    "UserUpdated": 2,
    "SubscriptionStarted": 3
  }
}
```

#### GET `/api/subscriptions/{eventType}/handlers`
- Returns handler count for specific event type
- Shows subscription status

**Response**:
```json
{
  "eventType": "UserRegistered",
  "handlerCount": 3,
  "isSubscribed": true,
  "timestamp": "2025-11-02T15:35:00Z"
}
```

#### GET `/api/subscriptions/health`
- System health check endpoint
- Returns subscription system status

**Response**:
```json
{
  "status": "healthy",
  "service": "EventBusService - Subscriptions",
  "registeredEventTypes": ["UserRegistered", "UserUpdated"],
  "totalHandlers": 8,
  "timestamp": "2025-11-02T15:35:00Z",
  "version": "1.0.0"
}
```

### 6. Dependency Injection Updates
**Location**: `WebAPI/DI/ServiceCollectionExtensions.cs`

**Registrations Added**:
```csharp
// Router (singleton - shared state)
services.AddSingleton<EventRouter>();
services.AddScoped<EventConsumerService>();

// Kafka Consumer (singleton)
services.AddSingleton<IKafkaConsumer, KafkaConsumer>();

// Settings
services.AddSingleton(configuration
    .GetSection("EventConsumer")
    .Get<EventConsumerBackgroundSettings>() ?? new EventConsumerBackgroundSettings());

// Background Service
services.AddHostedService<EventConsumerBackgroundService>();
```

## Configuration (appsettings.json)

Add this configuration section:
```json
{
  "EventConsumer": {
    "Enabled": true,
    "StartupDelaySeconds": 5,
    "Topics": []
  }
}
```

If `Topics` is empty, uses default topics (all 5 event types).

## How It Works: Complete Event Flow

```
┌─────────────────────────────────────────────────────────┐
│  1. Event Published & Stored in Outbox                  │
│     POST /api/events/publish → PublishEventService      │
│     → OutboxEvent (IsPublished=false)                   │
└────────────────────┬────────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────────┐
│  2. Background Worker Publishes to Kafka                │
│     OutboxPublisherBackgroundService (every 10 sec)     │
│     → OutboxPublisherService.PublishPendingEventsAsync()│
│     → IKafkaProducer.PublishAsync()                     │
│     → Kafka Topic (e.g., user-registered)               │
└────────────────────┬────────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────────┐
│  3. Consumer Receives from Kafka                        │
│     EventConsumerBackgroundService (continuous)         │
│     → KafkaConsumer.SubscribeAsync()                    │
│     → Kafka message consumed                            │
│     → EventFactory.CreateFromJson()                     │
└────────────────────┬────────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────────┐
│  4. Event Routed to Handlers                            │
│     EventConsumerService.StartConsumingAsync()          │
│     → EventRouter.RouteAsync(event)                     │
│     → For each handler of event type:                   │
│        └─> Execute handler async                       │
│            (e.g., HandleUserRegisteredAsync)           │
└────────────────────┬────────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────────┐
│  5. Handler Executes Business Logic                     │
│     HandleUserRegisteredAsync(event)                    │
│     → Log event received                                │
│     → Execute handler logic                             │
│     → Return completion                                 │
│                                                         │
│     Future: Business logic here (create profile, etc.)  │
└─────────────────────────────────────────────────────────┘
```

## Architecture Diagram

```
┌─ Event Bus Service ──────────────────────────────────────┐
│                                                          │
│  ┌─────────────────── Kafka Infrastructure ────────────┐│
│  │                                                     ││
│  │  Topic: user-registered    Topic: user-updated    ││
│  │  Topic: user-deactivated   Topic: subscription... ││
│  │                                                     ││
│  └─────────────────┬──────────────────────────────────┘│
│                    │                                    │
│  ┌────────────────┘  (KafkaConsumer subscribes)       │
│  │                                                     │
│  │   EventConsumerBackgroundService                   │
│  │   ├─ StartupDelay: 5 sec                          │
│  │   ├─ Topics: [user-registered, user-updated, ...] │
│  │   └─ Continuous polling                           │
│  │                                                     │
│  │         ↓                                           │
│  │                                                     │
│  │   EventConsumerService                            │
│  │   └─ HandleEventAsync()                           │
│  │      └─ Calls EventRouter                         │
│  │                                                     │
│  │         ↓                                           │
│  │                                                     │
│  │   EventRouter                                      │
│  │   ├─ "UserRegistered" → [handler1, handler2]     │
│  │   ├─ "UserUpdated" → [handler3]                  │
│  │   └─ RouteAsync(event) → Execute all handlers    │
│  │                                                     │
│  │         ↓                                           │
│  │                                                     │
│  │   Event Handlers (TODO: implement business logic)  │
│  │   ├─ HandleUserRegisteredAsync                   │
│  │   ├─ HandleUserUpdatedAsync                      │
│  │   └─ ... more handlers                           │
│  │                                                     │
│  └─────────────────────────────────────────────────────┘
│                                                          │
│  ┌─ Monitoring APIs ────────────────────────────────────┐
│  │  GET /api/subscriptions/info                        │
│  │  GET /api/subscriptions/{eventType}/handlers        │
│  │  GET /api/subscriptions/health                      │
│  │                                                      │
│  └──────────────────────────────────────────────────────┘
│                                                          │
└──────────────────────────────────────────────────────────┘
```

## Key Design Patterns

### 1. Publisher-Subscriber with Type-Based Routing
- Services register handlers for event types
- Multiple subscribers per event type supported
- Decoupled event producers from consumers

### 2. Singleton EventRouter
- Shared routing state across all handlers
- Thread-safe handler registration and execution
- Single source of truth for subscriptions

### 3. Background Services for Event Consumption
- Non-blocking event processing
- Graceful startup/shutdown
- Configurable intervals and topics

### 4. Factory Pattern for Deserialization
- EventFactory handles JSON → Event object conversion
- Supports multiple event types
- Centralized deserialization logic

## Testing Event Flow

### 1. Check Subscriptions
```bash
curl http://localhost:5020/api/subscriptions/info | jq
```

### 2. Publish Test Event
```bash
curl -X POST http://localhost:5020/api/events/publish \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <token>" \
  -d '{
    "eventType": "UserRegistered",
    "eventData": "{...}"
  }'
```

### 3. Monitor Event Processing
```bash
# Check outbox
curl http://localhost:5020/api/outbox/stats | jq

# Check pending events (should become empty as they're published)
curl http://localhost:5020/api/outbox/pending | jq
```

### 4. Watch Application Logs
Look for:
- "🔔 Subscribing to topics:"
- "📨 Message consumed"
- "🔀 Routing event"
- "✅ Event handled by X handler(s)"

## Build Status
✅ **Build succeeded with 0 errors**

```
dotnet build (from /services/event-bus-service/src)
Output: Build succeeded.
Warnings: 10 (dependency vulnerabilities, no code warnings)
```

## Next Steps: Step-7 Auth-Service Integration

Now the event bus can consume and route events. Next:
1. Add UserRegistered event producer to Auth Service
2. Create Profile Service as event consumer
3. Test end-to-end Use Case U1: User Registration → Profile Creation

---

**Completion Time**: ~60 minutes
**Files Created**: 5 (KafkaConsumer, EventRouter, EventConsumerService, EventConsumerBackgroundService, SubscriptionsController)
**Files Modified**: 1 (ServiceCollectionExtensions)
**Lines of Code**: ~750 (including DTOs and documentation)
**Build Status**: ✅ Success
