# Event Contracts Usage Patterns

Quick reference for using the event contracts created in Phase 3 across the entire platform.

## 1. Creating Events

### Pattern 1A: Using EventFactory (Recommended)
```csharp
using TechBirdsFly.Shared.Events.Contracts;

// Create with all parameters
var userEvent = EventFactory.CreateUserRegistered(
    userId: "user123",
    email: "user@example.com",
    firstName: "John",
    lastName: "Doe",
    correlationId: correlationId,
    metadata: new() 
    { 
        ["ipAddress"] = "192.168.1.1",
        ["userAgent"] = "Mozilla/5.0..."
    }
);
```

### Pattern 1B: Using Direct Factory Method
```csharp
// Event factory methods with defaults
var @event = UserRegisteredEvent.Create(
    userId: "user123",
    email: "user@example.com",
    firstName: "John",
    lastName: "Doe",
    correlationId: Request.Headers["X-Correlation-ID"]
);

@event.Metadata = new() { ["source"] = "web-api" };
```

---

## 2. Publishing Events to Outbox (Auth Service Example)

### Pattern 2A: In Auth Service (Producer)
```csharp
public class UserService
{
    private readonly AuthDbContext _dbContext;
    private readonly ILogger<UserService> _logger;

    public async Task<Result> RegisterUserAsync(RegisterRequest request)
    {
        // Create user in database
        var user = new User { /* ... */ };
        _dbContext.Users.Add(user);
        
        // Create event
        var @event = EventFactory.CreateUserRegistered(
            userId: user.Id,
            email: user.Email,
            firstName: user.FirstName,
            lastName: user.LastName,
            correlationId: HttpContext.TraceIdentifier
        );

        // Validate event
        if (!@event.Validate(out var errors))
        {
            _logger.LogError("Event validation failed: {Errors}", errors);
            return Result.Failure("Event creation failed");
        }

        // Wrap for Kafka
        var kafkaMessage = EventFactory.WrapForKafka(
            @event,
            partitionKey: user.Id
        );

        // Create OutboxEvent in Auth Service database
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

        _dbContext.OutboxEvents.Add(outboxEvent);
        
        // All changes (user + event) saved atomically
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation(
            "User {UserId} registered, event {EventId} queued for publishing",
            user.Id,
            @event.EventId);

        return Result.Success();
    }
}
```

---

## 3. Validating Events

### Pattern 3: Event Validation
```csharp
using TechBirdsFly.Shared.Events.Contracts;

public class EventValidator
{
    public bool ValidateAndLog(IEventContract @event, ILogger logger)
    {
        // Built-in validation
        if (!@event.Validate(out var errors))
        {
            foreach (var error in errors)
            {
                logger.LogWarning("Event validation error: {Error}", error);
            }
            return false;
        }

        // Additional checks
        if (string.IsNullOrEmpty(@event.CorrelationId))
        {
            logger.LogWarning(
                "Event {EventId} missing correlation ID",
                @event.EventId);
            return false;
        }

        logger.LogInformation(
            "Event {EventId} of type {EventType} validated successfully",
            @event.EventId,
            @event.EventType);

        return true;
    }
}
```

---

## 4. Consuming Events from Kafka (Consumer Service Example)

### Pattern 4A: Profile Service (Consumer)
```csharp
using TechBirdsFly.Shared.Events.Contracts;

public class ProfileService
{
    private readonly IKafkaConsumer _consumer;
    private readonly ProfileDbContext _dbContext;
    private readonly ILogger<ProfileService> _logger;

    public async Task HandleUserRegisteredAsync(
        KafkaEventMessage kafkaMessage,
        CancellationToken cancellationToken)
    {
        try
        {
            // Deserialize event
            var @event = EventFactory.CreateFromJson(
                kafkaMessage.Event.ToString()
            ) as UserRegisteredEvent;

            if (@event == null)
            {
                _logger.LogError(
                    "Failed to deserialize UserRegistered event: {EventId}",
                    kafkaMessage.MessageId);
                return;
            }

            _logger.LogInformation(
                "Processing UserRegistered event: {EventId}, UserId: {UserId}",
                @event.EventId,
                @event.UserId);

            // Create user profile
            var profile = new UserProfile
            {
                UserId = @event.UserId,
                Email = @event.Email,
                FirstName = @event.FirstName,
                LastName = @event.LastName,
                CreatedAt = DateTime.UtcNow,
                CorrelationId = @event.CorrelationId
            };

            _dbContext.UserProfiles.Add(profile);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Profile created for user {UserId}, event {EventId}",
                @event.UserId,
                @event.EventId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error handling UserRegistered event: {EventId}",
                kafkaMessage.MessageId);
            throw;
        }
    }
}
```

