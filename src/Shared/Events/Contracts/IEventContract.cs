namespace TechBirdsFly.Shared.Events.Contracts;

/// <summary>
/// Base interface for all event contracts
/// </summary>
public interface IEventContract
{
    /// <summary>
    /// Unique event ID
    /// </summary>
    string EventId { get; }

    /// <summary>
    /// Event type name (e.g., "UserRegistered")
    /// </summary>
    string EventType { get; }

    /// <summary>
    /// Schema version for versioning support
    /// </summary>
    int SchemaVersion { get; }

    /// <summary>
    /// Timestamp when event occurred
    /// </summary>
    long Timestamp { get; }

    /// <summary>
    /// Source/origin of the event (service name)
    /// </summary>
    string Source { get; }

    /// <summary>
    /// Correlation ID for tracing across services
    /// </summary>
    string? CorrelationId { get; }

    /// <summary>
    /// Causation ID for event chain tracking
    /// </summary>
    string? CausationId { get; }

    /// <summary>
    /// User ID associated with the event
    /// </summary>
    string? UserId { get; }

    /// <summary>
    /// Additional metadata
    /// </summary>
    Dictionary<string, string>? Metadata { get; }
}
