using EventBusService.Domain.Entities;

namespace EventBusService.Application.Interfaces;

/// <summary>
/// Interface for event subscription management
/// </summary>
public interface IEventSubscriptionRepository
{
    /// <summary>
    /// Subscribe to an event type
    /// </summary>
    Task AddAsync(EventSubscription subscription, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get subscriptions for an event type
    /// </summary>
    Task<List<EventSubscription>> GetByEventTypeAsync(string eventType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get subscriptions by service
    /// </summary>
    Task<List<EventSubscription>> GetByServiceAsync(string serviceName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get active subscriptions
    /// </summary>
    Task<List<EventSubscription>> GetActiveSubscriptionsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get subscription by id
    /// </summary>
    Task<EventSubscription?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update subscription
    /// </summary>
    Task UpdateAsync(EventSubscription subscription, CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove subscription
    /// </summary>
    Task RemoveAsync(Guid subscriptionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update subscription delivery status
    /// </summary>
    Task UpdateDeliveryStatusAsync(Guid subscriptionId, bool success, string? failureReason = null, CancellationToken cancellationToken = default);
}