### Pattern 4B: Email Service (Consumer)
```csharp
public class EmailService
{
    private readonly IEmailClient _emailClient;
    private readonly ILogger<EmailService> _logger;

    public async Task HandleUserRegisteredAsync(
        UserRegisteredEvent @event,
        CancellationToken cancellationToken)
    {
        try
        {
            var emailContent = $@"
                Welcome {event.FirstName}!
                
                Your account has been created with email: {@event.Email}
                
                Best regards,
                TechBirdsFly Team
            ";

            await _emailClient.SendAsync(
                to: @event.Email,
                subject: "Welcome to TechBirdsFly",
                body: emailContent,
                cancellationToken: cancellationToken
            );

            _logger.LogInformation(
                "Welcome email sent to {Email}, event {EventId}",
                @event.Email,
                @event.EventId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to send welcome email for user {UserId}",
                @event.UserId);
            throw;
        }
    }
}
```

---

## 5. Kafka Consumer Registration

### Pattern 5: Consumer Group Setup
```csharp
using TechBirdsFly.Shared.Events.Contracts;

public static class KafkaConsumerExtensions
{
    public static IServiceCollection AddKafkaConsumers(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var kafkaConfig = configuration.GetSection("Kafka");

        services.AddSingleton(provider =>
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = kafkaConfig["BootstrapServers"],
                GroupId = kafkaConfig["ConsumerGroupId"],
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false
            };

            return new ConsumerBuilder<string, string>(config).Build();
        });

        // Register consumer handlers
        services.AddScoped<UserRegisteredEventHandler>();
        services.AddScoped<WebsiteGeneratedEventHandler>();

        // Register hosted service for consumer loop
        services.AddHostedService<KafkaConsumerService>();

        return services;
    }
}

public class KafkaConsumerService : BackgroundService
{
    private readonly IKafkaConsumer _consumer;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<KafkaConsumerService> _logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Subscribe to topics
        _consumer.Subscribe(new[]
        {
            EventTopics.USER_REGISTERED,
            EventTopics.WEBSITE_GENERATED
        });

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = _consumer.Consume(
                    TimeSpan.FromSeconds(1),
                    stoppingToken
                );

                if (result != null)
                {
                    var kafkaMessage = JsonSerializer.Deserialize<KafkaEventMessage>(
                        result.Message.Value
                    );

                    var @event = EventFactory.CreateFromJson(
                        kafkaMessage?.Event.ToString() ?? ""
                    );

                    if (@event is UserRegisteredEvent userEvent)
                    {
                        using var scope = _serviceProvider.CreateScope();
                        var handler = scope.ServiceProvider
                            .GetRequiredService<UserRegisteredEventHandler>();
                        await handler.HandleAsync(userEvent);
                    }

                    _consumer.Commit(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Kafka consumer");
            }
        }

        _consumer.Close();
    }
}
```

---

## 6. Event Routing

### Pattern 6: Dynamic Event Routing
```csharp
using TechBirdsFly.Shared.Events.Contracts;

public class EventRouter
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EventRouter> _logger;

    public async Task RouteEventAsync(
        IEventContract @event,
        CancellationToken cancellationToken)
    {
        var topic = EventTopics.GetTopic(@event.EventType);
        
        _logger.LogInformation(
            "Routing event {EventId} of type {EventType} on topic {Topic}",
            @event.EventId,
            @event.EventType,
            topic);

        try
        {
            var handlers = @event switch
            {
                UserRegisteredEvent userEvent =>
                    await HandleUserRegisteredAsync(userEvent, cancellationToken),
                
                UserUpdatedEvent userEvent =>
                    await HandleUserUpdatedAsync(userEvent, cancellationToken),
                
                WebsiteGeneratedEvent websiteEvent =>
                    await HandleWebsiteGeneratedAsync(websiteEvent, cancellationToken),
                
                _ =>
                    throw new InvalidOperationException(
                        $"No handler for event type: {@event.EventType}")
            };

            _logger.LogInformation(
                "Event {EventId} routed successfully to {Handlers} handlers",
                @event.EventId,
                handlers);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error routing event {EventId}",
                @event.EventId);
            throw;
        }
    }

    private async Task<int> HandleUserRegisteredAsync(
        UserRegisteredEvent @event,
        CancellationToken cancellationToken)
    {
        var profileService = _serviceProvider
            .GetRequiredService<ProfileService>();
        var emailService = _serviceProvider
            .GetRequiredService<EmailService>();

        await profileService.HandleUserRegisteredAsync(@event, cancellationToken);
        await emailService.HandleUserRegisteredAsync(@event, cancellationToken);

        return 2;
    }

    private async Task<int> HandleUserUpdatedAsync(
        UserUpdatedEvent @event,
        CancellationToken cancellationToken)
    {
        var notificationService = _serviceProvider
            .GetRequiredService<NotificationService>();

        await notificationService.NotifyUserUpdatedAsync(@event, cancellationToken);
        return 1;
    }

    private async Task<int> HandleWebsiteGeneratedAsync(
        WebsiteGeneratedEvent @event,
        CancellationToken cancellationToken)
    {
        var notificationService = _serviceProvider
            .GetRequiredService<NotificationService>();

        await notificationService.NotifyWebsiteGeneratedAsync(@event, cancellationToken);
        return 1;
    }
}
```

