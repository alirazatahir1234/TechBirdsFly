namespace BillingService.Domain.Entities;

/// <summary>
/// Payment aggregate root - represents a payment transaction
/// </summary>
public sealed class Payment
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid InvoiceId { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "USD";
    public PaymentStatus Status { get; private set; }
    public string PaymentMethod { get; private set; } = string.Empty;
    public string? ExternalTransactionId { get; private set; }
    public string? ExternalPaymentGateway { get; private set; }
    public DateTime ProcessedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public string? FailureReason { get; private set; }
    public int RetryCount { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Payment() { }

    /// <summary>
    /// Factory method to create a new payment
    /// </summary>
    public static Payment Create(Guid userId, Guid invoiceId, decimal amount, string paymentMethod, string externalGateway = "manual")
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be greater than zero", nameof(amount));
        if (string.IsNullOrWhiteSpace(paymentMethod))
            throw new ArgumentException("Payment method is required", nameof(paymentMethod));

        return new Payment
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            InvoiceId = invoiceId,
            Amount = amount,
            PaymentMethod = paymentMethod,
            ExternalPaymentGateway = externalGateway,
            Status = PaymentStatus.Pending,
            ProcessedAt = DateTime.UtcNow,
            RetryCount = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Mark payment as completed
    /// </summary>
    public void MarkAsCompleted(string? externalTransactionId = null)
    {
        if (Status == PaymentStatus.Completed)
            throw new InvalidOperationException("Payment is already completed");

        Status = PaymentStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        ExternalTransactionId = externalTransactionId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Mark payment as failed
    /// </summary>
    public void MarkAsFailed(string failureReason, int maxRetries = 3)
    {
        if (Status == PaymentStatus.Completed)
            throw new InvalidOperationException("Cannot fail a completed payment");

        RetryCount++;

        if (RetryCount >= maxRetries)
        {
            Status = PaymentStatus.Failed;
            FailureReason = failureReason;
        }
        else
        {
            Status = PaymentStatus.Pending;
            FailureReason = null;
        }

        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Refund the payment
    /// </summary>
    public void Refund(string reason)
    {
        if (Status != PaymentStatus.Completed)
            throw new InvalidOperationException("Only completed payments can be refunded");

        Status = PaymentStatus.Refunded;
        FailureReason = reason;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Check if payment can be retried
    /// </summary>
    public bool CanRetry(int maxRetries = 3) => RetryCount < maxRetries && Status == PaymentStatus.Pending;
}

/// <summary>
/// Payment status enumeration
/// </summary>
public enum PaymentStatus
{
    Pending = 0,
    Processing = 1,
    Completed = 2,
    Failed = 3,
    Refunded = 4,
    Cancelled = 5
}
