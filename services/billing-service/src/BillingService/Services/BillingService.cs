using Microsoft.EntityFrameworkCore;
using BillingService.Data;
using BillingService.Models;

namespace BillingService.Services;

public class BillingService : IBillingService
{
    private readonly BillingDbContext _db;
    private readonly ILogger<BillingService> _logger;

    public BillingService(BillingDbContext db, ILogger<BillingService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<BillingAccount?> GetBillingAccountAsync(Guid userId)
    {
        return await _db.BillingAccounts
            .FirstOrDefaultAsync(b => b.UserId == userId);
    }

    public async Task<BillingAccount> CreateBillingAccountAsync(Guid userId)
    {
        // Check if already exists
        var existing = await GetBillingAccountAsync(userId);
        if (existing != null)
            return existing;

        var account = new BillingAccount
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PlanType = "free",
            MonthlyGenerationsLimit = 10,
            SubscriptionStatus = "active"
        };

        _db.BillingAccounts.Add(account);
        await _db.SaveChangesAsync();
        
        _logger.LogInformation("Created billing account for user {UserId}", userId);
        return account;
    }

    public async Task TrackUsageAsync(Guid userId, string eventType, int count, decimal costPerUnit)
    {
        var metric = new UsageMetric
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            EventType = eventType,
            Count = count,
            CostPerUnit = costPerUnit,
            TotalCost = count * costPerUnit
        };

        _db.UsageMetrics.Add(metric);
        
        // Update billing account
        var account = await GetBillingAccountAsync(userId);
        if (account != null)
        {
            account.MonthlyGenerations += count;
            account.MonthlyBill += metric.TotalCost;
            account.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        _logger.LogInformation("Tracked usage: {EventType} for user {UserId}", eventType, userId);
    }

    public async Task<IEnumerable<Invoice>> GetUserInvoicesAsync(Guid userId)
    {
        var account = await GetBillingAccountAsync(userId);
        if (account == null)
            return Enumerable.Empty<Invoice>();

        return await _db.Invoices
            .Where(i => i.BillingAccountId == account.Id)
            .OrderByDescending(i => i.BilledDate)
            .ToListAsync();
    }

    public async Task<decimal> GetCurrentMonthUsageAsync(Guid userId)
    {
        var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        return await _db.UsageMetrics
            .Where(u => u.UserId == userId && u.EventDate >= startOfMonth)
            .SumAsync(u => u.TotalCost);
    }

    public async Task<bool> IsUnderQuotaAsync(Guid userId)
    {
        var account = await GetBillingAccountAsync(userId);
        if (account == null)
            return true;

        return account.MonthlyGenerations < account.MonthlyGenerationsLimit;
    }
}
