namespace TechBirdsFly.Shared.Events.Contracts;

/// <summary>
/// Kafka message envelope wrapping the event with metadata
/// </summary>
public class KafkaEventMessage
{
    /// <summary>
    /// Unique message ID
    /// </summary>
    public string MessageId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Event data
    /// </summary>
    public object Event { get; set; } = null!;

    /// <summary>
    /// Event type for routing
    /// </summary>
    public string EventType { get; set; } = null!;

    /// <summary>
    /// Schema version
    /// </summary>
    public int SchemaVersion { get; set; } = 1;

    /// <summary>
    /// Timestamp when message was created
    /// </summary>
    public long CreatedAt { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    /// <summary>
    /// Source service
    /// </summary>
    public string Source { get; set; } = null!;

    /// <summary>
    /// Partition key for ordering (typically aggregate ID)
    /// </summary>
    public string? PartitionKey { get; set; }

    /// <summary>
    /// Correlation ID for distributed tracing
    /// </summary>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// Message headers as key-value pairs
    /// </summary>
    public Dictionary<string, string> Headers { get; set; } = new();

    /// <summary>
    /// Create a new Kafka message from an event contract
    /// </summary>
    public static KafkaEventMessage FromEvent(
        IEventContract @event,
        string? partitionKey = null)
    {
        return new KafkaEventMessage
        {
            MessageId = @event.EventId,
            Event = @event,
            EventType = @event.EventType,
            SchemaVersion = @event.SchemaVersion,
            CreatedAt = @event.Timestamp,
            Source = @event.Source,
            PartitionKey = partitionKey ?? @event.UserId,
            CorrelationId = @event.CorrelationId,
            Headers = new Dictionary<string, string>
            {
                { "eventType", @event.EventType },
                { "schemaVersion", @event.SchemaVersion.ToString() },
                { "source", @event.Source },
                { "timestamp", @event.Timestamp.ToString() },
                { "eventId", @event.EventId },
                { "correlationId", @event.CorrelationId ?? "" }
            }
        };
    }

    /// <summary>
    /// Validate message
    /// </summary>
    public bool Validate(out List<string> errors)
    {
        errors = new List<string>();

        if (string.IsNullOrWhiteSpace(MessageId))
            errors.Add("MessageId is required");

        if (Event == null)
            errors.Add("Event is required");

        if (string.IsNullOrWhiteSpace(EventType))
            errors.Add("EventType is required");

        if (string.IsNullOrWhiteSpace(Source))
            errors.Add("Source is required");

        if (CreatedAt <= 0)
            errors.Add("CreatedAt must be greater than 0");

        return errors.Count == 0;
    }
}
