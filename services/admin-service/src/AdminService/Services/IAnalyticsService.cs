using AdminService.Models;

namespace AdminService.Services;

public interface IAnalyticsService
{
    Task<DailyStatistic?> GetDailyStatsAsync(DateTime date);
    Task<IEnumerable<DailyStatistic>> GetStatsRangeAsync(DateTime from, DateTime to);
    Task<decimal> GetTotalRevenueAsync(DateTime from, DateTime to);
    Task<decimal> GetAverageUserSpendAsync(DateTime from, DateTime to);
    Task<int> GetTotalWebsitesGeneratedAsync(DateTime from, DateTime to);
    Task<int> GetTotalImagesGeneratedAsync(DateTime from, DateTime to);
    Task<double> GetAverageGenerationTimeAsync(DateTime from, DateTime to);
    Task<int> GetFailedGenerationsCountAsync(DateTime from, DateTime to);
    Task RecordDailyStatsAsync(DailyStatistic stats);
    Task<Dictionary<string, object>> GetPlatformSummaryAsync();
}
