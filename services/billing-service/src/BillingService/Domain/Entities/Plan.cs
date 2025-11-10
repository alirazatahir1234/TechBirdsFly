namespace BillingService.Domain.Entities;

/// <summary>
/// Plan aggregate root - represents a pricing tier or subscription plan
/// </summary>
public sealed class Plan
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public PlanType Type { get; private set; }
    public decimal Price { get; private set; }
    public string Currency { get; private set; } = "USD";
    public BillingCycle BillingCycle { get; private set; }
    public int? TrialDays { get; private set; }
    public bool IsActive { get; private set; }
    public string FeaturesJson { get; private set; } = "{}";  // JSON stored as string
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Plan() { }

    /// <summary>
    /// Factory method to create a new plan
    /// </summary>
    public static Plan Create(string name, string description, PlanType type, decimal price, BillingCycle billingCycle, int? trialDays = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Plan name is required", nameof(name));
        if (price < 0)
            throw new ArgumentException("Price cannot be negative", nameof(price));
        if (trialDays.HasValue && trialDays < 0)
            throw new ArgumentException("Trial days cannot be negative", nameof(trialDays));

        return new Plan
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            Type = type,
            Price = price,
            BillingCycle = billingCycle,
            TrialDays = trialDays,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Deactivate the plan
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Activate the plan
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }
}

/// <summary>
/// Subscription aggregate root - represents a user's subscription to a plan
/// </summary>
public sealed class Subscription
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid PlanId { get; private set; }
    public SubscriptionStatus Status { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public DateTime? NextBillingDate { get; private set; }
    public bool IsOnTrial { get; private set; }
    public DateTime? TrialEndDate { get; private set; }
    public string? CancellationReason { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // Navigation
    public Plan? Plan { get; private set; }
    public List<Invoice> Invoices { get; private set; } = [];

    private Subscription() { }

    /// <summary>
    /// Factory method to create a new subscription
    /// </summary>
    public static Subscription Create(Guid userId, Guid planId, int? trialDays = null)
    {
        var subscription = new Subscription
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PlanId = planId,
            Status = SubscriptionStatus.Active,
            StartDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Set trial if applicable
        if (trialDays.HasValue && trialDays > 0)
        {
            subscription.IsOnTrial = true;
            subscription.TrialEndDate = DateTime.UtcNow.AddDays(trialDays.Value);
            subscription.NextBillingDate = subscription.TrialEndDate;
        }
        else
        {
            subscription.NextBillingDate = DateTime.UtcNow.AddMonths(1);
        }

        return subscription;
    }

    /// <summary>
    /// Mark subscription as cancelled
    /// </summary>
    public void Cancel(string? reason = null)
    {
        if (Status == SubscriptionStatus.Cancelled)
            throw new InvalidOperationException("Subscription is already cancelled");

        Status = SubscriptionStatus.Cancelled;
        EndDate = DateTime.UtcNow;
        CancellationReason = reason;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Renew the subscription for the next billing cycle
    /// </summary>
    public void Renew()
    {
        if (Status == SubscriptionStatus.Cancelled)
            throw new InvalidOperationException("Cannot renew a cancelled subscription");

        IsOnTrial = false;
        TrialEndDate = null;
        NextBillingDate = DateTime.UtcNow.AddMonths(1);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Check if subscription trial has ended
    /// </summary>
    public bool IsTrialExpired => IsOnTrial && TrialEndDate.HasValue && DateTime.UtcNow >= TrialEndDate;

    /// <summary>
    /// Check if subscription needs renewal
    /// </summary>
    public bool NeedsRenewal => NextBillingDate.HasValue && DateTime.UtcNow >= NextBillingDate && Status == SubscriptionStatus.Active;
}

/// <summary>
/// Plan type enumeration
/// </summary>
public enum PlanType
{
    Free = 0,
    Starter = 1,
    Professional = 2,
    Enterprise = 3,
    Custom = 4
}

/// <summary>
/// Subscription status enumeration
/// </summary>
public enum SubscriptionStatus
{
    Trial = 0,
    Active = 1,
    Paused = 2,
    Cancelled = 3,
    Expired = 4
}

/// <summary>
/// Billing cycle enumeration
/// </summary>
public enum BillingCycle
{
    Monthly = 0,
    Quarterly = 1,
    Annually = 2
}
