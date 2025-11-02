using EventBusService.Domain.Entities;

namespace EventBusService.Application.Interfaces;

/// <summary>
/// Interface for outbox event operations
/// </summary>
public interface IOutboxEventRepository
{
    /// <summary>
    /// Add an outbox event
    /// </summary>
    Task AddAsync(OutboxEvent outboxEvent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get unpublished events
    /// </summary>
    Task<List<OutboxEvent>> GetUnpublishedAsync(int batchSize = 100, CancellationToken cancellationToken = default);

    /// <summary>
    /// Mark event as published
    /// </summary>
    Task MarkAsPublishedAsync(Guid eventId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update publish attempt
    /// </summary>
    Task UpdatePublishAttemptAsync(Guid eventId, string? errorMessage = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get event by id
    /// </summary>
    Task<OutboxEvent?> GetByIdAsync(Guid eventId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get events by type
    /// </summary>
    Task<List<OutboxEvent>> GetByEventTypeAsync(string eventType, int maxAge = 7, CancellationToken cancellationToken = default);
}
