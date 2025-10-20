namespace AdminService.Models;

public class DailyStatistic
{
    public Guid Id { get; set; }
    public DateTime Date { get; set; }
    public int NewUsersCount { get; set; } = 0;
    public int ActiveUsersCount { get; set; } = 0;
    public int WebsitesGeneratedCount { get; set; } = 0;
    public int ImagesGeneratedCount { get; set; } = 0;
    public decimal RevenueTotal { get; set; } = 0;
    public decimal AverageUserSpend { get; set; } = 0;
    public int FailedGenerations { get; set; } = 0;
    public double AverageGenerationTime { get; set; } = 0; // in seconds
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
