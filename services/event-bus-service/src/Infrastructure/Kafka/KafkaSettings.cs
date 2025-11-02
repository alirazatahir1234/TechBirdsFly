namespace EventBusService.Infrastructure.Kafka;

/// <summary>
/// Kafka configuration settings
/// </summary>
public class KafkaSettings
{
    public string BootstrapServers { get; set; } = "localhost:9092";

    public SchemaRegistrySettings SchemaRegistry { get; set; } = new();

    public ConsumerSettings Consumer { get; set; } = new();

    public TopicSettings Topics { get; set; } = new();
}

public class SchemaRegistrySettings
{
    public string Url { get; set; } = "http://localhost:8081";
}

public class ConsumerSettings
{
    public string GroupId { get; set; } = "event-bus-service-group";
    public string AutoOffsetReset { get; set; } = "Earliest";
    public bool EnableAutoCommit { get; set; } = true;
}

public class TopicSettings
{
    public string UserEvents { get; set; } = "user-events";
    public string SubscriptionEvents { get; set; } = "subscription-events";
    public string WebsiteEvents { get; set; } = "website-events";
}
