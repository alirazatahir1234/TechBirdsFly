namespace BillingService.Models;

public class BillingAccount
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string StripeCustomerId { get; set; } = string.Empty;
    public string SubscriptionStatus { get; set; } = "active"; // active, inactive, cancelled
    public string PlanType { get; set; } = "free"; // free, pro, enterprise
    public int MonthlyGenerations { get; set; } = 0;
    public int MonthlyGenerationsLimit { get; set; } = 10;
    public decimal MonthlyBill { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
}
