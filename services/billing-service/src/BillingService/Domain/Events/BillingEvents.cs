namespace BillingService.Domain.Events;

/// <summary>
/// Base domain event class
/// </summary>
public abstract class DomainEvent
{
    public Guid AggregateId { get; protected set; }
    public Guid CorrelationId { get; protected set; }
    public DateTime OccurredAt { get; protected set; }
    public string EventType => GetType().Name;

    protected DomainEvent(Guid aggregateId)
    {
        AggregateId = aggregateId;
        CorrelationId = Guid.NewGuid();
        OccurredAt = DateTime.UtcNow;
    }
}

/// <summary>
/// Event raised when invoice is created
/// </summary>
public sealed class InvoiceCreatedEvent : DomainEvent
{
    public Guid UserId { get; set; }
    public decimal Amount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string Description { get; set; } = string.Empty;

    public InvoiceCreatedEvent(Guid invoiceId, Guid userId, decimal amount, decimal taxAmount, string description)
        : base(invoiceId)
    {
        UserId = userId;
        Amount = amount;
        TaxAmount = taxAmount;
        TotalAmount = amount + taxAmount;
        Description = description;
    }
}

/// <summary>
/// Event raised when invoice is issued
/// </summary>
public sealed class InvoiceIssuedEvent : DomainEvent
{
    public Guid UserId { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime DueDate { get; set; }

    public InvoiceIssuedEvent(Guid invoiceId, Guid userId, decimal totalAmount, DateTime dueDate)
        : base(invoiceId)
    {
        UserId = userId;
        TotalAmount = totalAmount;
        DueDate = dueDate;
    }
}

/// <summary>
/// Event raised when payment is processed
/// </summary>
public sealed class PaymentProcessedEvent : DomainEvent
{
    public Guid UserId { get; set; }
    public Guid InvoiceId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public bool Success { get; set; }

    public PaymentProcessedEvent(Guid paymentId, Guid userId, Guid invoiceId, decimal amount, string paymentMethod, bool success)
        : base(paymentId)
    {
        UserId = userId;
        InvoiceId = invoiceId;
        Amount = amount;
        PaymentMethod = paymentMethod;
        Success = success;
    }
}

/// <summary>
/// Event raised when payment fails
/// </summary>
public sealed class PaymentFailedEvent : DomainEvent
{
    public Guid UserId { get; set; }
    public Guid InvoiceId { get; set; }
    public decimal Amount { get; set; }
    public string FailureReason { get; set; } = string.Empty;
    public int RetryCount { get; set; }

    public PaymentFailedEvent(Guid paymentId, Guid userId, Guid invoiceId, decimal amount, string failureReason, int retryCount)
        : base(paymentId)
    {
        UserId = userId;
        InvoiceId = invoiceId;
        Amount = amount;
        FailureReason = failureReason;
        RetryCount = retryCount;
    }
}

/// <summary>
/// Event raised when subscription is created
/// </summary>
public sealed class SubscriptionCreatedEvent : DomainEvent
{
    public Guid UserId { get; set; }
    public Guid PlanId { get; set; }
    public bool IsOnTrial { get; set; }
    public DateTime? TrialEndDate { get; set; }

    public SubscriptionCreatedEvent(Guid subscriptionId, Guid userId, Guid planId, bool isOnTrial, DateTime? trialEndDate)
        : base(subscriptionId)
    {
        UserId = userId;
        PlanId = planId;
        IsOnTrial = isOnTrial;
        TrialEndDate = trialEndDate;
    }
}

/// <summary>
/// Event raised when subscription is cancelled
/// </summary>
public sealed class SubscriptionCancelledEvent : DomainEvent
{
    public Guid UserId { get; set; }
    public Guid PlanId { get; set; }
    public string? CancellationReason { get; set; }

    public SubscriptionCancelledEvent(Guid subscriptionId, Guid userId, Guid planId, string? reason)
        : base(subscriptionId)
    {
        UserId = userId;
        PlanId = planId;
        CancellationReason = reason;
    }
}

/// <summary>
/// Event raised when subscription is renewed
/// </summary>
public sealed class SubscriptionRenewedEvent : DomainEvent
{
    public Guid UserId { get; set; }
    public Guid PlanId { get; set; }
    public DateTime NextBillingDate { get; set; }

    public SubscriptionRenewedEvent(Guid subscriptionId, Guid userId, Guid planId, DateTime nextBillingDate)
        : base(subscriptionId)
    {
        UserId = userId;
        PlanId = planId;
        NextBillingDate = nextBillingDate;
    }
}
