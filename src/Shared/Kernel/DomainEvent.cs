namespace TechBirdsFly.Shared.Kernel;

/// <summary>
/// Base class for all domain events in the system.
/// Domain events represent something that has happened in the domain that other parts of the system need to know about.
/// </summary>
public abstract class DomainEvent
{
    /// <summary>
    /// Unique identifier for this domain event
    /// </summary>
    public Guid EventId { get; } = Guid.NewGuid();

    /// <summary>
    /// Timestamp when the event occurred
    /// </summary>
    public DateTime OccurredAt { get; } = DateTime.UtcNow;

    /// <summary>
    /// Optional correlation ID to track related events
    /// </summary>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// Optional metadata about the event
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Generic domain event for common scenarios
/// </summary>
public class GenericDomainEvent : DomainEvent
{
    public string EventType { get; set; } = string.Empty;
    public object? Data { get; set; }

    public GenericDomainEvent() { }

    public GenericDomainEvent(string eventType, object? data = null)
    {
        EventType = eventType;
        Data = data;
    }
}
