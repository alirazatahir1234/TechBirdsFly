using TechBirdsFly.Shared.Events.Serialization;

namespace TechBirdsFly.Shared.Events.Contracts;

/// <summary>
/// Factory for creating typed events with validation and defaults
/// </summary>
public static class EventFactory
{
    /// <summary>
    /// Creates a UserRegistered event
    /// </summary>
    public static UserRegisteredEvent CreateUserRegistered(
        string userId,
        string email,
        string firstName,
        string lastName,
        string? correlationId = null,
        Dictionary<string, string>? metadata = null)
    {
        var @event = UserRegisteredEvent.Create(userId, email, firstName, lastName, correlationId);
        @event.Metadata = metadata ?? new();
        return @event;
    }

    /// <summary>
    /// Creates a UserUpdated event
    /// </summary>
    public static UserUpdatedEvent CreateUserUpdated(
        string userId,
        string email,
        string firstName,
        string lastName,
        string? correlationId = null,
        Dictionary<string, string>? metadata = null)
    {
        var @event = UserUpdatedEvent.Create(userId, email, firstName, lastName, correlationId);
        @event.Metadata = metadata ?? new();
        return @event;
    }

    /// <summary>
    /// Creates a UserDeactivated event
    /// </summary>
    public static UserDeactivatedEvent CreateUserDeactivated(
        string userId,
        string? reason = null,
        string? correlationId = null,
        Dictionary<string, string>? metadata = null)
    {
        var @event = UserDeactivatedEvent.Create(userId, reason, correlationId);
        @event.Metadata = metadata ?? new();
        return @event;
    }

    /// <summary>
    /// Creates a SubscriptionStarted event
    /// </summary>
    public static SubscriptionStartedEvent CreateSubscriptionStarted(
        string userId,
        string subscriptionId,
        string plan,
        decimal price,
        string? correlationId = null,
        Dictionary<string, string>? metadata = null)
    {
        var @event = SubscriptionStartedEvent.Create(userId, subscriptionId, plan, price, correlationId);
        @event.Metadata = metadata ?? new();
        return @event;
    }

    /// <summary>
    /// Creates a WebsiteGenerated event
    /// </summary>
    public static WebsiteGeneratedEvent CreateWebsiteGenerated(
        string userId,
        string websiteId,
        string websiteName,
        string url,
        string? correlationId = null,
        Dictionary<string, string>? metadata = null)
    {
        var @event = WebsiteGeneratedEvent.Create(userId, websiteId, websiteName, url, correlationId);
        @event.Metadata = metadata ?? new();
        return @event;
    }

    /// <summary>
    /// Creates an event from dictionary (useful for Kafka deserialization)
    /// </summary>
    public static IEventContract? CreateFromDictionary(Dictionary<string, object?> data)
    {
        if (data == null || !data.ContainsKey("eventType"))
            return null;

        var eventType = data["eventType"]?.ToString();
        
        return eventType switch
        {
            "UserRegistered" => EventSerializer.DictionaryToObject<UserRegisteredEvent>(data),
            "UserUpdated" => EventSerializer.DictionaryToObject<UserUpdatedEvent>(data),
            "UserDeactivated" => EventSerializer.DictionaryToObject<UserDeactivatedEvent>(data),
            "SubscriptionStarted" => EventSerializer.DictionaryToObject<SubscriptionStartedEvent>(data),
            "WebsiteGenerated" => EventSerializer.DictionaryToObject<WebsiteGeneratedEvent>(data),
            _ => null
        };
    }

    /// <summary>
    /// Creates an event from JSON string (useful for Kafka message parsing)
    /// </summary>
    public static IEventContract? CreateFromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            var dict = EventSerializer.DeserializeFromJson<Dictionary<string, object?>>(json);
            return dict != null ? CreateFromDictionary(dict) : null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Creates a KafkaEventMessage wrapper from an event
    /// </summary>
    public static KafkaEventMessage WrapForKafka(
        IEventContract @event,
        string? partitionKey = null,
        Dictionary<string, string>? headers = null)
    {
        var kafkaMessage = KafkaEventMessage.FromEvent(@event);
        kafkaMessage.PartitionKey = partitionKey ?? @event.UserId;
        
        if (headers != null)
        {
            foreach (var (key, value) in headers)
            {
                kafkaMessage.Headers[key] = value;
            }
        }

        return kafkaMessage;
    }
}