---

## 7. REST Webhook Delivery

### Pattern 7: Webhook Publisher
```csharp
using TechBirdsFly.Shared.Events.Contracts;

public class WebhookPublisher
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<WebhookPublisher> _logger;

    public async Task PublishToSubscribersAsync(
        IEventContract @event,
        IEnumerable<EventSubscription> subscriptions,
        CancellationToken cancellationToken)
    {
        var payload = EventSerializer.SerializeToJson(@event);
        var tasks = subscriptions.Select(sub =>
            PublishToWebhookAsync(sub, payload, @event, cancellationToken)
        );

        await Task.WhenAll(tasks);
    }

    private async Task PublishToWebhookAsync(
        EventSubscription subscription,
        string payload,
        IEventContract @event,
        CancellationToken cancellationToken)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, subscription.WebhookUrl)
            {
                Content = new StringContent(
                    payload,
                    Encoding.UTF8,
                    "application/json")
            };

            // Add trace headers
            request.Headers.Add("X-Event-ID", @event.EventId);
            request.Headers.Add("X-Event-Type", @event.EventType);
            request.Headers.Add("X-Correlation-ID", @event.CorrelationId ?? "");

            var response = await _httpClient.SendAsync(
                request,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"Webhook failed: {response.StatusCode}");
            }

            _logger.LogInformation(
                "Event {EventId} delivered to webhook: {WebhookUrl}",
                @event.EventId,
                subscription.WebhookUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to deliver event {EventId} to webhook: {WebhookUrl}",
                @event.EventId,
                subscription.WebhookUrl);

            // Update subscription with failure reason
            subscription.FailureReason = ex.Message;
            subscription.FailureCount++;
        }
    }
}
```

---

## 8. Testing Events

### Pattern 8: Unit Tests
```csharp
using Xunit;
using TechBirdsFly.Shared.Events.Contracts;

public class UserRegisteredEventTests
{
    [Fact]
    public void Create_WithValidData_SuccessfullyCreates()
    {
        // Arrange
        var userId = "user123";
        var email = "user@example.com";
        var firstName = "John";
        var lastName = "Doe";

        // Act
        var @event = EventFactory.CreateUserRegistered(
            userId, email, firstName, lastName);

        // Assert
        Assert.NotEmpty(@event.EventId);
        Assert.Equal(userId, @event.UserId);
        Assert.Equal(email, @event.Email);
        Assert.True(@event.Validate(out _));
    }

    [Fact]
    public void Validate_WithMissingEmail_ReturnsFalse()
    {
        // Arrange
        var @event = new UserRegisteredEvent
        {
            EventId = "evt-001",
            UserId = "user123",
            Email = "", // Invalid: empty email
            FirstName = "John",
            LastName = "Doe",
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        };

        // Act
        var isValid = @event.Validate(out var errors);

        // Assert
        Assert.False(isValid);
        Assert.NotEmpty(errors);
    }

    [Fact]
    public void Serialize_AndDeserialize_MaintainsData()
    {
        // Arrange
        var original = EventFactory.CreateUserRegistered(
            "user123", "user@example.com", "John", "Doe");

        // Act
        var json = EventSerializer.SerializeToJson(original);
        var deserialized = EventSerializer
            .DeserializeFromJson<UserRegisteredEvent>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(original.UserId, deserialized.UserId);
        Assert.Equal(original.Email, deserialized.Email);
        Assert.Equal(original.FirstName, deserialized.FirstName);
        Assert.Equal(original.LastName, deserialized.LastName);
    }

    [Fact]
    public void KafkaWrapping_IncludesPartitionKey()
    {
        // Arrange
        var @event = EventFactory.CreateUserRegistered(
            "user123", "user@example.com", "John", "Doe");

        // Act
        var kafkaMessage = EventFactory.WrapForKafka(@event);

        // Assert
        Assert.Equal("user123", kafkaMessage.PartitionKey);
        Assert.Equal(@event.EventId, kafkaMessage.MessageId);
    }
}
```

