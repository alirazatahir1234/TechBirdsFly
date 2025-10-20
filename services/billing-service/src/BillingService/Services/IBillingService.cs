using BillingService.Models;

namespace BillingService.Services;

public interface IBillingService
{
    Task<BillingAccount?> GetBillingAccountAsync(Guid userId);
    Task<BillingAccount> CreateBillingAccountAsync(Guid userId);
    Task TrackUsageAsync(Guid userId, string eventType, int count, decimal costPerUnit);
    Task<IEnumerable<Invoice>> GetUserInvoicesAsync(Guid userId);
    Task<decimal> GetCurrentMonthUsageAsync(Guid userId);
    Task<bool> IsUnderQuotaAsync(Guid userId);
}
