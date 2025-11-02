using TechBirdsFly.Shared.Kernel;

namespace EventBusService.Domain.Entities;

/// <summary>
/// Represents an event subscription from a service
/// </summary>
public class EventSubscription : BaseEntity
{
    /// <summary>
    /// Name of the service subscribing to events
    /// </summary>
    public string ServiceName { get; set; } = null!;

    /// <summary>
    /// Event type to subscribe to (e.g., "UserRegistered", "SubscriptionStarted")
    /// </summary>
    public string EventType { get; set; } = null!;

    /// <summary>
    /// Webhook URL where events will be delivered
    /// </summary>
    public string WebhookUrl { get; set; } = null!;

    /// <summary>
    /// Whether this subscription is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Retry count for failed deliveries
    /// </summary>
    public int RetryCount { get; set; } = 3;

    /// <summary>
    /// Timeout in seconds for webhook delivery
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Last successful delivery timestamp
    /// </summary>
    public DateTime? LastDeliveredAt { get; set; }

    /// <summary>
    /// Last failed delivery timestamp
    /// </summary>
    public DateTime? LastFailedAt { get; set; }

    /// <summary>
    /// Failure reason for debugging
    /// </summary>
    public string? FailureReason { get; set; }
}