---

## 9. Integration Workflow (U1 Use Case)

### Pattern 9: Complete U1 Flow
```csharp
// Step 1: User registers (Auth Service)
var user = await authService.RegisterAsync(
    email: "user@example.com",
    password: "Password@123"
);

// Event is created and stored in Outbox
// ✓ UserRegisteredEvent created
// ✓ OutboxEvent stored (IsPublished=false)
// ✓ Atomic transaction (user + event)

// Step 2: Background worker publishes to Kafka
// ✓ OutboxPublisher polls OutboxEvents
// ✓ Publishes to Kafka topic: "user-registered"
// ✓ Marks OutboxEvent as published (IsPublished=true)

// Step 3: Profile Service consumes event
// ✓ Kafka Consumer receives message
// ✓ EventFactory deserializes UserRegisteredEvent
// ✓ ProfileService.HandleUserRegisteredAsync called
// ✓ User profile created

// Step 4: Email Service consumes event
// ✓ Same Kafka topic, different consumer
// ✓ EventFactory deserializes UserRegisteredEvent
// ✓ EmailService.HandleUserRegisteredAsync called
// ✓ Welcome email sent

// Result:
// 1. User account created
// 2. User profile created
// 3. Welcome email sent
// All triggered by single event with guaranteed delivery
```

---

## 10. Error Handling Pattern

### Pattern 10: Robust Event Handling
```csharp
public class RobustEventHandler
{
    private readonly ILogger<RobustEventHandler> _logger;
    private readonly int _maxRetries = 3;

    public async Task SafeHandleEventAsync(
        IEventContract @event,
        Func<IEventContract, CancellationToken, Task> handler,
        CancellationToken cancellationToken)
    {
        var attempt = 0;

        while (attempt < _maxRetries)
        {
            try
            {
                // Validate event
                if (!@event.Validate(out var errors))
                {
                    _logger.LogError(
                        "Event {EventId} failed validation: {Errors}",
                        @event.EventId,
                        string.Join(", ", errors));
                    return; // Don't retry validation errors
                }

                // Handle event
                await handler(@event, cancellationToken);

                _logger.LogInformation(
                    "Event {EventId} handled successfully",
                    @event.EventId);
                return;
            }
            catch (Exception ex) when (attempt < _maxRetries - 1)
            {
                attempt++;
                _logger.LogWarning(
                    "Event {EventId} handling failed (attempt {Attempt}/{MaxRetries}): {Message}",
                    @event.EventId,
                    attempt,
                    _maxRetries,
                    ex.Message);

                // Exponential backoff
                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)));
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Event {EventId} handling failed after {MaxRetries} attempts",
                    @event.EventId,
                    _maxRetries);
                throw; // Poison message - manual intervention needed
            }
        }
    }
}
```

---

## Summary of Patterns

| Pattern | Use Case | Benefits |
|---------|----------|----------|
| **1A: EventFactory** | Creating events | Type-safe, defaults included |
| **2A: Auth Service Producer** | Publishing events | Atomic with user creation |
| **3: Validation** | Ensuring data quality | Error collection for feedback |
| **4A/B: Consumers** | Processing events | Decoupled, scalable |
| **5: Consumer Registration** | Kafka setup | Organized consumer groups |
| **6: Event Routing** | Dynamic dispatch | Flexible handler mapping |
| **7: Webhooks** | External notifications | REST-based integration |
| **8: Testing** | Quality assurance | Comprehensive coverage |
| **9: U1 Workflow** | End-to-end flow | Real-world scenario |
| **10: Error Handling** | Resilience | Retry logic, poison handling |

---

**For questions:** See `/src/Shared/Events/README.md` or `/services/event-bus-service/README.md`
