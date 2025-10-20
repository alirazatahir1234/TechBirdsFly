using UserService.Models;
using UserService.Data;
using Microsoft.EntityFrameworkCore;

namespace UserService.Services;

/// <summary>
/// Service for user subscription management
/// </summary>
public interface ISubscriptionService
{
    Task<UserSubscription?> GetSubscriptionAsync(string userId);
    Task<UserSubscription> UpgradePlanAsync(string userId, string planType);
    Task<UserSubscription> CancelSubscriptionAsync(string userId);
    Task<bool> UpdateUsageAsync(string userId, int generationCount, decimal storageUsedGb);
    Task<Dictionary<string, UserSubscription>> GetSubscriptionsByPlanAsync(string planType);
}

public class SubscriptionService : ISubscriptionService
{
    private readonly UserDbContext _context;
    private readonly ILogger<SubscriptionService> _logger;

    public SubscriptionService(UserDbContext context, ILogger<SubscriptionService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<UserSubscription?> GetSubscriptionAsync(string userId)
    {
        try
        {
            return await _context.Subscriptions
                .FirstOrDefaultAsync(s => s.UserId == userId && s.Status != "cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subscription for user: {UserId}", userId);
            throw;
        }
    }

    public async Task<UserSubscription> UpgradePlanAsync(string userId, string planType)
    {
        try
        {
            var subscription = await _context.Subscriptions
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (subscription == null)
            {
                throw new KeyNotFoundException($"Subscription not found for user: {userId}");
            }

            var (monthlyCost, monthlyGenerations, monthlyStorageGb) = GetPlanDetails(planType);

            subscription.PlanType = planType;
            subscription.Status = "active";
            subscription.MonthlyCost = monthlyCost;
            subscription.MonthlyImageGenerations = monthlyGenerations;
            subscription.MonthlyStorageGb = monthlyStorageGb;
            subscription.StartDate = DateTime.UtcNow;
            subscription.RenewalDate = DateTime.UtcNow.AddMonths(1);
            subscription.UpdatedAt = DateTime.UtcNow;

            _context.Subscriptions.Update(subscription);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Plan upgraded for user {UserId} to {PlanType}", userId, planType);
            return subscription;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error upgrading plan for user: {UserId}", userId);
            throw;
        }
    }

    public async Task<UserSubscription> CancelSubscriptionAsync(string userId)
    {
        try
        {
            var subscription = await _context.Subscriptions
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (subscription == null)
            {
                throw new KeyNotFoundException($"Subscription not found for user: {userId}");
            }

            subscription.Status = "cancelled";
            subscription.EndDate = DateTime.UtcNow;
            subscription.UpdatedAt = DateTime.UtcNow;

            _context.Subscriptions.Update(subscription);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Subscription cancelled for user: {UserId}", userId);
            return subscription;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling subscription for user: {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> UpdateUsageAsync(string userId, int generationCount, decimal storageUsedGb)
    {
        try
        {
            var subscription = await _context.Subscriptions
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (subscription == null)
            {
                return false;
            }

            subscription.UsedGenerations += generationCount;
            subscription.UsedStorageGb += storageUsedGb;
            subscription.UpdatedAt = DateTime.UtcNow;

            _context.Subscriptions.Update(subscription);
            await _context.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating usage for user: {UserId}", userId);
            throw;
        }
    }

    public async Task<Dictionary<string, UserSubscription>> GetSubscriptionsByPlanAsync(string planType)
    {
        try
        {
            var subscriptions = await _context.Subscriptions
                .Where(s => s.PlanType == planType && s.Status == "active")
                .ToListAsync();

            return subscriptions.ToDictionary(s => s.UserId, s => s);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subscriptions by plan: {PlanType}", planType);
            throw;
        }
    }

    private static (decimal, int, int) GetPlanDetails(string planType)
    {
        return planType.ToLower() switch
        {
            "free" => (0m, 10, 1),
            "starter" => (9.99m, 100, 10),
            "pro" => (29.99m, 500, 50),
            "enterprise" => (99.99m, 5000, 500),
            _ => throw new ArgumentException($"Unknown plan type: {planType}")
        };
    }
}
