# Event Contracts Guide

This directory contains the shared event contracts and schemas used across all microservices in the TechBirdsFly platform. All events follow the **IEventContract** interface and are serialized using Avro schemas for versioning and schema registry integration.

## Directory Structure

```
Events/
├── Contracts/
│   ├── IEventContract.cs           # Base interface for all events
│   ├── UserRegisteredEvent.cs      # Event for new user registration (U1)
│   ├── DomainEvents.cs             # Additional events (UserUpdated, UserDeactivated, etc.)
│   ├── KafkaEventMessage.cs        # Kafka envelope wrapper
│   ├── EventTopics.cs              # Topic constants and routing
│   ├── EventFactory.cs             # Factory for creating events
│   └── README.md                   # This file
├── Schemas/
│   ├── UserRegistered.avsc         # Avro schema for UserRegistered
│   ├── UserUpdated.avsc            # Avro schema for UserUpdated
│   ├── UserDeactivated.avsc        # Avro schema for UserDeactivated
│   ├── SubscriptionStarted.avsc    # Avro schema for SubscriptionStarted
│   └── WebsiteGenerated.avsc       # Avro schema for WebsiteGenerated
└── Serialization/
    └── EventSerializer.cs          # JSON serialization utilities
```

## Event Contracts

### 1. UserRegisteredEvent

**Topic:** `user-registered`  
**Source:** Auth Service  
**Use Case:** U1 - User registration triggers profile creation and welcome email

**Properties:**
- `UserId` - Unique user identifier
- `Email` - User's email address
- `FirstName` - User's first name
- `LastName` - User's last name
- `CorrelationId` - Request tracing ID
- `Timestamp` - Event creation time (Unix milliseconds)

**Usage:**
```csharp
var @event = EventFactory.CreateUserRegistered(
    userId: "user123",
    email: "user@example.com",
    firstName: "John",
    lastName: "Doe"
);

// Validate
if (!@event.Validate(out var errors))
{
    errors.ForEach(e => logger.LogError(e));
    return;
}

// Wrap for Kafka
var kafkaMessage = EventFactory.WrapForKafka(@event);
```

### 2. UserUpdatedEvent

**Topic:** `user-updated`  
**Source:** Auth Service  
**Trigger:** User profile changes (name, email, etc.)

**Properties:**
- `UserId` - User identifier
- `Email` - Updated email
- `FirstName` - Updated first name
- `LastName` - Updated last name

### 3. UserDeactivatedEvent

**Topic:** `user-deactivated`  
**Source:** Auth Service  
**Trigger:** User account deactivation

**Properties:**
- `UserId` - User identifier
- `Reason` - Optional deactivation reason

### 4. SubscriptionStartedEvent

**Topic:** `subscription-started`  
**Source:** Billing Service  
**Trigger:** New subscription purchase

**Properties:**
- `UserId` - User identifier
- `SubscriptionId` - Unique subscription ID
- `Plan` - Plan type (basic, pro, enterprise)
- `Price` - Subscription price

### 5. WebsiteGeneratedEvent

**Topic:** `website-generated`  
**Source:** Generator Service  
**Trigger:** AI-generated website created

**Properties:**
- `UserId` - User identifier
- `WebsiteId` - Generated website ID
- `WebsiteName` - Website name
- `Url` - Public URL of website

## IEventContract Interface

All events implement this interface:

```csharp
public interface IEventContract
{
    string EventId { get; set; }
    string EventType { get; }
    int SchemaVersion { get; }
    long Timestamp { get; set; }
    string Source { get; set; }
    string? CorrelationId { get; set; }
    string? CausationId { get; set; }
    string? UserId { get; }
    Dictionary<string, string>? Metadata { get; set; }
}
```

**Key Properties:**
- `EventId` - Unique event identifier (GUID)
- `EventType` - Event type name (e.g., "UserRegistered")
- `SchemaVersion` - Schema version for evolution
- `Timestamp` - Event creation time (Unix milliseconds)
- `Source` - Originating service name
- `CorrelationId` - Correlates related events
- `CausationId` - Event that caused this event
- `UserId` - User associated with event
- `Metadata` - Custom key-value pairs for extensibility

## EventTopics Constants

`EventTopics` class provides centralized management of topics and event types:

