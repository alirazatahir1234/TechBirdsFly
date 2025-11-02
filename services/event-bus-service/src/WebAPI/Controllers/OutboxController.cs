using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EventBusService.Application.Interfaces;
using EventBusService.Domain.Entities;

namespace EventBusService.WebAPI.Controllers;

/// <summary>
/// API endpoints for monitoring outbox status
/// Provides visibility into event publishing state
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[ApiExplorerSettings(GroupName = "Monitoring")]
public class OutboxController : ControllerBase
{
    private readonly IOutboxEventRepository _outboxRepository;
    private readonly ILogger<OutboxController> _logger;

    public OutboxController(
        IOutboxEventRepository outboxRepository,
        ILogger<OutboxController> logger)
    {
        _outboxRepository = outboxRepository ?? throw new ArgumentNullException(nameof(outboxRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get outbox statistics
    /// </summary>
    /// <remarks>
    /// Returns count of pending, published, and dead-lettered events
    /// </remarks>
    /// <returns>Outbox statistics</returns>
    /// <response code="200">Statistics retrieved successfully</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("stats")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(OutboxStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<OutboxStatsDto>> GetOutboxStats()
    {
        try
        {
            _logger.LogInformation("Fetching outbox statistics");

            // Get unpublished events
            var unpublished = await _outboxRepository.GetUnpublishedAsync(int.MaxValue);

            var stats = new OutboxStatsDto
            {
                Timestamp = DateTime.UtcNow,
                TotalPending = unpublished.Count,
                PendingByType = unpublished
                    .GroupBy(e => e.EventType)
                    .ToDictionary(g => g.Key, g => g.Count()),
                OldestEventAge = unpublished.Any()
                    ? DateTime.UtcNow.Subtract(unpublished.Min(e => e.OccurredAt)).TotalSeconds
                    : 0
            };

            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving outbox statistics");
            return StatusCode(500, new ErrorDto
            {
                Error = "Failed to retrieve outbox statistics",
                Message = ex.Message
            });
        }
    }

    /// <summary>
    /// Get pending events
    /// </summary>
    /// <remarks>
    /// Returns list of unpublished events with pagination
    /// </remarks>
    /// <param name="limit">Maximum number of events to return (default 50)</param>
    /// <returns>List of pending events</returns>
    /// <response code="200">Events retrieved successfully</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("pending")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<OutboxEventDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<OutboxEventDto>>> GetPendingEvents([FromQuery] int limit = 50)
    {
        try
        {
            _logger.LogInformation("Fetching pending outbox events - Limit: {Limit}", limit);

            var events = await _outboxRepository.GetUnpublishedAsync(Math.Min(limit, 100));

            var dtos = events.Select(e => new OutboxEventDto
            {
                Id = e.Id,
                EventType = e.EventType,
                Topic = e.Topic,
                CreatedAt = e.OccurredAt,
                Attempts = e.PublishAttempts,
                LastError = e.LastErrorMessage
            }).ToList();

            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving pending events");
            return StatusCode(500, new ErrorDto
            {
                Error = "Failed to retrieve pending events",
                Message = ex.Message
            });
        }
    }

    /// <summary>
    /// Get event by ID
    /// </summary>
    /// <remarks>
    /// Retrieve detailed information about a specific outbox event
    /// </remarks>
    /// <param name="eventId">Event ID</param>
    /// <returns>Event details</returns>
    /// <response code="200">Event found</response>
    /// <response code="404">Event not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{eventId}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(OutboxEventDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<OutboxEventDetailDto>> GetEvent(Guid eventId)
    {
        try
        {
            _logger.LogInformation("Fetching event - EventId: {EventId}", eventId);

            var @event = await _outboxRepository.GetByIdAsync(eventId);

            if (@event == null)
            {
                return NotFound(new ErrorDto
                {
                    Error = "Event not found",
                    Message = $"No event found with ID: {eventId}"
                });
            }

            var dto = new OutboxEventDetailDto
            {
                Id = @event.Id,
                EventType = @event.EventType,
                Topic = @event.Topic,
                PartitionKey = @event.PartitionKey,
                IsPublished = @event.IsPublished,
                CreatedAt = @event.OccurredAt,
                PublishedAt = @event.PublishedAt,
                Attempts = @event.PublishAttempts,
                LastError = @event.LastErrorMessage,
                Payload = @event.EventPayload
            };

            return Ok(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving event");
            return StatusCode(500, new ErrorDto
            {
                Error = "Failed to retrieve event",
                Message = ex.Message
            });
        }
    }
}

/// <summary>
/// Outbox statistics
/// </summary>
public class OutboxStatsDto
{
    /// <summary>
    /// Statistics timestamp
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Total pending events
    /// </summary>
    public int TotalPending { get; set; }

    /// <summary>
    /// Pending events by type
    /// </summary>
    public Dictionary<string, int> PendingByType { get; set; } = new();

    /// <summary>
    /// Age of oldest pending event in seconds
    /// </summary>
    public double OldestEventAge { get; set; }
}

/// <summary>
/// Outbox event summary
/// </summary>
public class OutboxEventDto
{
    /// <summary>
    /// Event ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Event type
    /// </summary>
    public string EventType { get; set; } = null!;

    /// <summary>
    /// Kafka topic
    /// </summary>
    public string Topic { get; set; } = null!;

    /// <summary>
    /// Event created timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Number of publish attempts
    /// </summary>
    public int Attempts { get; set; }

    /// <summary>
    /// Last error message
    /// </summary>
    public string? LastError { get; set; }
}

/// <summary>
/// Outbox event detailed information
/// </summary>
public class OutboxEventDetailDto
{
    /// <summary>
    /// Event ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Event type
    /// </summary>
    public string EventType { get; set; } = null!;

    /// <summary>
    /// Kafka topic
    /// </summary>
    public string Topic { get; set; } = null!;

    /// <summary>
    /// Partition key
    /// </summary>
    public string? PartitionKey { get; set; }

    /// <summary>
    /// Whether event was published
    /// </summary>
    public bool IsPublished { get; set; }

    /// <summary>
    /// Event created timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Event published timestamp (if published)
    /// </summary>
    public DateTime? PublishedAt { get; set; }

    /// <summary>
    /// Number of publish attempts
    /// </summary>
    public int Attempts { get; set; }

    /// <summary>
    /// Last error message
    /// </summary>
    public string? LastError { get; set; }

    /// <summary>
    /// Full event payload
    /// </summary>
    public string Payload { get; set; } = null!;
}

/// <summary>
/// Error response
/// </summary>
public class ErrorDto
{
    /// <summary>
    /// Error code/type
    /// </summary>
    public string Error { get; set; } = null!;

    /// <summary>
    /// Detailed error message
    /// </summary>
    public string Message { get; set; } = null!;
}
