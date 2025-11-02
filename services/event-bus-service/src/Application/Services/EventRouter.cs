using TechBirdsFly.Shared.Events.Contracts;

namespace EventBusService.Application.Services;

/// <summary>
/// Delegate for event handlers
/// </summary>
public delegate Task EventHandlerDelegate(IEventContract @event, CancellationToken cancellationToken);

/// <summary>
/// Routes events to appropriate handlers based on event type
/// Implements the Publisher-Subscriber pattern with type-based routing
/// </summary>
public class EventRouter
{
    private readonly Dictionary<string, List<EventHandlerDelegate>> _handlers = new();
    private readonly ILogger<EventRouter> _logger;

    public EventRouter(ILogger<EventRouter> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Subscribe a handler to events of a specific type
    /// </summary>
    /// <param name="eventType">The event type to subscribe to (e.g., "UserRegistered")</param>
    /// <param name="handler">The handler function to execute when event is received</param>
    public void Subscribe(string eventType, EventHandlerDelegate handler)
    {
        if (string.IsNullOrWhiteSpace(eventType))
            throw new ArgumentException("Event type cannot be empty", nameof(eventType));

        if (handler == null)
            throw new ArgumentNullException(nameof(handler));

        if (!_handlers.ContainsKey(eventType))
        {
            _handlers[eventType] = new List<EventHandlerDelegate>();
        }

        _handlers[eventType].Add(handler);

        _logger.LogInformation(
            "🔔 Event handler subscribed - EventType: {EventType}, TotalHandlers: {Count}",
            eventType,
            _handlers[eventType].Count);
    }

    /// <summary>
    /// Route an event to all registered handlers
    /// </summary>
    /// <param name="@event">The event to route</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of handlers executed</returns>
    public async Task<int> RouteAsync(IEventContract @event, CancellationToken cancellationToken = default)
    {
        if (@event == null)
        {
            _logger.LogWarning("Cannot route null event");
            return 0;
        }

        if (!_handlers.ContainsKey(@event.EventType))
        {
            _logger.LogWarning(
                "⚠️ No handlers registered for event type: {EventType}",
                @event.EventType);
            return 0;
        }

        var handlers = _handlers[@event.EventType];
        var executedCount = 0;

        _logger.LogInformation(
            "🔀 Routing event - EventId: {EventId}, EventType: {EventType}, Handlers: {HandlerCount}",
            @event.EventId,
            @event.EventType,
            handlers.Count);

        foreach (var handler in handlers)
        {
            try
            {
                _logger.LogDebug(
                    "📤 Executing handler for event {EventId}",
                    @event.EventId);

                await handler(@event, cancellationToken);
                executedCount++;

                _logger.LogDebug(
                    "✅ Handler executed successfully for event {EventId}",
                    @event.EventId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "❌ Handler failed for event {EventId}: {Error}",
                    @event.EventId,
                    ex.Message);
                // Continue with other handlers even if one fails
            }
        }

        _logger.LogInformation(
            "✅ Event routing complete - EventId: {EventId}, ExecutedHandlers: {ExecutedCount}/{Total}",
            @event.EventId,
            executedCount,
            handlers.Count);

        return executedCount;
    }

    /// <summary>
    /// Get all registered event types
    /// </summary>
    public IEnumerable<string> GetRegisteredEventTypes() => _handlers.Keys;

    /// <summary>
    /// Get handler count for a specific event type
    /// </summary>
    public int GetHandlerCount(string eventType)
    {
        return _handlers.ContainsKey(eventType) ? _handlers[eventType].Count : 0;
    }

    /// <summary>
    /// Get total handler count across all event types
    /// </summary>
    public int GetTotalHandlerCount()
    {
        return _handlers.Values.Sum(h => h.Count);
    }

    /// <summary>
    /// Clear all handlers (for testing)
    /// </summary>
    public void ClearHandlers()
    {
        _handlers.Clear();
        _logger.LogWarning("⚠️ All event handlers cleared");
    }
}
