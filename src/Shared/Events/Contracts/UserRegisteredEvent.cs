namespace TechBirdsFly.Shared.Events.Contracts;

/// <summary>
/// Event fired when a new user registers in the system
/// Use Case U1: UserRegistered → Create Profile + Send Welcome Email
/// </summary>
public class UserRegisteredEvent : IEventContract
{
    /// <summary>
    /// Event ID for idempotency
    /// </summary>
    public string EventId { get; set; } = null!;

    /// <summary>
    /// User ID who just registered
    /// </summary>
    public string UserId { get; set; } = null!;

    /// <summary>
    /// User's email address
    /// </summary>
    public string Email { get; set; } = null!;

    /// <summary>
    /// User's first name
    /// </summary>
    public string FirstName { get; set; } = null!;

    /// <summary>
    /// User's last name
    /// </summary>
    public string LastName { get; set; } = null!;

    /// <summary>
    /// Full name (convenience property)
    /// </summary>
    public string FullName => $"{FirstName} {LastName}";

    /// <summary>
    /// Registration timestamp (Unix milliseconds)
    /// </summary>
    public long Timestamp { get; set; }

    /// <summary>
    /// Source service (always "auth-service")
    /// </summary>
    public string Source { get; set; } = "auth-service";

    /// <summary>
    /// Event type identifier
    /// </summary>
    public string EventType => "UserRegistered";

    /// <summary>
    /// Schema version for versioning
    /// </summary>
    public int SchemaVersion => 1;

    /// <summary>
    /// Correlation ID for distributed tracing
    /// </summary>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// Causation ID (typically the command ID that caused this event)
    /// </summary>
    public string? CausationId { get; set; }

    /// <summary>
    /// User ID from event context (same as UserId for this event)
    /// </summary>
    string? IEventContract.UserId => UserId;

    /// <summary>
    /// Additional metadata (e.g., IP address, user agent, registration source)
    /// </summary>
    public Dictionary<string, string>? Metadata { get; set; }

    /// <summary>
    /// Create a new UserRegisteredEvent
    /// </summary>
    public static UserRegisteredEvent Create(
        string userId,
        string email,
        string firstName,
        string lastName,
        string? correlationId = null,
        Dictionary<string, string>? metadata = null)
    {
        return new UserRegisteredEvent
        {
            EventId = Guid.NewGuid().ToString(),
            UserId = userId,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            CorrelationId = correlationId ?? Guid.NewGuid().ToString(),
            Metadata = metadata ?? new Dictionary<string, string>()
        };
    }

    /// <summary>
    /// Validate event data
    /// </summary>
    public bool Validate(out List<string> errors)
    {
        errors = new List<string>();

        if (string.IsNullOrWhiteSpace(EventId))
            errors.Add("EventId is required");

        if (string.IsNullOrWhiteSpace(UserId))
            errors.Add("UserId is required");

        if (string.IsNullOrWhiteSpace(Email) || !Email.Contains("@"))
            errors.Add("Valid Email is required");

        if (string.IsNullOrWhiteSpace(FirstName))
            errors.Add("FirstName is required");

        if (string.IsNullOrWhiteSpace(LastName))
            errors.Add("LastName is required");

        if (Timestamp <= 0)
            errors.Add("Timestamp must be greater than 0");

        return errors.Count == 0;
    }
}
