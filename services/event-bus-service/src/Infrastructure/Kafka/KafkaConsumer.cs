using Confluent.Kafka;
using TechBirdsFly.Shared.Events.Contracts;
using TechBirdsFly.Shared.Events.Serialization;
using EventBusService.Application.Interfaces;

namespace EventBusService.Infrastructure.Kafka;

/// <summary>
/// Kafka consumer implementation for receiving and processing events
/// </summary>
public class KafkaConsumer : IKafkaConsumer
{
    private readonly IConsumer<string, string> _consumer;
    private readonly ILogger<KafkaConsumer> _logger;
    private readonly KafkaSettings _kafkaSettings;

    public KafkaConsumer(KafkaSettings kafkaSettings, ILogger<KafkaConsumer> logger)
    {
        _kafkaSettings = kafkaSettings ?? throw new ArgumentNullException(nameof(kafkaSettings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = kafkaSettings.BootstrapServers,
            GroupId = kafkaSettings.Consumer.GroupId,
            AutoOffsetReset = (AutoOffsetReset)Enum.Parse(
                typeof(AutoOffsetReset),
                kafkaSettings.Consumer.AutoOffsetReset),
            EnableAutoCommit = kafkaSettings.Consumer.EnableAutoCommit,
            StatisticsIntervalMs = 5000,
            SessionTimeoutMs = 6000
        };

        _consumer = new ConsumerBuilder<string, string>(consumerConfig)
            .SetErrorHandler((_, error) =>
            {
                _logger.LogError($"❌ Kafka consumer error: {error.Reason}");
            })
            .SetStatisticsHandler((_, json) =>
            {
                _logger.LogDebug($"📊 Kafka stats: {json}");
            })
            .Build();

        _logger.LogInformation(
            "✅ Kafka Consumer initialized - GroupId: {GroupId}, Servers: {Servers}",
            kafkaSettings.Consumer.GroupId,
            kafkaSettings.BootstrapServers);
    }

    /// <summary>
    /// Subscribe to a single topic and process messages
    /// </summary>
    public async Task SubscribeAsync(
        string topic,
        Func<IEventContract?, CancellationToken, Task> messageHandler,
        CancellationToken cancellationToken = default)
    {
        await SubscribeAsync(new[] { topic }, messageHandler, cancellationToken);
    }

    /// <summary>
    /// Subscribe to multiple topics and process messages
    /// </summary>
    public async Task SubscribeAsync(
        IEnumerable<string> topics,
        Func<IEventContract?, CancellationToken, Task> messageHandler,
        CancellationToken cancellationToken = default)
    {
        var topicList = topics.ToList();

        _logger.LogInformation(
            "🔔 Subscribing to topics: {Topics}",
            string.Join(", ", topicList));

        _consumer.Subscribe(topicList);

        try
        {
            await ConsumeMessagesAsync(messageHandler, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("⏸️ Consumer subscription cancelled");
            _consumer.Unsubscribe();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error in consumer subscription");
            _consumer.Unsubscribe();
            throw;
        }
    }

    /// <summary>
    /// Internal method to consume messages from topics
    /// </summary>
    private async Task ConsumeMessagesAsync(
        Func<IEventContract?, CancellationToken, Task> messageHandler,
        CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var consumeResult = _consumer.Consume(cancellationToken);

                if (consumeResult.IsPartitionEOF)
                {
                    _logger.LogDebug(
                        "📭 Reached end of partition {Partition} at offset {Offset}",
                        consumeResult.Partition.Value,
                        consumeResult.Offset.Value);
                    continue;
                }

                _logger.LogDebug(
                    "📨 Message consumed - Topic: {Topic}, Partition: {Partition}, Offset: {Offset}",
                    consumeResult.Topic,
                    consumeResult.Partition.Value,
                    consumeResult.Offset.Value);

                // Deserialize event
                var @event = EventFactory.CreateFromJson(consumeResult.Message.Value);

                if (@event == null)
                {
                    _logger.LogWarning(
                        "⚠️ Failed to deserialize event from topic {Topic}",
                        consumeResult.Topic);
                    continue;
                }

                _logger.LogInformation(
                    "✅ Processing event - EventId: {EventId}, Type: {EventType}",
                    @event.EventId,
                    @event.EventType);

                // Call the message handler
                await messageHandler(@event, cancellationToken);

                _logger.LogInformation(
                    "✅ Event processed successfully - EventId: {EventId}",
                    @event.EventId);
            }
            catch (ConsumeException ex)
            {
                _logger.LogError(ex, "❌ Consume error: {Error}", ex.Error.Reason);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("⏸️ Consumer cancelled");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error processing message");
            }
        }
    }

    /// <summary>
    /// Get the consumer group ID
    /// </summary>
    public string GetConsumerGroup() => _kafkaSettings.Consumer.GroupId;

    /// <summary>
    /// Dispose the consumer
    /// </summary>
    public void Dispose()
    {
        _consumer?.Close();
        _consumer?.Dispose();
        _logger.LogInformation("🛑 Kafka Consumer disposed");
    }
}
