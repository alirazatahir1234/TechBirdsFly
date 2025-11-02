namespace TechBirdsFly.Shared.Events.Contracts;

/// <summary>
/// Kafka topic and event type constants
/// </summary>
public static class EventTopics
{
    /// <summary>
    /// User domain events topic
    /// </summary>
    public const string USER_EVENTS = "user-events";

    /// <summary>
    /// Specific user events
    /// </summary>
    public const string USER_REGISTERED = "user-registered";
    public const string USER_UPDATED = "user-updated";
    public const string USER_DEACTIVATED = "user-deactivated";

    /// <summary>
    /// Subscription domain events topic
    /// </summary>
    public const string SUBSCRIPTION_EVENTS = "subscription-events";

    /// <summary>
    /// Specific subscription events
    /// </summary>
    public const string SUBSCRIPTION_STARTED = "subscription-started";
    public const string SUBSCRIPTION_ENDED = "subscription-ended";
    public const string SUBSCRIPTION_UPGRADED = "subscription-upgraded";

    /// <summary>
    /// Website domain events topic
    /// </summary>
    public const string WEBSITE_EVENTS = "website-events";

    /// <summary>
    /// Specific website events
    /// </summary>
    public const string WEBSITE_GENERATED = "website-generated";
    public const string WEBSITE_PUBLISHED = "website-published";
    public const string WEBSITE_DELETED = "website-deleted";

    /// <summary>
    /// Billing domain events topic
    /// </summary>
    public const string BILLING_EVENTS = "billing-events";

    /// <summary>
    /// Specific billing events
    /// </summary>
    public const string PAYMENT_PROCESSED = "payment-processed";
    public const string INVOICE_CREATED = "invoice-created";

    /// <summary>
    /// System events topic
    /// </summary>
    public const string SYSTEM_EVENTS = "system-events";

    /// <summary>
    /// Health check topic
    /// </summary>
    public const string HEALTH_CHECK = "health-check";

    /// <summary>
    /// Get event type for a topic
    /// </summary>
    public static string GetEventType(string topic) => topic switch
    {
        USER_REGISTERED => nameof(UserRegisteredEvent),
        USER_UPDATED => "UserUpdated",
        USER_DEACTIVATED => "UserDeactivated",
        SUBSCRIPTION_STARTED => "SubscriptionStarted",
        SUBSCRIPTION_ENDED => "SubscriptionEnded",
        SUBSCRIPTION_UPGRADED => "SubscriptionUpgraded",
        WEBSITE_GENERATED => "WebsiteGenerated",
        WEBSITE_PUBLISHED => "WebsitePublished",
        WEBSITE_DELETED => "WebsiteDeleted",
        PAYMENT_PROCESSED => "PaymentProcessed",
        INVOICE_CREATED => "InvoiceCreated",
        _ => "Unknown"
    };

    /// <summary>
    /// Get topic for an event type
    /// </summary>
    public static string GetTopic(string eventType) => eventType switch
    {
        nameof(UserRegisteredEvent) => USER_REGISTERED,
        "UserUpdated" => USER_UPDATED,
        "UserDeactivated" => USER_DEACTIVATED,
        "SubscriptionStarted" => SUBSCRIPTION_STARTED,
        "SubscriptionEnded" => SUBSCRIPTION_ENDED,
        "SubscriptionUpgraded" => SUBSCRIPTION_UPGRADED,
        "WebsiteGenerated" => WEBSITE_GENERATED,
        "WebsitePublished" => WEBSITE_PUBLISHED,
        "WebsiteDeleted" => WEBSITE_DELETED,
        "PaymentProcessed" => PAYMENT_PROCESSED,
        "InvoiceCreated" => INVOICE_CREATED,
        _ => SYSTEM_EVENTS
    };

    /// <summary>
    /// All event topics
    /// </summary>
    public static readonly string[] AllTopics = new[]
    {
        USER_EVENTS,
        USER_REGISTERED,
        USER_UPDATED,
        USER_DEACTIVATED,
        SUBSCRIPTION_EVENTS,
        SUBSCRIPTION_STARTED,
        SUBSCRIPTION_ENDED,
        SUBSCRIPTION_UPGRADED,
        WEBSITE_EVENTS,
        WEBSITE_GENERATED,
        WEBSITE_PUBLISHED,
        WEBSITE_DELETED,
        BILLING_EVENTS,
        PAYMENT_PROCESSED,
        INVOICE_CREATED,
        SYSTEM_EVENTS,
        HEALTH_CHECK
    };

    /// <summary>
    /// Get all event topics for a domain
    /// </summary>
    public static string[] GetDomainTopics(string domain) => domain.ToLower() switch
    {
        "user" => new[] { USER_EVENTS, USER_REGISTERED, USER_UPDATED, USER_DEACTIVATED },
        "subscription" => new[] { SUBSCRIPTION_EVENTS, SUBSCRIPTION_STARTED, SUBSCRIPTION_ENDED, SUBSCRIPTION_UPGRADED },
        "website" => new[] { WEBSITE_EVENTS, WEBSITE_GENERATED, WEBSITE_PUBLISHED, WEBSITE_DELETED },
        "billing" => new[] { BILLING_EVENTS, PAYMENT_PROCESSED, INVOICE_CREATED },
        _ => new[] { SYSTEM_EVENTS }
    };
}