```csharp
// Topic constants (4 domains × 4 events each)
public const string USER_EVENTS = "user-events";
public const string USER_REGISTERED = "user-registered";
public const string USER_UPDATED = "user-updated";
public const string USER_DEACTIVATED = "user-deactivated";

public const string SUBSCRIPTION_EVENTS = "subscription-events";
public const string SUBSCRIPTION_STARTED = "subscription-started";
public const string SUBSCRIPTION_ENDED = "subscription-ended";
public const string SUBSCRIPTION_UPGRADED = "subscription-upgraded";

public const string WEBSITE_EVENTS = "website-events";
public const string WEBSITE_GENERATED = "website-generated";
public const string WEBSITE_PUBLISHED = "website-published";
public const string WEBSITE_DELETED = "website-deleted";

public const string BILLING_EVENTS = "billing-events";
public const string PAYMENT_PROCESSED = "payment-processed";
public const string INVOICE_CREATED = "invoice-created";

// Routing methods
public static string GetEventType(string topic);      // Topic → EventType
public static string GetTopic(string eventType);      // EventType → Topic
public static string[] GetDomainTopics(string domain); // Get all topics in domain
```

## KafkaEventMessage Envelope

Wraps events with Kafka metadata:

```csharp
public class KafkaEventMessage
{
    public string MessageId { get; set; }              // Unique message ID
    public object Event { get; set; }                   // Serialized event
    public string EventType { get; set; }              // Event type name
    public int SchemaVersion { get; set; }             // Schema version
    public long CreatedAt { get; set; }                // Creation timestamp
    public string Source { get; set; }                 // Source service
    public string? PartitionKey { get; set; }          // Kafka partition key
    public string? CorrelationId { get; set; }         // Request tracing
    public Dictionary<string, string> Headers { get; } // Custom headers
}
```

**Usage:**
```csharp
var kafkaMessage = KafkaEventMessage.FromEvent(userEvent, partitionKey: userId);

// Add custom headers
kafkaMessage.Headers["x-request-id"] = requestId;
kafkaMessage.Headers["x-user-agent"] = userAgent;
```

## EventSerializer Utilities

JSON serialization with support for polymorphic event types:

```csharp
// Serialize to JSON
var json = EventSerializer.SerializeToJson(userEvent);

// Deserialize from JSON
var @event = EventSerializer.DeserializeFromJson<UserRegisteredEvent>(json);

// Convert to/from dictionaries (useful for Kafka message parsing)
var dict = EventSerializer.ObjectToDictionary(userEvent);
var @event = EventSerializer.DictionaryToObject<UserRegisteredEvent>(dict);

// Pretty-printed JSON
var prettyJson = EventSerializer.SerializeToJsonPretty(userEvent);
```

## EventFactory Helper

Simplified factory for creating and wrapping events:

```csharp
// Create events with defaults
var userEvent = EventFactory.CreateUserRegistered(
    userId: "user123",
    email: "user@example.com",
    firstName: "John",
    lastName: "Doe",
    metadata: new() { ["ipAddress"] = "192.168.1.1" }
);

// Wrap for Kafka
var kafkaMessage = EventFactory.WrapForKafka(
    userEvent,
    partitionKey: "user123",
    headers: new() { ["trace-id"] = traceId }
);

// Deserialize from JSON
var @event = EventFactory.CreateFromJson(jsonString);

// Deserialize from dictionary
var @event = EventFactory.CreateFromDictionary(dictionary);
```

## Avro Schemas

All events are registered in Schema Registry with Avro format:

```bash
# Upload schema to Schema Registry
curl -X POST http://localhost:8081/subjects/user-registered-value/versions \
  -H "Content-Type: application/vnd.schemaregistry.v1+json" \
  -d @src/Shared/Events/Schemas/UserRegistered.avsc
```

**Schema Features:**
- Versioned for backward/forward compatibility
- Default values for optional fields
- Metadata documentation
- Map-type support for extensibility

## Best Practices

### 1. Event Creation

✅ **Do:**
```csharp
// Use factory methods with optional parameters
var @event = UserRegisteredEvent.Create(userId, email, firstName, lastName);

// Set correlation ID for tracing
@event.CorrelationId = correlationId;

// Add custom metadata
@event.Metadata = new() { ["source"] = "web" };
```

❌ **Don't:**
```csharp
// Don't use new() directly - use factory methods
var @event = new UserRegisteredEvent { ... };

// Don't forget to set EventId and Timestamp
```

### 2. Event Publishing

