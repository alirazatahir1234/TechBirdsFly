using Microsoft.AspNetCore.Mvc;
using BillingService.Services;
using BillingService.Services.Cache;

namespace BillingService.Controllers;

[ApiController]
[Route("api/billing")]
public class BillingController : ControllerBase
{
    private readonly IBillingService _billingService;
    private readonly ICacheService _cache;
    private readonly ILogger<BillingController> _logger;

    public BillingController(IBillingService billingService, ICacheService cache, ILogger<BillingController> logger)
    {
        _billingService = billingService;
        _cache = cache;
        _logger = logger;
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetBillingInfo(Guid userId)
    {
        // Try cache first
        var cacheKey = $"billing-account:{userId}";
        var cachedAccount = await _cache.GetAsync<dynamic>(cacheKey);
        if (cachedAccount != null)
        {
            _logger.LogInformation($"Billing info retrieved from cache for user {userId}");
            return Ok(cachedAccount);
        }

        var account = await _billingService.GetBillingAccountAsync(userId);
        if (account == null)
        {
            account = await _billingService.CreateBillingAccountAsync(userId);
        }

        var result = new
        {
            account.Id,
            account.UserId,
            account.SubscriptionStatus,
            account.PlanType,
            account.MonthlyGenerations,
            account.MonthlyGenerationsLimit,
            account.MonthlyBill,
            account.CreatedAt
        };

        // Cache for 15 minutes
        await _cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(15));

        return Ok(result);
    }

    [HttpGet("invoices/{userId}")]
    public async Task<IActionResult> GetInvoices(Guid userId)
    {
        // Try cache first
        var cacheKey = $"invoices:{userId}";
        var cachedInvoices = await _cache.GetAsync<dynamic>(cacheKey);
        if (cachedInvoices != null)
        {
            _logger.LogInformation($"Invoices retrieved from cache for user {userId}");
            return Ok(cachedInvoices);
        }

        var invoices = await _billingService.GetUserInvoicesAsync(userId);
        var result = invoices.Select(i => new
        {
            i.Id,
            i.Amount,
            i.BilledDate,
            i.DueDate,
            i.Status
        });

        // Cache for 30 minutes (invoices don't change often)
        await _cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(30));

        return Ok(result);
    }

    [HttpPost("track-usage")]
    public async Task<IActionResult> TrackUsage([FromBody] TrackUsageRequest request)
    {
        await _billingService.TrackUsageAsync(request.UserId, request.EventType, request.Count, request.CostPerUnit);

        // Invalidate usage cache when usage is tracked
        await _cache.RemoveAsync($"usage:{request.UserId}");

        return Ok(new { message = "Usage tracked" });
    }

    [HttpGet("usage/{userId}")]
    public async Task<IActionResult> GetCurrentUsage(Guid userId)
    {
        // Try cache first
        var cacheKey = $"usage:{userId}";
        var cachedUsage = await _cache.GetAsync<dynamic>(cacheKey);
        if (cachedUsage != null)
        {
            _logger.LogInformation($"Current usage retrieved from cache for user {userId}");
            return Ok(cachedUsage);
        }

        var currentUsage = await _billingService.GetCurrentMonthUsageAsync(userId);
        var isUnderQuota = await _billingService.IsUnderQuotaAsync(userId);

        var result = new
        {
            currentMonthUsage = currentUsage,
            isUnderQuota
        };

        // Cache for 5 minutes (usage changes frequently, so shorter TTL)
        await _cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5));

        return Ok(result);
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
