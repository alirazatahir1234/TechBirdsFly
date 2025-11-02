using TechBirdsFly.Shared.Events.Contracts;
using EventBusService.Application.Interfaces;

namespace EventBusService.Application.Services;

/// <summary>
/// Orchestrates Kafka consumption and event routing
/// Connects to Kafka consumer and routes events to registered handlers
/// </summary>
public class EventConsumerService
{
    private readonly IKafkaConsumer _kafkaConsumer;
    private readonly EventRouter _eventRouter;
    private readonly ILogger<EventConsumerService> _logger;

    public EventConsumerService(
        IKafkaConsumer kafkaConsumer,
        EventRouter eventRouter,
        ILogger<EventConsumerService> logger)
    {
        _kafkaConsumer = kafkaConsumer ?? throw new ArgumentNullException(nameof(kafkaConsumer));
        _eventRouter = eventRouter ?? throw new ArgumentNullException(nameof(eventRouter));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Start consuming events from Kafka topics
    /// </summary>
    /// <param name="topics">Topics to consume from</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task StartConsumingAsync(
        IEnumerable<string> topics,
        CancellationToken cancellationToken = default)
    {
        var topicList = topics.ToList();

        _logger.LogInformation(
            "🚀 Starting event consumer for topics: {Topics}",
            string.Join(", ", topicList));

        if (!_eventRouter.GetRegisteredEventTypes().Any())
        {
            _logger.LogWarning(
                "⚠️ No event handlers registered! Consumer will receive events but won't process them.");
        }

        try
        {
            await _kafkaConsumer.SubscribeAsync(
                topicList,
                HandleEventAsync,
                cancellationToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("⏸️ Event consumer cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Fatal error in event consumer");
            throw;
        }
    }

    /// <summary>
    /// Handle a single event by routing it to registered handlers
    /// </summary>
    private async Task HandleEventAsync(IEventContract? @event, CancellationToken cancellationToken)
    {
        if (@event == null)
        {
            _logger.LogWarning("Received null event");
            return;
        }

        try
        {
            _logger.LogInformation(
                "📥 Received event - EventId: {EventId}, EventType: {EventType}",
                @event.EventId,
                @event.EventType);

            // Route the event to all registered handlers
            var handledCount = await _eventRouter.RouteAsync(@event, cancellationToken);

            if (handledCount == 0)
            {
                _logger.LogWarning(
                    "⚠️ Event received but no handlers executed - EventId: {EventId}, EventType: {EventType}",
                    @event.EventId,
                    @event.EventType);
            }
            else
            {
                _logger.LogInformation(
                    "✅ Event handled by {Count} handler(s) - EventId: {EventId}",
                    handledCount,
                    @event.EventId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "❌ Error handling event - EventId: {EventId}, EventType: {EventType}",
                @event.EventId,
                @event.EventType);
            throw;
        }
    }

    /// <summary>
    /// Get information about registered handlers
    /// </summary>
    public EventConsumerInfo GetConsumerInfo()
    {
        return new EventConsumerInfo
        {
            ConsumerGroup = _kafkaConsumer.GetConsumerGroup(),
            RegisteredEventTypes = _eventRouter.GetRegisteredEventTypes().ToList(),
            TotalHandlers = _eventRouter.GetTotalHandlerCount(),
            Timestamp = DateTime.UtcNow
        };
    }
}

/// <summary>
/// Information about the event consumer
/// </summary>
public class EventConsumerInfo
{
    /// <summary>
    /// Kafka consumer group ID
    /// </summary>
    public string ConsumerGroup { get; set; } = null!;

    /// <summary>
    /// List of event types with registered handlers
    /// </summary>
    public List<string> RegisteredEventTypes { get; set; } = new();

    /// <summary>
    /// Total number of handlers across all event types
    /// </summary>
    public int TotalHandlers { get; set; }

    /// <summary>
    /// Timestamp of info retrieval
    /// </summary>
    public DateTime Timestamp { get; set; }
}