✅ **Do:**
```csharp
// Validate before publishing
if (!@event.Validate(out var errors))
{
    logger.LogError("Event validation failed: {Errors}", string.Join(", ", errors));
    return;
}

// Wrap for Kafka with partition key
var kafkaMessage = EventFactory.WrapForKafka(@event, partitionKey: userId);

// Use Outbox pattern for transactional safety
await outboxRepository.AddAsync(kafkaMessage);
await context.SaveChangesAsync();
```

❌ **Don't:**
```csharp
// Don't publish directly without validation
kafkaProducer.PublishAsync(kafkaMessage);

// Don't forget partition key (required for ordering)
var kafkaMessage = KafkaEventMessage.FromEvent(@event);
```

### 3. Event Consumption

✅ **Do:**
```csharp
// Deserialize with error handling
if (KafkaEventMessage.TryDeserialize(message, out var kafkaMessage))
{
    var @event = EventFactory.CreateFromJson(kafkaMessage.Event);
    if (@event != null)
    {
        await HandleEvent(@event);
    }
}

// Track correlation ID for distributed tracing
var correlationId = kafkaMessage.CorrelationId;
```

❌ **Don't:**
```csharp
// Don't assume deserialization succeeds
var kafkaMessage = JsonSerializer.Deserialize<KafkaEventMessage>(message);
var @event = EventFactory.CreateFromJson(kafkaMessage.Event);
```

### 4. Extending Events

✅ **Do:**
```csharp
// Add optional metadata instead of modifying events
@event.Metadata = new()
{
    ["customField"] = "value"
};

// Use factory methods for new event types
var newEvent = EventFactory.CreateCustomEvent(...);
```

❌ **Don't:**
```csharp
// Don't modify existing event contracts
public class UserRegisteredEvent
{
    public string CustomField { get; set; } // ❌ Breaking change
}
```

## Event Topics Map

| Domain | Topic | Event Type | Source |
|--------|-------|-----------|--------|
| User | `user-events` | UserRegistered | Auth Service |
| | `user-events` | UserUpdated | Auth Service |
| | `user-events` | UserDeactivated | Auth Service |
| Subscription | `subscription-events` | SubscriptionStarted | Billing Service |
| | `subscription-events` | SubscriptionEnded | Billing Service |
| | `subscription-events` | SubscriptionUpgraded | Billing Service |
| Website | `website-events` | WebsiteGenerated | Generator Service |
| | `website-events` | WebsitePublished | Generator Service |
| | `website-events` | WebsiteDeleted | Generator Service |
| Billing | `billing-events` | PaymentProcessed | Billing Service |
| | `billing-events` | InvoiceCreated | Billing Service |

## Schema Registry Integration

Schemas are automatically registered with versioning:

```csharp
// Consumers use schema registry for deserialization
var schemaRegistry = new CachedSchemaRegistryClient(
    new SchemaRegistryConfig { Url = "http://localhost:8081" }
);

var deserializer = new AvroDeserializer<UserRegisteredEvent>(schemaRegistry);
var @event = await deserializer.DeserializeAsync(message);
```

## Related Documentation

- **Event Bus Service:** `/services/event-bus-service/README.md`
- **Kafka Topics:** `/infra/DOCKER_SETUP.md#kafka-topics`
- **Outbox Pattern:** `/services/event-bus-service/src/Domain/Entities/OutboxEvent.cs`
- **Event Bus API:** `/services/event-bus-service/README.md#api-endpoints`

## Testing Events

```csharp
[Fact]
public void UserRegisteredEvent_ShouldValidate()
{
    // Arrange
    var @event = EventFactory.CreateUserRegistered(
        "user123", "user@example.com", "John", "Doe"
    );

    // Act
    var isValid = @event.Validate(out var errors);

    // Assert
    Assert.True(isValid);
    Assert.Empty(errors);
}

[Fact]
public void UserRegisteredEvent_ShouldSerialize()
{
    // Arrange
    var @event = EventFactory.CreateUserRegistered(
        "user123", "user@example.com", "John", "Doe"
    );

    // Act
    var json = EventSerializer.SerializeToJson(@event);
    var deserialized = EventSerializer.DeserializeFromJson<UserRegisteredEvent>(json);

    // Assert
    Assert.NotNull(deserialized);
    Assert.Equal(@event.UserId, deserialized.UserId);
    Assert.Equal(@event.Email, deserialized.Email);
}
```

## Questions?

Refer to `/services/event-bus-service/README.md` for Event Bus Service architecture and API documentation.
