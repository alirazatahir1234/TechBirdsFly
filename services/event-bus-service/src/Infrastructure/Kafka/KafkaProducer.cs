using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using EventBusService.Application.Interfaces;

namespace EventBusService.Infrastructure.Kafka;

/// <summary>
/// Kafka producer implementation for publishing events
/// </summary>
public class KafkaProducer : IKafkaProducer, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly ILogger<KafkaProducer> _logger;

    public KafkaProducer(KafkaSettings kafkaSettings, ILogger<KafkaProducer> logger)
    {
        _logger = logger;

        var producerConfig = new ProducerConfig
        {
            BootstrapServers = kafkaSettings.BootstrapServers,
            ClientId = "event-bus-producer",
            Acks = Acks.All,
            MessageTimeoutMs = 30000,
            CompressionType = CompressionType.Snappy
        };

        _producer = new ProducerBuilder<string, string>(producerConfig)
            .SetErrorHandler((_, error) =>
            {
                _logger.LogError($"Kafka error: {error.Reason}");
            })
            .Build();

        _logger.LogInformation("✅ Kafka Producer initialized successfully");
    }

    /// <summary>
    /// Publish event to Kafka topic
    /// </summary>
    public async Task PublishAsync(
        string topic,
        string partitionKey,
        string eventPayload,
        string eventType,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var message = new Message<string, string>
            {
                Key = partitionKey,
                Value = eventPayload,
                Headers = new Headers
                {
                    { "EventType", System.Text.Encoding.UTF8.GetBytes(eventType) }
                }
            };

            var deliveryReport = await _producer.ProduceAsync(topic, message, cancellationToken);

            _logger.LogInformation(
                $"📤 Event published: EventType={eventType}, Topic={topic}, " +
                $"Partition={deliveryReport.Partition}, Offset={deliveryReport.Offset}");
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning($"Publish operation cancelled for topic {topic}");
            throw;
        }
        catch (KafkaException ex)
        {
            _logger.LogError($"❌ Kafka publish error: {ex.Error.Reason}");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ Publish error: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Publish typed event to Kafka
    /// </summary>
    public async Task PublishAsync<T>(
        string topic,
        string partitionKey,
        T value,
        CancellationToken cancellationToken = default) where T : class
    {
        var payload = JsonSerializer.Serialize(value);
        await PublishAsync(topic, partitionKey, payload, typeof(T).Name, cancellationToken);
    }

    public void Dispose()
    {
        _producer?.Dispose();
    }
}
