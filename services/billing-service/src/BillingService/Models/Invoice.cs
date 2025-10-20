namespace BillingService.Models;

public class Invoice
{
    public Guid Id { get; set; }
    public Guid BillingAccountId { get; set; }
    public string StripeInvoiceId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime BilledDate { get; set; }
    public DateTime DueDate { get; set; }
    public string Status { get; set; } = "draft"; // draft, open, paid, uncollectible, void
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
