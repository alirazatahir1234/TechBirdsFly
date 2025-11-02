using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EventBusService.Application.Services;

namespace EventBusService.WebAPI.Controllers;

/// <summary>
/// API endpoints for managing event subscriptions
/// Allows services to register interest in specific event types
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
[ApiExplorerSettings(GroupName = "Subscriptions")]
public class SubscriptionsController : ControllerBase
{
    private readonly EventRouter _eventRouter;
    private readonly ILogger<SubscriptionsController> _logger;

    public SubscriptionsController(
        EventRouter eventRouter,
        ILogger<SubscriptionsController> logger)
    {
        _eventRouter = eventRouter ?? throw new ArgumentNullException(nameof(eventRouter));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get event subscription information
    /// </summary>
    /// <remarks>
    /// Returns information about registered event handlers and subscriptions
    /// </remarks>
    /// <returns>Subscription information</returns>
    /// <response code="200">Subscription info retrieved successfully</response>
    [HttpGet("info")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(SubscriptionInfoDto), StatusCodes.Status200OK)]
    public IActionResult GetSubscriptionInfo()
    {
        _logger.LogInformation("Fetching subscription information");

        var info = new SubscriptionInfoDto
        {
            Timestamp = DateTime.UtcNow,
            RegisteredEventTypes = _eventRouter.GetRegisteredEventTypes().ToList(),
            TotalHandlers = _eventRouter.GetTotalHandlerCount(),
            EventHandlerCounts = _eventRouter.GetRegisteredEventTypes()
                .ToDictionary(
                    eventType => eventType,
                    eventType => _eventRouter.GetHandlerCount(eventType))
        };

        return Ok(info);
    }

    /// <summary>
    /// Get handlers for a specific event type
    /// </summary>
    /// <remarks>
    /// Returns the number of handlers registered for an event type
    /// </remarks>
    /// <param name="eventType">Event type to check (e.g., "UserRegistered")</param>
    /// <returns>Handler count</returns>
    /// <response code="200">Handler count retrieved</response>
    [HttpGet("{eventType}/handlers")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(EventHandlerCountDto), StatusCodes.Status200OK)]
    public IActionResult GetHandlerCount(string eventType)
    {
        _logger.LogInformation("Fetching handler count for event type: {EventType}", eventType);

        var count = _eventRouter.GetHandlerCount(eventType);

        return Ok(new EventHandlerCountDto
        {
            EventType = eventType,
            HandlerCount = count,
            IsSubscribed = count > 0,
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Health check endpoint for subscriptions
    /// </summary>
    /// <remarks>
    /// Verifies that the subscription system is operational
    /// </remarks>
    /// <returns>Health status</returns>
    /// <response code="200">System is healthy</response>
    [HttpGet("health")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(SubscriptionHealthDto), StatusCodes.Status200OK)]
    public IActionResult GetHealth()
    {
        return Ok(new SubscriptionHealthDto
        {
            Status = "healthy",
            Service = "EventBusService - Subscriptions",
            RegisteredEventTypes = _eventRouter.GetRegisteredEventTypes().ToList(),
            TotalHandlers = _eventRouter.GetTotalHandlerCount(),
            Timestamp = DateTime.UtcNow,
            Version = "1.0.0"
        });
    }
}

/// <summary>
/// Subscription information DTO
/// </summary>
public class SubscriptionInfoDto
{
    /// <summary>
    /// Information timestamp
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// List of event types with registered handlers
    /// </summary>
    public List<string> RegisteredEventTypes { get; set; } = new();

    /// <summary>
    /// Total number of handlers across all event types
    /// </summary>
    public int TotalHandlers { get; set; }

    /// <summary>
    /// Number of handlers per event type
    /// </summary>
    public Dictionary<string, int> EventHandlerCounts { get; set; } = new();
}

/// <summary>
/// Event handler count DTO
/// </summary>
public class EventHandlerCountDto
{
    /// <summary>
    /// Event type
    /// </summary>
    public string EventType { get; set; } = null!;

    /// <summary>
    /// Number of handlers for this event type
    /// </summary>
    public int HandlerCount { get; set; }

    /// <summary>
    /// Whether there are any handlers (subscribed)
    /// </summary>
    public bool IsSubscribed { get; set; }

    /// <summary>
    /// Timestamp
    /// </summary>
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Subscription system health DTO
/// </summary>
public class SubscriptionHealthDto
{
    /// <summary>
    /// Health status
    /// </summary>
    public string Status { get; set; } = null!;

    /// <summary>
    /// Service name
    /// </summary>
    public string Service { get; set; } = null!;

    /// <summary>
    /// Registered event types
    /// </summary>
    public List<string> RegisteredEventTypes { get; set; } = new();

    /// <summary>
    /// Total handlers
    /// </summary>
    public int TotalHandlers { get; set; }

    /// <summary>
    /// Timestamp
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// API version
    /// </summary>
    public string Version { get; set; } = null!;
}
