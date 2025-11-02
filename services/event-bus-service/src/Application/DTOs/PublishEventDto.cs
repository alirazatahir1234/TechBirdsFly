namespace EventBusService.Application.DTOs;

/// <summary>
/// Request model for publishing an event to the outbox
/// </summary>
public class PublishEventRequest
{
    /// <summary>
    /// Event type (e.g., "UserRegistered", "UserUpdated")
    /// </summary>
    public string EventType { get; set; } = null!;

    /// <summary>
    /// Event payload as JSON string
    /// </summary>
    public string EventData { get; set; } = null!;

    /// <summary>
    /// Optional correlation ID for distributed tracing
    /// </summary>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// Optional metadata key-value pairs
    /// </summary>
    public Dictionary<string, string>? Metadata { get; set; }
}

/// <summary>
/// Response model for event publishing result
/// </summary>
public class PublishEventResponse
{
    /// <summary>
    /// Indicates successful publishing
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Unique event identifier
    /// </summary>
    public string EventId { get; set; } = null!;

    /// <summary>
    /// Response message
    /// </summary>
    public string Message { get; set; } = null!;

    /// <summary>
    /// Event creation timestamp (Unix milliseconds)
    /// </summary>
    public long Timestamp { get; set; }

    /// <summary>
    /// Correlation ID for distributed tracing
    /// </summary>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// Create success response
    /// </summary>
    public static PublishEventResponse CreateSuccess(
        string eventId,
        string? correlationId = null)
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

    /// <summary>
    /// Create failure response
    /// </summary>
    public static PublishEventResponse CreateFailure(string message)
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
