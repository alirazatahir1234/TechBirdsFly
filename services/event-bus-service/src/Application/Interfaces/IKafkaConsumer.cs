using TechBirdsFly.Shared.Events.Contracts;

namespace EventBusService.Application.Interfaces;

/// <summary>
/// Interface for Kafka consumer operations
/// </summary>
public interface IKafkaConsumer : IDisposable
{
    /// <summary>
    /// Subscribe to a Kafka topic and process messages
    /// </summary>
    Task SubscribeAsync(
        string topic,
        Func<IEventContract?, CancellationToken, Task> messageHandler,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Subscribe to multiple topics
    /// </summary>
    Task SubscribeAsync(
        IEnumerable<string> topics,
        Func<IEventContract?, CancellationToken, Task> messageHandler,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get consumer group metadata
    /// </summary>
    string GetConsumerGroup();
}
