namespace BillingService.Models;

public class UsageMetric
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string EventType { get; set; } = string.Empty; // website_generated, image_generated, etc.
    public int Count { get; set; } = 1;
    public decimal CostPerUnit { get; set; } = 0;
    public decimal TotalCost { get; set; } = 0;
    public DateTime EventDate { get; set; } = DateTime.UtcNow;
    public string? Metadata { get; set; } // JSON string
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
