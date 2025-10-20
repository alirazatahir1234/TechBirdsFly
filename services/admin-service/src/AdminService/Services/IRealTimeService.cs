using Microsoft.AspNetCore.SignalR;
using AdminService.Hubs;
using AdminService.Models;

namespace AdminService.Services;

/// <summary>
/// Interface for real-time notification service
/// </summary>
public interface IRealTimeService
{
    Task BroadcastUserActivityAsync(object activityData);
    Task BroadcastAnalyticsUpdateAsync(object analyticsData);
    Task BroadcastUserCountUpdateAsync(int totalUsers, int activeUsers, int newUsersToday);
    Task BroadcastGenerationMetricsAsync(int websitesGenerated, int imagesGenerated, 
        int failedGenerations, double avgGenerationTime);
    Task BroadcastRevenueUpdateAsync(decimal dailyRevenue, decimal totalRevenue30d);
    Task NotifyUserActionAsync(Guid userId, string action, object? details = null);
    Task NotifyRoleAssignmentAsync(Guid userId, string roleName, string action);
    Task NotifySystemEventAsync(string eventType, string description, object? data = null);
    Task SendAlertAsync(string alertType, string message, object? data = null);
    Task SendPerformanceAlertAsync(string metric, double value, string threshold);
    Task BroadcastPlatformStatusAsync(object statusData);
}

/// <summary>
/// Real-time notification service implementation
/// Manages WebSocket broadcasts for admin dashboard
/// </summary>
public class RealTimeService : IRealTimeService
{
    private readonly IHubContext<AdminHub> _hubContext;
    private readonly ILogger<RealTimeService> _logger;

    public RealTimeService(
        IHubContext<AdminHub> hubContext,
        ILogger<RealTimeService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task BroadcastUserActivityAsync(object activityData)
    {
        try
        {
            _logger.LogInformation("Broadcasting user activity");
            await _hubContext.Clients.Group("user-activity").SendAsync("UserActivityUpdate", new
            {
                timestamp = DateTime.UtcNow,
                data = activityData
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting user activity");
        }
    }

    public async Task BroadcastAnalyticsUpdateAsync(object analyticsData)
    {
        try
        {
            _logger.LogInformation("Broadcasting analytics update");
            await _hubContext.Clients.Group("analytics").SendAsync("AnalyticsUpdate", new
            {
                timestamp = DateTime.UtcNow,
                data = analyticsData
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting analytics update");
        }
    }

    public async Task BroadcastUserCountUpdateAsync(int totalUsers, int activeUsers, int newUsersToday)
    {
        try
        {
            _logger.LogInformation("Broadcasting user count update");
            await _hubContext.Clients.Group("analytics").SendAsync("UserCountUpdate", new
            {
                timestamp = DateTime.UtcNow,
                totalUsers = totalUsers,
                activeUsers = activeUsers,
                newUsersToday = newUsersToday
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting user count update");
        }
    }

    public async Task BroadcastGenerationMetricsAsync(int websitesGenerated, int imagesGenerated, 
        int failedGenerations, double avgGenerationTime)
    {
        try
        {
            _logger.LogInformation("Broadcasting generation metrics");
            await _hubContext.Clients.Group("analytics").SendAsync("GenerationMetricsUpdate", new
            {
                timestamp = DateTime.UtcNow,
                websitesGenerated = websitesGenerated,
                imagesGenerated = imagesGenerated,
                failedGenerations = failedGenerations,
                avgGenerationTime = avgGenerationTime
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting generation metrics");
        }
    }

    public async Task BroadcastRevenueUpdateAsync(decimal dailyRevenue, decimal totalRevenue30d)
    {
        try
        {
            _logger.LogInformation("Broadcasting revenue update");
            await _hubContext.Clients.Group("analytics").SendAsync("RevenueUpdate", new
            {
                timestamp = DateTime.UtcNow,
                dailyRevenue = dailyRevenue,
                totalRevenue30d = totalRevenue30d
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting revenue update");
        }
    }

    public async Task NotifyUserActionAsync(Guid userId, string action, object? details = null)
    {
        try
        {
            _logger.LogInformation("Notifying user action: {UserId} - {Action}", userId, action);
            await _hubContext.Clients.Group("user-activity").SendAsync("UserActionNotification", new
            {
                timestamp = DateTime.UtcNow,
                userId = userId,
                action = action,
                details = details
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying user action");
        }
    }

    public async Task NotifyRoleAssignmentAsync(Guid userId, string roleName, string action)
    {
        try
        {
            _logger.LogInformation("Notifying role assignment: {UserId} - {RoleName} - {Action}", 
                userId, roleName, action);
            await _hubContext.Clients.Group("user-activity").SendAsync("RoleAssignmentNotification", new
            {
                timestamp = DateTime.UtcNow,
                userId = userId,
                roleName = roleName,
                action = action
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying role assignment");
        }
    }

    public async Task NotifySystemEventAsync(string eventType, string description, object? data = null)
    {
        try
        {
            _logger.LogInformation("Notifying system event: {EventType} - {Description}", 
                eventType, description);
            await _hubContext.Clients.Group("admins").SendAsync("SystemEventNotification", new
            {
                timestamp = DateTime.UtcNow,
                eventType = eventType,
                description = description,
                data = data
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying system event");
        }
    }

    public async Task SendAlertAsync(string alertType, string message, object? data = null)
    {
        try
        {
            _logger.LogInformation("Sending alert: {AlertType} - {Message}", alertType, message);
            await _hubContext.Clients.Group("alerts").SendAsync("SystemAlert", new
            {
                timestamp = DateTime.UtcNow,
                type = alertType,
                message = message,
                data = data
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending alert");
        }
    }

    public async Task SendPerformanceAlertAsync(string metric, double value, string threshold)
    {
        try
        {
            _logger.LogWarning("Performance alert: {Metric} = {Value} (threshold: {Threshold})", 
                metric, value, threshold);
            await _hubContext.Clients.Group("alerts").SendAsync("PerformanceAlert", new
            {
                timestamp = DateTime.UtcNow,
                metric = metric,
                value = value,
                threshold = threshold
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending performance alert");
        }
    }

    public async Task BroadcastPlatformStatusAsync(object statusData)
    {
        try
        {
            _logger.LogInformation("Broadcasting platform status");
            await _hubContext.Clients.Group("admins").SendAsync("PlatformStatus", new
            {
                timestamp = DateTime.UtcNow,
                status = statusData
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting platform status");
        }
    }
}
