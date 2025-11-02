namespace EventBusService.Application.DTOs;

/// <summary>
/// DTO for subscribing to events
/// </summary>
public class SubscribeToEventRequest
{
    /// <summary>
    /// Service name subscribing to the event
    /// </summary>
    public string ServiceName { get; set; } = null!;

    /// <summary>
    /// Event type to subscribe to
    /// </summary>
    public string EventType { get; set; } = null!;

    /// <summary>
    /// Webhook URL for event delivery
    /// </summary>
    public string WebhookUrl { get; set; } = null!;

    /// <summary>
    /// Number of retries for failed deliveries
    /// </summary>
    public int RetryCount { get; set; } = 3;

    /// <summary>
    /// Timeout in seconds for webhook calls
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;
}

/// <summary>
/// DTO for subscription response
/// </summary>
public class SubscriptionResponse
{
    public Guid SubscriptionId { get; set; }
    public string ServiceName { get; set; } = null!;
    public string EventType { get; set; } = null!;
    public string WebhookUrl { get; set; } = null!;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
