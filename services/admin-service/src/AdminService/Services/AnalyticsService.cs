using Microsoft.EntityFrameworkCore;
using AdminService.Data;
using AdminService.Models;

namespace AdminService.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly AdminDbContext _db;
    private readonly ILogger<AnalyticsService> _logger;
    private readonly IUserManagementService _userService;

    public AnalyticsService(AdminDbContext db, ILogger<AnalyticsService> logger, IUserManagementService userService)
    {
        _db = db;
        _logger = logger;
        _userService = userService;
    }

    public async Task<DailyStatistic?> GetDailyStatsAsync(DateTime date)
    {
        var dateOnly = date.Date;
        return await _db.DailyStatistics.FirstOrDefaultAsync(d => d.Date == dateOnly);
    }

    public async Task<IEnumerable<DailyStatistic>> GetStatsRangeAsync(DateTime from, DateTime to)
    {
        var fromDate = from.Date;
        var toDate = to.Date;
        
        return await _db.DailyStatistics
            .Where(d => d.Date >= fromDate && d.Date <= toDate)
            .OrderBy(d => d.Date)
            .ToListAsync();
    }

    public async Task<decimal> GetTotalRevenueAsync(DateTime from, DateTime to)
    {
        var stats = await GetStatsRangeAsync(from, to);
        return stats.Sum(s => s.RevenueTotal);
    }

    public async Task<decimal> GetAverageUserSpendAsync(DateTime from, DateTime to)
    {
        var stats = await GetStatsRangeAsync(from, to);
        if (!stats.Any())
            return 0;
        
        return stats.Average(s => s.AverageUserSpend);
    }

    public async Task<int> GetTotalWebsitesGeneratedAsync(DateTime from, DateTime to)
    {
        var stats = await GetStatsRangeAsync(from, to);
        return stats.Sum(s => s.WebsitesGeneratedCount);
    }

    public async Task<int> GetTotalImagesGeneratedAsync(DateTime from, DateTime to)
    {
        var stats = await GetStatsRangeAsync(from, to);
        return stats.Sum(s => s.ImagesGeneratedCount);
    }

    public async Task<double> GetAverageGenerationTimeAsync(DateTime from, DateTime to)
    {
        var stats = await GetStatsRangeAsync(from, to);
        if (!stats.Any())
            return 0;
        
        return stats.Average(s => s.AverageGenerationTime);
    }

    public async Task<int> GetFailedGenerationsCountAsync(DateTime from, DateTime to)
    {
        var stats = await GetStatsRangeAsync(from, to);
        return stats.Sum(s => s.FailedGenerations);
    }

    public async Task RecordDailyStatsAsync(DailyStatistic stats)
    {
        var existing = await GetDailyStatsAsync(stats.Date);
        
        if (existing != null)
        {
            // Update existing stats
            existing.NewUsersCount = stats.NewUsersCount;
            existing.ActiveUsersCount = stats.ActiveUsersCount;
            existing.WebsitesGeneratedCount = stats.WebsitesGeneratedCount;
            existing.ImagesGeneratedCount = stats.ImagesGeneratedCount;
            existing.RevenueTotal = stats.RevenueTotal;
            existing.AverageUserSpend = stats.AverageUserSpend;
            existing.FailedGenerations = stats.FailedGenerations;
            existing.AverageGenerationTime = stats.AverageGenerationTime;
            
            await _db.SaveChangesAsync();
        }
        else
        {
            stats.Id = Guid.NewGuid();
            stats.CreatedAt = DateTime.UtcNow;
            _db.DailyStatistics.Add(stats);
            await _db.SaveChangesAsync();
        }

        _logger.LogInformation("Recorded daily statistics for {Date}", stats.Date);
    }

    public async Task<Dictionary<string, object>> GetPlatformSummaryAsync()
    {
        var totalUsers = await _userService.GetTotalUsersCountAsync();
        var activeUsers = await _userService.GetActiveUsersCountAsync();
        var newUsersToday = await _userService.GetNewUsersCountAsync(DateTime.UtcNow.Date);

        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
        var totalRevenue = await GetTotalRevenueAsync(thirtyDaysAgo, DateTime.UtcNow);
        var websitesGenerated = await GetTotalWebsitesGeneratedAsync(thirtyDaysAgo, DateTime.UtcNow);
        var imagesGenerated = await GetTotalImagesGeneratedAsync(thirtyDaysAgo, DateTime.UtcNow);
        var failedGenerations = await GetFailedGenerationsCountAsync(thirtyDaysAgo, DateTime.UtcNow);
        var avgGenerationTime = await GetAverageGenerationTimeAsync(thirtyDaysAgo, DateTime.UtcNow);

        var todayStats = await GetDailyStatsAsync(DateTime.UtcNow);

        return new Dictionary<string, object>
        {
            ["total_users"] = totalUsers,
            ["active_users"] = activeUsers,
            ["new_users_today"] = newUsersToday,
            ["total_revenue_30d"] = totalRevenue,
            ["websites_generated_30d"] = websitesGenerated,
            ["images_generated_30d"] = imagesGenerated,
            ["failed_generations_30d"] = failedGenerations,
            ["avg_generation_time_30d"] = Math.Round(avgGenerationTime, 2),
            ["today_stats"] = new
            {
                NewUsersCount = todayStats?.NewUsersCount ?? 0,
                ActiveUsersCount = todayStats?.ActiveUsersCount ?? 0,
                WebsitesGeneratedCount = todayStats?.WebsitesGeneratedCount ?? 0,
                ImagesGeneratedCount = todayStats?.ImagesGeneratedCount ?? 0,
                RevenueTotal = todayStats?.RevenueTotal ?? 0
            },
            ["timestamp"] = DateTime.UtcNow
        };
    }
}
