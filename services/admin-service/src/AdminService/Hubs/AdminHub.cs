using Microsoft.AspNetCore.SignalR;
using AdminService.Models;
using AdminService.Services;

namespace AdminService.Hubs;

/// <summary>
/// Admin Hub for real-time monitoring and notifications
/// Handles WebSocket connections for live dashboard updates
/// </summary>
public class AdminHub : Hub
{
    private readonly ILogger<AdminHub> _logger;
    private readonly IUserManagementService _userService;
    private readonly IAnalyticsService _analyticsService;

    public AdminHub(
        ILogger<AdminHub> logger,
        IUserManagementService userService,
        IAnalyticsService analyticsService)
    {
        _logger = logger;
        _userService = userService;
        _analyticsService = analyticsService;
    }

    /// <summary>
    /// Called when a client connects to the hub
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        var connectionId = Context.ConnectionId;
        var userId = Context.User?.FindFirst("sub")?.Value ?? "unknown";

        _logger.LogInformation("Admin client connected: {ConnectionId}, UserId: {UserId}", 
            connectionId, userId);

        // Add connection to "admins" group for broadcast capabilities
        await Groups.AddToGroupAsync(connectionId, "admins");

        // Send initial connection confirmation
        await Clients.Caller.SendAsync("ConnectionEstablished", new
        {
            timestamp = DateTime.UtcNow,
            message = "Connected to admin hub"
        });

        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Called when a client disconnects from the hub
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var connectionId = Context.ConnectionId;
        _logger.LogInformation("Admin client disconnected: {ConnectionId}", connectionId);

