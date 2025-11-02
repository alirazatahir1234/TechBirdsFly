# Step-4 Quick Start: Producer & Publish API

This guide shows how to implement the Producer & Publish API using the event contracts created in Step-3.

## 1. Create PublishEventRequest DTO

**File:** `/services/event-bus-service/src/Application/DTOs/PublishEventRequest.cs`

```csharp
namespace EventBusService.Application.DTOs;

/// <summary>
/// Request model for publishing an event
/// </summary>
public class PublishEventRequest
{
    /// <summary>
    /// Event type (e.g., "UserRegistered", "UserUpdated")
    /// </summary>
    public string EventType { get; set; } = null!;

    /// <summary>
    /// Event payload as JSON
    /// </summary>
    public string EventData { get; set; } = null!;

    /// <summary>
    /// Optional correlation ID for request tracing
    /// </summary>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// Optional metadata for the event
    /// </summary>
    public Dictionary<string, string>? Metadata { get; set; }
}
```

## 2. Create PublishEventResponse DTO

**File:** `/services/event-bus-service/src/Application/DTOs/PublishEventResponse.cs`

```csharp
namespace EventBusService.Application.DTOs;

/// <summary>
/// Response model for event publishing
/// </summary>
public class PublishEventResponse
{
    public bool Success { get; set; }
    public string EventId { get; set; } = null!;
    public string Message { get; set; } = null!;
    public long Timestamp { get; set; }
    public string? CorrelationId { get; set; }

    public static PublishEventResponse Created(
        string eventId,
        string correlationId)
    {
        return new PublishEventResponse
        {
            Success = true,
            EventId = eventId,
            Message = "Event published successfully and stored in outbox",
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            CorrelationId = correlationId
        };
    }

    public static PublishEventResponse Failed(string message)
    {
        return new PublishEventResponse
        {
            Success = false,
            EventId = string.Empty,
            Message = message,
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        };
    }
}
```

## 3. Create PublishEventService

**File:** `/services/event-bus-service/src/Application/Services/PublishEventService.cs`

```csharp
using TechBirdsFly.Shared.Events.Contracts;
using TechBirdsFly.Shared.Events.Serialization;
using EventBusService.Application.DTOs;
using EventBusService.Application.Interfaces;
using EventBusService.Domain.Entities;

namespace EventBusService.Application.Services;

/// <summary>
/// Service for publishing events to the outbox
/// </summary>
public class PublishEventService
{
    private readonly IOutboxEventRepository _outboxRepository;
    private readonly ILogger<PublishEventService> _logger;

    public PublishEventService(
        IOutboxEventRepository outboxRepository,
        ILogger<PublishEventService> logger)
    {
        _outboxRepository = outboxRepository;
        _logger = logger;
    }

    public async Task<PublishEventResponse> PublishEventAsync(
        PublishEventRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "Publishing event of type: {EventType}",
                request.EventType);

            // Deserialize event from JSON
            var @event = EventFactory.CreateFromJson(request.EventData);
            
            if (@event == null)
            {
                _logger.LogWarning(
                    "Failed to deserialize event of type: {EventType}",
                    request.EventType);
                return PublishEventResponse.Failed("Invalid event format");
            }

            // Validate event
            if (!@event.Validate(out var errors))
            {
                _logger.LogWarning(
                    "Event validation failed: {Errors}",
                    string.Join(", ", errors));
                return PublishEventResponse.Failed($"Validation failed: {string.Join(", ", errors)}");
            }

            // Set correlation ID if not present
            @event.CorrelationId = request.CorrelationId ?? @event.CorrelationId;

            // Add metadata if provided
            if (request.Metadata != null)
            {
                @event.Metadata ??= new();
                foreach (var (key, value) in request.Metadata)
                {
                    @event.Metadata[key] = value;
                }
            }

            // Wrap for Kafka
            var kafkaMessage = EventFactory.WrapForKafka(
                @event,
                partitionKey: @event.UserId);

            // Create outbox event
            var outboxEvent = new OutboxEvent
            {
                EventId = @event.EventId,
                EventType = @event.EventType,
                Topic = EventTopics.GetTopic(@event.EventType),
                Payload = EventSerializer.SerializeToJson(kafkaMessage),
                CreatedAt = DateTime.UtcNow,
                IsPublished = false,
                CorrelationId = @event.CorrelationId
            };

            // Store in outbox
            await _outboxRepository.AddAsync(outboxEvent, cancellationToken);

            _logger.LogInformation(
                "Event {EventId} of type {EventType} stored in outbox",
                @event.EventId,
                @event.EventType);

            return PublishEventResponse.Created(@event.EventId, @event.CorrelationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error publishing event of type: {EventType}",
                request.EventType);
            return PublishEventResponse.Failed($"Internal error: {ex.Message}");
        }
    }
}
```

## 4. Update ServiceCollectionExtensions

**File:** `/services/event-bus-service/src/WebAPI/DI/ServiceCollectionExtensions.cs`

Add the PublishEventService to DI:

```csharp
services.AddScoped<PublishEventService>();
```

## 5. Create EventsController

**File:** `/services/event-bus-service/src/WebAPI/Controllers/EventsController.cs`

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EventBusService.Application.DTOs;
using EventBusService.Application.Services;

