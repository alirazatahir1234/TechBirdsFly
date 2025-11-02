using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EventBusService.Application.DTOs;
using EventBusService.Application.Services;

namespace EventBusService.WebAPI.Controllers;

/// <summary>
/// API endpoints for event publishing
/// Provides REST interface for microservices to publish events
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
[ApiExplorerSettings(GroupName = "Events")]
public class EventsController : ControllerBase
{
    private readonly PublishEventService _publishEventService;
    private readonly ILogger<EventsController> _logger;

    public EventsController(
        PublishEventService publishEventService,
        ILogger<EventsController> logger)
    {
        _publishEventService = publishEventService ?? throw new ArgumentNullException(nameof(publishEventService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Publish an event to the outbox for guaranteed delivery
    /// </summary>
    /// <remarks>
    /// The event is stored in the outbox and a background worker will publish it to Kafka.
    /// This ensures guaranteed delivery even if the service crashes between event creation and publication.
    ///
    /// Example request body:
    /// ```json
    /// {
    ///   "eventType": "UserRegistered",
    ///   "eventData": "{\"eventId\":\"evt-001\",\"userId\":\"user123\",...}",
    ///   "correlationId": "req-123",
    ///   "metadata": {
    ///     "ipAddress": "192.168.1.1"
    ///   }
    /// }
    /// ```
    /// </remarks>
    /// <param name="request">Event publishing request with event data and metadata</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Event ID and confirmation message</returns>
    /// <response code="200">Event successfully published to outbox</response>
    /// <response code="400">Invalid event format or validation error</response>
    /// <response code="401">Unauthorized - valid JWT token required</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("publish")]
    [ProducesResponseType(typeof(PublishEventResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PublishEventResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(PublishEventResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PublishEventResponse>> PublishEvent(
        [FromBody] PublishEventRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Publish event request received - EventType: {EventType}",
            request?.EventType);

        if (request == null)
        {
            var response = PublishEventResponse.CreateFailure("Request body is required");
            return BadRequest(response);
        }

        var result = await _publishEventService.PublishEventAsync(request, cancellationToken);

        if (!result.Success)
        {
            _logger.LogWarning(
                "Event publication failed - EventType: {EventType}, Reason: {Message}",
                request.EventType,
                result.Message);
            return BadRequest(result);
        }

        _logger.LogInformation(
            "Event published successfully - EventId: {EventId}, EventType: {EventType}",
            result.EventId,
            request.EventType);

        return Ok(result);
    }

    /// <summary>
    /// Get event publication health status
    /// </summary>
    /// <remarks>
    /// Use this endpoint to verify the event bus service is operational
    /// and ready to accept event publications.
    /// </remarks>
    /// <returns>Service health status</returns>
    /// <response code="200">Service is healthy</response>
    [HttpGet("health")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetHealth()
    {
        return Ok(new
        {
            status = "healthy",
            service = "EventBusService",
            timestamp = DateTime.UtcNow,
            version = "1.0.0"
        });
    }

    /// <summary>
    /// Get supported event types
    /// </summary>
    /// <remarks>
    /// Returns a list of all supported event types that can be published.
    /// Useful for clients to discover available events.
    /// </remarks>
    /// <returns>List of supported event types and their topics</returns>
    /// <response code="200">List of event types</response>
    [HttpGet("types")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetEventTypes()
    {
        var eventTypes = new[]
        {
            new { eventType = "UserRegistered", topic = "user-registered", description = "New user registration" },
            new { eventType = "UserUpdated", topic = "user-updated", description = "User profile updated" },
            new { eventType = "UserDeactivated", topic = "user-deactivated", description = "User account deactivated" },
            new { eventType = "SubscriptionStarted", topic = "subscription-started", description = "New subscription started" },
            new { eventType = "WebsiteGenerated", topic = "website-generated", description = "Website generated" }
        };

        return Ok(new
        {
            eventTypes,
            totalCount = eventTypes.Length,
            timestamp = DateTime.UtcNow
        });
    }
}
