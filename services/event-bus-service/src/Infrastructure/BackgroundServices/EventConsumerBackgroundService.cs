using EventBusService.Application.Services;
using TechBirdsFly.Shared.Events.Contracts;

namespace EventBusService.Infrastructure.BackgroundServices;

/// <summary>
/// Background service that continuously consumes events from Kafka
/// Runs independently and processes events as they arrive
/// </summary>
public class EventConsumerBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EventConsumerBackgroundService> _logger;
    private readonly EventConsumerBackgroundSettings _settings;

    public EventConsumerBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<EventConsumerBackgroundService> logger,
        EventConsumerBackgroundSettings settings)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
    }

    /// <summary>
    /// Main execution loop
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "🚀 Event Consumer Background Service started - Enabled: {Enabled}",
            _settings.Enabled);

        // Initial delay before starting
        await Task.Delay(_settings.StartupDelaySeconds * 1000, stoppingToken);

        if (!_settings.Enabled)
        {
            _logger.LogWarning("⏸️ Event Consumer is disabled (set Enabled=true in configuration)");
            return;
        }

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var consumerService = scope.ServiceProvider.GetRequiredService<EventConsumerService>();
            var eventRouter = scope.ServiceProvider.GetRequiredService<EventRouter>();

            // Register event handlers
            RegisterEventHandlers(eventRouter);

            // Get topics from settings
            var topics = _settings.Topics?.Count > 0
                ? _settings.Topics
                : GetDefaultTopics();

            _logger.LogInformation(
                "📋 Starting consumption from topics: {Topics}",
                string.Join(", ", topics));

            // Start consuming
            await consumerService.StartConsumingAsync(topics, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("⏸️ Event Consumer Background Service cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Fatal error in Event Consumer Background Service");
        }

        _logger.LogInformation("🛑 Event Consumer Background Service stopped");
    }

    /// <summary>
    /// Register event handlers for all supported event types
    /// This is where you wire up what happens for each event type
    /// </summary>
    private void RegisterEventHandlers(EventRouter router)
    {
        _logger.LogInformation("📝 Registering event handlers");

        // Example handlers - customize based on your requirements
        // These are no-op handlers for now, replace with actual business logic

        router.Subscribe("UserRegistered", HandleUserRegisteredAsync);
        router.Subscribe("UserUpdated", HandleUserUpdatedAsync);
        router.Subscribe("UserDeactivated", HandleUserDeactivatedAsync);
        router.Subscribe("SubscriptionStarted", HandleSubscriptionStartedAsync);
        router.Subscribe("WebsiteGenerated", HandleWebsiteGeneratedAsync);

        _logger.LogInformation(
            "✅ Event handlers registered - Total: {Count}",
            router.GetTotalHandlerCount());
    }

    /// <summary>
    /// Get default topics if not configured
    /// </summary>
    private static List<string> GetDefaultTopics()
    {
        return new List<string>
        {
            EventTopics.USER_REGISTERED,
            EventTopics.USER_UPDATED,
            EventTopics.USER_DEACTIVATED,
            EventTopics.SUBSCRIPTION_STARTED,
            EventTopics.WEBSITE_GENERATED
        };
    }

    #region Event Handlers

    private async Task HandleUserRegisteredAsync(
        TechBirdsFly.Shared.Events.Contracts.IEventContract @event,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "👤 Handling UserRegistered event - EventId: {EventId}",
            @event.EventId);

        // TODO: Implement business logic for user registration
        // Examples:
        // - Create user profile
        // - Send welcome email
        // - Initialize user preferences
        // - Create default dashboard

        await Task.CompletedTask;
    }

    private async Task HandleUserUpdatedAsync(
        TechBirdsFly.Shared.Events.Contracts.IEventContract @event,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "✏️ Handling UserUpdated event - EventId: {EventId}",
            @event.EventId);

        // TODO: Implement business logic for user updates
        // Examples:
        // - Update user profile cache
        // - Sync with external systems
        // - Update search indexes

        await Task.CompletedTask;
    }

    private async Task HandleUserDeactivatedAsync(
        TechBirdsFly.Shared.Events.Contracts.IEventContract @event,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "🚫 Handling UserDeactivated event - EventId: {EventId}",
            @event.EventId);

        // TODO: Implement business logic for user deactivation
        // Examples:
        // - Archive user data
        // - Cancel active subscriptions
        // - Send farewell email
        // - Clean up resources

        await Task.CompletedTask;
    }

    private async Task HandleSubscriptionStartedAsync(
        TechBirdsFly.Shared.Events.Contracts.IEventContract @event,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "💳 Handling SubscriptionStarted event - EventId: {EventId}",
            @event.EventId);

        // TODO: Implement business logic for subscription start
        // Examples:
        // - Update billing records
        // - Activate premium features
        // - Send confirmation email
        // - Initialize subscription tracking

        await Task.CompletedTask;
    }

    private async Task HandleWebsiteGeneratedAsync(
        TechBirdsFly.Shared.Events.Contracts.IEventContract @event,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "🌐 Handling WebsiteGenerated event - EventId: {EventId}",
            @event.EventId);

        // TODO: Implement business logic for website generation
        // Examples:
        // - Archive generated website metadata
        // - Update generation statistics
        // - Trigger deployment
        // - Send completion notification

        await Task.CompletedTask;
    }

    #endregion
}

/// <summary>
/// Settings for event consumer background service
/// </summary>
public class EventConsumerBackgroundSettings
{
    /// <summary>
    /// Whether the background service is enabled
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Delay in seconds before starting consumption (allows application startup)
    /// </summary>
    public int StartupDelaySeconds { get; set; } = 5;

    /// <summary>
    /// Topics to consume from (if empty, uses defaults)
    /// </summary>
    public List<string> Topics { get; set; } = new();
}