namespace EventBusService.WebAPI.Controllers;

/// <summary>
/// API endpoints for event publishing
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class EventsController : ControllerBase
{
    private readonly PublishEventService _publishEventService;
    private readonly ILogger<EventsController> _logger;

    public EventsController(
        PublishEventService publishEventService,
        ILogger<EventsController> logger)
    {
        _publishEventService = publishEventService;
        _logger = logger;
    }

    /// <summary>
    /// Publish an event to the outbox
    /// </summary>
    /// <param name="request">Event to publish</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Event ID and confirmation</returns>
    /// <response code="200">Event successfully published</response>
    /// <response code="400">Invalid event format or validation error</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("publish")]
    [ProducesResponseType(typeof(PublishEventResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PublishEventResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PublishEventResponse>> PublishEvent(
        [FromBody] PublishEventRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            return BadRequest(PublishEventResponse.Failed("Request body is required"));
        }

        if (string.IsNullOrWhiteSpace(request.EventType))
        {
            return BadRequest(PublishEventResponse.Failed("EventType is required"));
        }

        if (string.IsNullOrWhiteSpace(request.EventData))
        {
            return BadRequest(PublishEventResponse.Failed("EventData is required"));
        }

        var response = await _publishEventService.PublishEventAsync(request, cancellationToken);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    /// <summary>
    /// Get health status of the event bus
    /// </summary>
    /// <returns>Service status</returns>
    [HttpGet("health")]
    [AllowAnonymous]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
    }
}
```

## 6. Test with cURL

Once the API is running (port 5030):

```bash
# Get JWT token first (from Auth Service)
TOKEN=$(curl -X POST http://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@techbirdsfly.com","password":"Admin@123"}' \
  | jq -r '.token')

# Publish UserRegistered event
curl -X POST http://localhost:5030/api/events/publish \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "eventType": "UserRegistered",
    "eventData": "{\"eventId\":\"evt-001\",\"userId\":\"user123\",\"email\":\"user@example.com\",\"firstName\":\"John\",\"lastName\":\"Doe\",\"timestamp":1700000000000,\"source\":\"auth-service\",\"eventType\":\"UserRegistered\",\"schemaVersion\":1,\"correlationId\":\"corr-001\"}",
    "correlationId": "corr-001",
    "metadata": {
      "ipAddress": "192.168.1.1",
      "source": "web"
    }
  }'
```

## 7. Key Integration Points

### From Event Contracts (Step-3):
- ✅ `EventFactory.CreateFromJson()` - Deserialize event JSON
- ✅ `@event.Validate()` - Validate event data
- ✅ `EventFactory.WrapForKafka()` - Prepare for Kafka
- ✅ `EventTopics.GetTopic()` - Get Kafka topic from event type
- ✅ `EventSerializer.SerializeToJson()` - Serialize for storage

### Outbox Pattern:
- ✅ `OutboxEvent` entity (already created in Domain)
- ✅ `IOutboxEventRepository` interface (already created)
- ✅ Store in database with `IsPublished = false`
- ✅ Background worker (Step-5) will publish and mark as published

## 8. Example Request Flow

```
1. Client calls POST /api/events/publish
   ↓
2. EventsController validates request
   ↓
3. PublishEventService:
   - Deserializes event JSON → IEventContract
   - Validates event (required fields, data types)
   - Sets correlation ID if missing
   - Merges metadata
   - Wraps in KafkaEventMessage
   ↓
4. Creates OutboxEvent:
   - Stores payload JSON in database
   - Sets IsPublished = false
   - Records CorrelationId for tracing
   ↓
5. Returns PublishEventResponse with EventId
   ↓
6. Background worker (Step-5):
   - Polls unpublished OutboxEvents
   - Publishes to Kafka via KafkaProducer
   - Marks as published
   ↓
7. Kafka consumers (Step-6):
   - Receive message
   - Deserialize via EventFactory
   - Route to handlers
   - Execute business logic
```

## 9. Configuration Needed

**appsettings.json** - Already configured, but verify:

```json
{
  "Jwt": {
    "ValidIssuer": "techbirdsfly",
    "ValidAudience": "techbirdsfly-api",
    "SecretKey": "your-secret-key-here-min-32-chars"
  },
  "Kafka": {
    "BootstrapServers": "localhost:9092",
    "ProducerId": "event-bus-service"
  },
  "Database": {
    "ConnectionString": "Server=localhost;Port=5432;Database=techbirdsfly_eventbus;User Id=postgres;Password=Alisheikh@123;"
  }
}
```

## 10. Next: Step-5 - Outbox Worker

Once this is working, implement the background worker that:
1. Queries unpublished OutboxEvents
2. Publishes to Kafka
3. Marks as published
4. Handles retries and poison messages

---

**Files to Create:**
- [ ] PublishEventRequest.cs (14 lines)
- [ ] PublishEventResponse.cs (42 lines)
- [ ] PublishEventService.cs (95 lines)
- [ ] EventsController.cs (95 lines)

**Time Estimate:** 30-45 minutes  
**Complexity:** Medium  
**Dependencies:** ✅ All satisfied

---

**Ready to proceed?** Run this command to verify Event Bus Service builds with the new service:

```bash
cd /path/to/event-bus-service/src && dotnet build
```
