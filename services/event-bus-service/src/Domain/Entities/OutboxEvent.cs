using TechBirdsFly.Shared.Kernel;

namespace EventBusService.Domain.Entities;

/// <summary>
/// Outbox pattern entity - stores events for guaranteed delivery
/// </summary>
public class OutboxEvent : BaseEntity
{
    /// <summary>
    /// The event type (e.g., "UserRegistered", "SubscriptionStarted")
    /// </summary>
    public string EventType { get; set; } = null!;

    /// <summary>
    /// The event payload as JSON
    /// </summary>
    public string EventPayload { get; set; } = null!;

    /// <summary>
    /// Timestamp when event was created
    /// </summary>
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when event was published to Kafka
    /// </summary>
    public DateTime? PublishedAt { get; set; }

    /// <summary>
    /// Whether event has been processed and published
    /// </summary>
    public bool IsPublished { get; set; }

    /// <summary>
    /// Topic where this event should be published
    /// </summary>
    public string Topic { get; set; } = null!;

    /// <summary>
    /// Partition key for Kafka (for ordering guarantees)
    /// </summary>
    public string? PartitionKey { get; set; }

    /// <summary>
    /// Number of publishing attempts
    /// </summary>
    public int PublishAttempts { get; set; }

    /// <summary>
    /// Last error message if publishing failed
    /// </summary>
    public string? LastErrorMessage { get; set; }
}
