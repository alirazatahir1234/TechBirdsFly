namespace BillingService.Domain.Entities;

/// <summary>
/// Invoice aggregate root - represents a billing invoice for charges and payments
/// </summary>
public sealed class Invoice
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid? SubscriptionId { get; private set; }
    public decimal Amount { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal TotalAmount { get; private set; }
    public string Currency { get; private set; } = "USD";
    public string Description { get; private set; } = string.Empty;
    public InvoiceStatus Status { get; private set; }
    public DateTime IssuedDate { get; private set; }
    public DateTime? DueDate { get; private set; }
    public DateTime? PaidDate { get; private set; }
    public string? Notes { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // Navigation
    public List<InvoiceLineItem> LineItems { get; private set; } = [];
    public List<Payment> Payments { get; private set; } = [];

    private Invoice() { }

    /// <summary>
    /// Factory method to create a new invoice
    /// </summary>
    public static Invoice Create(Guid userId, decimal amount, decimal taxAmount, string description, Guid? subscriptionId = null)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be greater than zero", nameof(amount));
        if (taxAmount < 0)
            throw new ArgumentException("Tax amount cannot be negative", nameof(taxAmount));

        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            SubscriptionId = subscriptionId,
            Amount = amount,
            TaxAmount = taxAmount,
            TotalAmount = amount + taxAmount,
            Description = description,
            Status = InvoiceStatus.Draft,
            IssuedDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return invoice;
    }

    /// <summary>
    /// Issue the invoice (move to sent state)
    /// </summary>
    public void Issue()
    {
        if (Status != InvoiceStatus.Draft)
            throw new InvalidOperationException("Only draft invoices can be issued");

        Status = InvoiceStatus.Sent;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Mark invoice as paid
    /// </summary>
    public void MarkAsPaid()
    {
        if (Status == InvoiceStatus.Paid)
            throw new InvalidOperationException("Invoice is already paid");

        Status = InvoiceStatus.Paid;
        PaidDate = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Add a line item to the invoice
    /// </summary>
    public void AddLineItem(InvoiceLineItem lineItem)
    {
        if (Status != InvoiceStatus.Draft)
            throw new InvalidOperationException("Cannot modify non-draft invoices");

        LineItems.Add(lineItem);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Record a payment for this invoice
    /// </summary>
    public void RecordPayment(Payment payment)
    {
        if (payment.InvoiceId != Id)
            throw new ArgumentException("Payment does not belong to this invoice", nameof(payment));

        Payments.Add(payment);

        var totalPaid = Payments.Where(p => p.Status == PaymentStatus.Completed).Sum(p => p.Amount);
        if (totalPaid >= TotalAmount)
        {
            MarkAsPaid();
        }

        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Calculate remaining balance
    /// </summary>
    public decimal GetRemainingBalance()
    {
        var totalPaid = Payments.Where(p => p.Status == PaymentStatus.Completed).Sum(p => p.Amount);
        return TotalAmount - totalPaid;
    }

    /// <summary>
    /// Check if invoice is overdue
    /// </summary>
    public bool IsOverdue => DueDate.HasValue && DateTime.UtcNow > DueDate && Status != InvoiceStatus.Paid;
}

/// <summary>
/// Invoice line item representing individual charges
/// </summary>
public sealed class InvoiceLineItem
{
    public Guid Id { get; private set; }
    public Guid InvoiceId { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public decimal Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal Amount { get; private set; }

    private InvoiceLineItem() { }

    public static InvoiceLineItem Create(Guid invoiceId, string description, decimal quantity, decimal unitPrice)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));
        if (unitPrice < 0)
            throw new ArgumentException("Unit price cannot be negative", nameof(unitPrice));

        return new InvoiceLineItem
        {
            Id = Guid.NewGuid(),
            InvoiceId = invoiceId,
            Description = description,
            Quantity = quantity,
            UnitPrice = unitPrice,
            Amount = quantity * unitPrice
        };
    }
}

/// <summary>
/// Invoice status enumeration
/// </summary>
public enum InvoiceStatus
{
    Draft = 0,
    Sent = 1,
    Paid = 2,
    Overdue = 3,
    Cancelled = 4,
    Refunded = 5
}