        if (exception != null)
        {
            _logger.LogError(exception, "Disconnect error for connection: {ConnectionId}", connectionId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Get current platform statistics
    /// </summary>
    public async Task<Dictionary<string, object>> GetPlatformStats()
    {
        try
        {
            var stats = await _analyticsService.GetPlatformSummaryAsync();
            _logger.LogInformation("Platform stats retrieved");
            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving platform stats");
            throw;
        }
    }

    /// <summary>
    /// Subscribe to user activity updates
    /// </summary>
    public async Task SubscribeToUserActivity()
    {
        var connectionId = Context.ConnectionId;
        _logger.LogInformation("User activity subscription requested: {ConnectionId}", connectionId);
        await Groups.AddToGroupAsync(connectionId, "user-activity");
        await Clients.Caller.SendAsync("SubscriptionConfirmed", "user-activity");
    }

    /// <summary>
    /// Unsubscribe from user activity updates
    /// </summary>
    public async Task UnsubscribeFromUserActivity()
    {
        var connectionId = Context.ConnectionId;
        _logger.LogInformation("User activity unsubscription requested: {ConnectionId}", connectionId);
        await Groups.RemoveFromGroupAsync(connectionId, "user-activity");
        await Clients.Caller.SendAsync("UnsubscriptionConfirmed", "user-activity");
    }

    /// <summary>
    /// Subscribe to analytics updates
    /// </summary>
    public async Task SubscribeToAnalytics()
    {
        var connectionId = Context.ConnectionId;
        _logger.LogInformation("Analytics subscription requested: {ConnectionId}", connectionId);
        await Groups.AddToGroupAsync(connectionId, "analytics");
        await Clients.Caller.SendAsync("SubscriptionConfirmed", "analytics");
    }

    /// <summary>
    /// Unsubscribe from analytics updates
    /// </summary>
    public async Task UnsubscribeFromAnalytics()
    {
        var connectionId = Context.ConnectionId;
        _logger.LogInformation("Analytics unsubscription requested: {ConnectionId}", connectionId);
        await Groups.RemoveFromGroupAsync(connectionId, "analytics");
        await Clients.Caller.SendAsync("UnsubscriptionConfirmed", "analytics");
    }

    /// <summary>
    /// Subscribe to system alerts
    /// </summary>
    public async Task SubscribeToAlerts()
    {
        var connectionId = Context.ConnectionId;
        _logger.LogInformation("Alert subscription requested: {ConnectionId}", connectionId);
        await Groups.AddToGroupAsync(connectionId, "alerts");
        await Clients.Caller.SendAsync("SubscriptionConfirmed", "alerts");
    }

    /// <summary>
    /// Get active connection count
    /// </summary>
    public int GetConnectionCount()
    {
        _logger.LogInformation("Connection count requested");
        return Context.ConnectionAborted.IsCancellationRequested ? 0 : 1;
    }

    /// <summary>
    /// Send a test message (for connectivity testing)
    /// </summary>
    public async Task SendTestMessage(string message)
    {
        var connectionId = Context.ConnectionId;
        _logger.LogInformation("Test message from {ConnectionId}: {Message}", connectionId, message);
        await Clients.Caller.SendAsync("TestMessageResponse", $"Echo: {message}");
    }

    /// <summary>
    /// Broadcast user activity to all connected clients
    /// Called by backend services when user actions occur
    /// </summary>
    public async Task BroadcastUserActivity(object activityData)
    {
        _logger.LogInformation("Broadcasting user activity");
        await Clients.Group("user-activity").SendAsync("UserActivityUpdate", new
        {
            timestamp = DateTime.UtcNow,
            data = activityData
        });
    }

    /// <summary>
    /// Broadcast analytics update to subscribed clients
    /// Called by analytics service with new metrics
    /// </summary>
    public async Task BroadcastAnalyticsUpdate(object analyticsData)
    {
        _logger.LogInformation("Broadcasting analytics update");
        await Clients.Group("analytics").SendAsync("AnalyticsUpdate", new
        {
            timestamp = DateTime.UtcNow,
            data = analyticsData
        });
    }

    /// <summary>
    /// Send system alert to all admin connections
    /// </summary>
    public async Task SendSystemAlert(string alertType, string message, object? data = null)
    {
        _logger.LogInformation("Sending system alert: {AlertType} - {Message}", alertType, message);
        await Clients.Group("alerts").SendAsync("SystemAlert", new
        {
            timestamp = DateTime.UtcNow,
            type = alertType,
            message = message,
            data = data
        });
    }

    /// <summary>
    /// Send warning alert
    /// </summary>
    public async Task SendWarning(string message, object? data = null)
    {
        await SendSystemAlert("warning", message, data);
    }

    /// <summary>
    /// Send error alert
    /// </summary>
    public async Task SendError(string message, object? data = null)
    {
        await SendSystemAlert("error", message, data);
    }

    /// <summary>
    /// Send success notification
    /// </summary>
    public async Task SendSuccess(string message, object? data = null)
    {
        await SendSystemAlert("success", message, data);
    }

    /// <summary>
    /// Broadcast real-time platform status
    /// </summary>
    public async Task BroadcastPlatformStatus(object statusData)
    {
        _logger.LogInformation("Broadcasting platform status");
        await Clients.Group("admins").SendAsync("PlatformStatus", new
        {
            timestamp = DateTime.UtcNow,
            status = statusData
        });
    }

    /// <summary>
    /// Broadcast user count update
    /// </summary>
    public async Task BroadcastUserCountUpdate(int totalUsers, int activeUsers, int newUsersToday)
    {
        _logger.LogInformation("Broadcasting user count update");
        await Clients.Group("analytics").SendAsync("UserCountUpdate", new
        {
            timestamp = DateTime.UtcNow,
            totalUsers = totalUsers,
            activeUsers = activeUsers,
            newUsersToday = newUsersToday
        });
    }

    /// <summary>
    /// Broadcast generation metrics update
    /// </summary>
    public async Task BroadcastGenerationMetrics(int websitesGenerated, int imagesGenerated, 
        int failedGenerations, double avgGenerationTime)
    {
        _logger.LogInformation("Broadcasting generation metrics update");
        await Clients.Group("analytics").SendAsync("GenerationMetricsUpdate", new
        {
            timestamp = DateTime.UtcNow,
            websitesGenerated = websitesGenerated,
            imagesGenerated = imagesGenerated,
            failedGenerations = failedGenerations,
            avgGenerationTime = avgGenerationTime
        });
    }

    /// <summary>
    /// Broadcast revenue update
    /// </summary>
    public async Task BroadcastRevenueUpdate(decimal dailyRevenue, decimal totalRevenue30d)
    {
        _logger.LogInformation("Broadcasting revenue update");
        await Clients.Group("analytics").SendAsync("RevenueUpdate", new
        {
            timestamp = DateTime.UtcNow,
            dailyRevenue = dailyRevenue,
            totalRevenue30d = totalRevenue30d
        });
    }

    /// <summary>
    /// Notify about user action (login, logout, activity)
    /// </summary>
    public async Task NotifyUserAction(Guid userId, string action, object? details = null)
    {
        _logger.LogInformation("Notifying user action: {UserId} - {Action}", userId, action);
        await Clients.Group("user-activity").SendAsync("UserActionNotification", new
        {
            timestamp = DateTime.UtcNow,
            userId = userId,
            action = action,
            details = details
        });
    }

    /// <summary>
    /// Notify about role assignment
    /// </summary>
    public async Task NotifyRoleAssignment(Guid userId, string roleName, string action)
    {
        _logger.LogInformation("Notifying role assignment: {UserId} - {RoleName} - {Action}", 
            userId, roleName, action);
        await Clients.Group("user-activity").SendAsync("RoleAssignmentNotification", new
        {
            timestamp = DateTime.UtcNow,
            userId = userId,
            roleName = roleName,
            action = action
        });
    }

    /// <summary>
    /// Notify about system event
    /// </summary>
    public async Task NotifySystemEvent(string eventType, string description, object? data = null)
    {
        _logger.LogInformation("Notifying system event: {EventType} - {Description}", 
            eventType, description);
        await Clients.Group("admins").SendAsync("SystemEventNotification", new
        {
            timestamp = DateTime.UtcNow,
            eventType = eventType,
            description = description,
            data = data
        });
    }

    /// <summary>
    /// Notify about performance issue
    /// </summary>
    public async Task NotifyPerformanceAlert(string metric, double value, string threshold)
    {
        _logger.LogWarning("Performance alert: {Metric} = {Value} (threshold: {Threshold})", 
            metric, value, threshold);
        await Clients.Group("alerts").SendAsync("PerformanceAlert", new
        {
            timestamp = DateTime.UtcNow,
            metric = metric,
            value = value,
            threshold = threshold
        });
    }
}
