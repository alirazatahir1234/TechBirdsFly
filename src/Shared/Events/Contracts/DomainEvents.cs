namespace TechBirdsFly.Shared.Events.Contracts;

/// <summary>
/// Event fired when user profile is updated
/// </summary>
public class UserUpdatedEvent : IEventContract
{
    public string EventId { get; set; } = null!;
    public string UserId { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public long Timestamp { get; set; }
    public string Source { get; set; } = "auth-service";
    public string EventType => "UserUpdated";
    public int SchemaVersion => 1;
    public string? CorrelationId { get; set; }
    public string? CausationId { get; set; }
    string? IEventContract.UserId => UserId;
    public Dictionary<string, string>? Metadata { get; set; }

    public static UserUpdatedEvent Create(
        string userId,
        string email,
        string firstName,
        string lastName,
        string? correlationId = null)
    {
        return new UserUpdatedEvent
        {
            EventId = Guid.NewGuid().ToString(),
            UserId = userId,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            CorrelationId = correlationId ?? Guid.NewGuid().ToString()
        };
    }
}

/// <summary>
/// Event fired when user account is deactivated
/// </summary>
public class UserDeactivatedEvent : IEventContract
{
    public string EventId { get; set; } = null!;
    public string UserId { get; set; } = null!;
    public string? Reason { get; set; }
    public long Timestamp { get; set; }
    public string Source { get; set; } = "auth-service";
    public string EventType => "UserDeactivated";
    public int SchemaVersion => 1;
    public string? CorrelationId { get; set; }
    public string? CausationId { get; set; }
    string? IEventContract.UserId => UserId;
    public Dictionary<string, string>? Metadata { get; set; }

    public static UserDeactivatedEvent Create(
        string userId,
        string? reason = null,
        string? correlationId = null)
    {
        return new UserDeactivatedEvent
        {
            EventId = Guid.NewGuid().ToString(),
            UserId = userId,
            Reason = reason,
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            CorrelationId = correlationId ?? Guid.NewGuid().ToString()
        };
    }
}

/// <summary>
/// Event fired when subscription starts
/// </summary>
public class SubscriptionStartedEvent : IEventContract
{
    public string EventId { get; set; } = null!;
    public string UserId { get; set; } = null!;
    public string SubscriptionId { get; set; } = null!;
    public string Plan { get; set; } = null!;
    public decimal Price { get; set; }
    public long Timestamp { get; set; }
    public string Source { get; set; } = "billing-service";
    public string EventType => "SubscriptionStarted";
    public int SchemaVersion => 1;
    public string? CorrelationId { get; set; }
    public string? CausationId { get; set; }
    string? IEventContract.UserId => UserId;
    public Dictionary<string, string>? Metadata { get; set; }

    public static SubscriptionStartedEvent Create(
        string userId,
        string subscriptionId,
        string plan,
        decimal price,
        string? correlationId = null)
    {
        return new SubscriptionStartedEvent
        {
            EventId = Guid.NewGuid().ToString(),
            UserId = userId,
            SubscriptionId = subscriptionId,
            Plan = plan,
            Price = price,
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            CorrelationId = correlationId ?? Guid.NewGuid().ToString()
        };
    }
}

/// <summary>
/// Event fired when website is generated
/// </summary>
public class WebsiteGeneratedEvent : IEventContract
{
    public string EventId { get; set; } = null!;
    public string UserId { get; set; } = null!;
    public string WebsiteId { get; set; } = null!;
    public string WebsiteName { get; set; } = null!;
    public string Url { get; set; } = null!;
    public long Timestamp { get; set; }
    public string Source { get; set; } = "generator-service";
    public string EventType => "WebsiteGenerated";
    public int SchemaVersion => 1;
    public string? CorrelationId { get; set; }
    public string? CausationId { get; set; }
    string? IEventContract.UserId => UserId;
    public Dictionary<string, string>? Metadata { get; set; }

    public static WebsiteGeneratedEvent Create(
        string userId,
        string websiteId,
        string websiteName,
        string url,
        string? correlationId = null)
    {
        return new WebsiteGeneratedEvent
        {
            EventId = Guid.NewGuid().ToString(),
            UserId = userId,
            WebsiteId = websiteId,
            WebsiteName = websiteName,
            Url = url,
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            CorrelationId = correlationId ?? Guid.NewGuid().ToString()
        };
    }
}
