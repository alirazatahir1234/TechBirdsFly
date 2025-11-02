using TechBirdsFly.Shared.Events.Contracts;
using System.Text.Json;
using Serilog.Context;

namespace UserService.EventConsumers;

/// <summary>
/// Background service that consumes events from Event Bus
/// Subscribes to UserRegistered and other relevant events
/// </summary>
public class EventConsumerService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EventConsumerService> _logger;
    private readonly HttpClient _httpClient;
    private const string EventBusHealthUrl = "http://localhost:5020/health";
    private const int PollIntervalSeconds = 5;

    public EventConsumerService(
        IServiceProvider serviceProvider,
        ILogger<EventConsumerService> logger,
        HttpClient httpClient)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <summary>
    /// Main execution loop for event consumption
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("🚀 User Service Event Consumer started");

        // Initial delay to allow startup
        await Task.Delay(3000, stoppingToken);

        try
        {
            // Verify Event Bus connectivity
            await WaitForEventBusAsync(stoppingToken);

            // Start consuming events
            await ConsumeEventsAsync(stoppingToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("⏸️ Event Consumer cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Fatal error in Event Consumer");
        }

        _logger.LogInformation("🛑 Event Consumer stopped");
    }

    /// <summary>
    /// Wait for Event Bus to become available
    /// </summary>
    private async Task WaitForEventBusAsync(CancellationToken cancellationToken, int maxRetries = 10)
    {
        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                var response = await _httpClient.GetAsync(EventBusHealthUrl, cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("✅ Event Bus is healthy and ready");
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⏳ Waiting for Event Bus... (Attempt {Attempt}/{MaxRetries})", i + 1, maxRetries);
            }

            if (i < maxRetries - 1)
                await Task.Delay(2000, cancellationToken);
        }

        throw new InvalidOperationException("Event Bus is not available after multiple retries");
    }

    /// <summary>
    /// Poll Event Bus for new events
    /// </summary>
    private async Task ConsumeEventsAsync(CancellationToken cancellationToken)
    {
        var subscriptionsUrl = "http://localhost:5020/api/subscriptions";

        // Register subscriptions for events this service is interested in
        await RegisterSubscriptionsAsync(subscriptionsUrl, cancellationToken);

        // Poll for events (simple polling pattern, could be enhanced with webhooks)
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogDebug("🔍 Polling for events...");

                // In a real implementation, you would:
                // 1. Query Event Bus for pending events for this service
                // 2. Process each event
                // 3. Mark as processed

                // For now, we've registered subscriptions and rely on Event Bus routing
                await Task.Delay(PollIntervalSeconds * 1000, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error during event polling");
                await Task.Delay(PollIntervalSeconds * 1000, cancellationToken);
            }
        }
    }

    /// <summary>
    /// Register event subscriptions with Event Bus
    /// </summary>
    private async Task RegisterSubscriptionsAsync(string subscriptionsUrl, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("📋 Registering event subscriptions with Event Bus");

            // Register UserRegistered event subscription
            await RegisterSubscriptionAsync(
                subscriptionsUrl,
                "UserRegistered",
                "user-service",
                "HandleUserRegistered",
                cancellationToken);

            _logger.LogInformation("✅ Event subscriptions registered");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "⚠️ Failed to register subscriptions (non-critical)");
            // Don't throw - service can still receive events via routing
        }
    }

    /// <summary>
    /// Register a single subscription
    /// </summary>
    private async Task RegisterSubscriptionAsync(
        string baseUrl,
        string eventType,
        string subscriberService,
        string handler,
        CancellationToken cancellationToken)
    {
        try
        {
            var subscriptionRequest = new
            {
                eventType,
                subscriberService,
                handler,
                isActive = true
            };

            var content = new StringContent(
                JsonSerializer.Serialize(subscriptionRequest),
                System.Text.Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync(
                $"{baseUrl}/subscribe",
                content,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "⚠️ Subscription registration returned {StatusCode} for {EventType}",
                    response.StatusCode,
                    eventType);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "⚠️ Error registering {EventType} subscription", eventType);
        }
    }

    /// <summary>
    /// Get the handler for an event type
    /// This is called internally when processing events
    /// </summary>
    public Func<IEventContract, CancellationToken, Task>? GetHandler(string eventType)
    {
        return eventType switch
        {
            "UserRegistered" => HandleUserRegisteredAsync,
            _ => null
        };
    }

    /// <summary>
    /// Handle UserRegistered event
    /// </summary>
    private async Task HandleUserRegisteredAsync(
        IEventContract @event,
        CancellationToken cancellationToken)
    {
        try
        {
            if (@event is not UserRegisteredEvent userRegisteredEvent)
            {
                _logger.LogWarning("Invalid event type for UserRegistered handler");
                return;
            }

            using var scope = _serviceProvider.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<UserProfileEventHandler>();

            await handler.HandleUserRegisteredAsync(userRegisteredEvent, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error handling UserRegistered event");
            throw;
        }
    }
}

/// <summary>
/// Configuration settings for event consumer
/// </summary>
public class EventConsumerSettings
{
    /// <summary>
    /// Enable/disable event consumption
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Event Bus base URL
    /// </summary>
    public string EventBusUrl { get; set; } = "http://localhost:5020";

    /// <summary>
    /// Poll interval in seconds
    /// </summary>
    public int PollIntervalSeconds { get; set; } = 5;
}
