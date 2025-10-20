using Microsoft.AspNetCore.Mvc;
using BillingService.Services;

namespace BillingService.Controllers;

[ApiController]
[Route("api/billing")]
public class BillingController : ControllerBase
{
    private readonly IBillingService _billingService;
    private readonly ILogger<BillingController> _logger;

    public BillingController(IBillingService billingService, ILogger<BillingController> logger)
    {
        _billingService = billingService;
        _logger = logger;
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetBillingInfo(Guid userId)
    {
        var account = await _billingService.GetBillingAccountAsync(userId);
        if (account == null)
        {
            account = await _billingService.CreateBillingAccountAsync(userId);
        }
        
        return Ok(new
        {
            account.Id,
            account.UserId,
            account.SubscriptionStatus,
            account.PlanType,
            account.MonthlyGenerations,
            account.MonthlyGenerationsLimit,
            account.MonthlyBill,
            account.CreatedAt
        });
    }

    [HttpGet("invoices/{userId}")]
    public async Task<IActionResult> GetInvoices(Guid userId)
    {
        var invoices = await _billingService.GetUserInvoicesAsync(userId);
        return Ok(invoices.Select(i => new
        {
            i.Id,
            i.Amount,
            i.BilledDate,
            i.DueDate,
            i.Status
        }));
    }

    [HttpPost("track-usage")]
    public async Task<IActionResult> TrackUsage([FromBody] TrackUsageRequest request)
    {
        await _billingService.TrackUsageAsync(request.UserId, request.EventType, request.Count, request.CostPerUnit);
        return Ok(new { message = "Usage tracked" });
    }

    [HttpGet("usage/{userId}")]
    public async Task<IActionResult> GetCurrentUsage(Guid userId)
    {
        var currentUsage = await _billingService.GetCurrentMonthUsageAsync(userId);
        var isUnderQuota = await _billingService.IsUnderQuotaAsync(userId);
        
        return Ok(new
        {
            currentMonthUsage = currentUsage,
            isUnderQuota
        });
    }

    [HttpPost("webhook/stripe")]
    public IActionResult StripeWebhook()
    {
        // TODO: Implement Stripe webhook handling
        _logger.LogInformation("Received Stripe webhook");
        return Ok();
    }
}

public class TrackUsageRequest
{
    public Guid UserId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public int Count { get; set; } = 1;
    public decimal CostPerUnit { get; set; } = 0;
}
