using TechBirdsFly.Shared.Events.Contracts;

namespace AuthService.Application.Interfaces;

/// <summary>
/// Interface for publishing events to the Event Bus service
/// </summary>
public interface IEventPublisher
{
    /// <summary>
    /// Publish an event to the Event Bus
    /// </summary>
    /// <param name="event">The event to publish</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task PublishEventAsync(IEventContract @event, CancellationToken cancellationToken = default);

    /// <summary>
    /// Publish an event with correlation ID for distributed tracing
    /// </summary>
    /// <param name="event">The event to publish</param>
    /// <param name="correlationId">Correlation ID for tracing</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task PublishEventAsync(
        IEventContract @event,
        string correlationId,
        CancellationToken cancellationToken = default);
}
