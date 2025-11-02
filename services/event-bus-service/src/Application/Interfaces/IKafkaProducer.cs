namespace EventBusService.Application.Interfaces;

/// <summary>
/// Interface for Kafka producer operations
/// </summary>
public interface IKafkaProducer
{
    /// <summary>
    /// Publish an event to Kafka
    /// </summary>
    Task PublishAsync(string topic, string partitionKey, string eventPayload, string eventType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Publish an event with specific value serializer
    /// </summary>
    Task PublishAsync<T>(string topic, string partitionKey, T value, CancellationToken cancellationToken = default) where T : class;
}
