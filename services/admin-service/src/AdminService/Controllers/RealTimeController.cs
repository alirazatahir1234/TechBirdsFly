using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AdminService.Services;

namespace AdminService.Controllers;

[ApiController]
[Route("api/admin/realtime")]
[Authorize]
public class RealTimeController : ControllerBase
{
    private readonly IRealTimeService _realTimeService;
    private readonly IAnalyticsService _analyticsService;
    private readonly IUserManagementService _userService;
    private readonly ILogger<RealTimeController> _logger;

    public RealTimeController(
        IRealTimeService realTimeService,
        IAnalyticsService analyticsService,
        IUserManagementService userService,
        ILogger<RealTimeController> logger)
    {
        _realTimeService = realTimeService;
        _analyticsService = analyticsService;
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// Get real-time connection status
    /// </summary>
    [HttpGet("status")]
    public IActionResult GetConnectionStatus()
    {
        return Ok(new
        {
            status = "connected",
            timestamp = DateTime.UtcNow,
            message = "Real-time service is operational"
        });
    }

    /// <summary>
    /// Get current platform statistics (immediate snapshot)
    /// </summary>
    [HttpGet("platform-snapshot")]
    public async Task<IActionResult> GetPlatformSnapshot()
    {
        try
        {
            var stats = await _analyticsService.GetPlatformSummaryAsync();
            return Ok(new
            {
                timestamp = DateTime.UtcNow,
                data = stats
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting platform snapshot");
            return StatusCode(500, new { error = "Failed to retrieve platform snapshot" });
        }
    }

    /// <summary>
    /// Broadcast platform statistics update to all connected clients
    /// Admin-only operation
    /// </summary>
    [HttpPost("broadcast/platform-update")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> BroadcastPlatformUpdate()
    {
        try
        {
            var stats = await _analyticsService.GetPlatformSummaryAsync();
            await _realTimeService.BroadcastAnalyticsUpdateAsync(stats);
            _logger.LogInformation("Platform update broadcasted");
            return Ok(new { message = "Platform update broadcasted" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting platform update");
            return StatusCode(500, new { error = "Failed to broadcast update" });
        }
    }

    /// <summary>
    /// Broadcast user count update
    /// </summary>
    [HttpPost("broadcast/user-count")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> BroadcastUserCount()
    {
        try
        {
            var totalUsers = await _userService.GetTotalUsersCountAsync();
            var activeUsers = await _userService.GetActiveUsersCountAsync();
            var newUsersToday = await _userService.GetNewUsersCountAsync(DateTime.UtcNow.Date);

            await _realTimeService.BroadcastUserCountUpdateAsync(totalUsers, activeUsers, newUsersToday);
            _logger.LogInformation("User count update broadcasted");
            return Ok(new 
            { 
                message = "User count update broadcasted",
                totalUsers,
                activeUsers,
                newUsersToday
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting user count");
            return StatusCode(500, new { error = "Failed to broadcast user count" });
        }
    }

    /// <summary>
    /// Broadcast generation metrics update
    /// </summary>
    [HttpPost("broadcast/generation-metrics")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> BroadcastGenerationMetrics(
        [FromBody] GenerationMetricsRequest request)
    {
        try
        {
            await _realTimeService.BroadcastGenerationMetricsAsync(
                request.WebsitesGenerated,
                request.ImagesGenerated,
                request.FailedGenerations,
                request.AverageGenerationTime);

            _logger.LogInformation("Generation metrics broadcasted");
            return Ok(new { message = "Generation metrics broadcasted" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting generation metrics");
            return StatusCode(500, new { error = "Failed to broadcast generation metrics" });
        }
    }

    /// <summary>
    /// Broadcast revenue update
    /// </summary>
    [HttpPost("broadcast/revenue")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> BroadcastRevenue(
        [FromBody] RevenueUpdateRequest request)
    {
        try
        {
            await _realTimeService.BroadcastRevenueUpdateAsync(
                request.DailyRevenue,
                request.TotalRevenue30d);

            _logger.LogInformation("Revenue update broadcasted");
            return Ok(new { message = "Revenue update broadcasted" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting revenue update");
            return StatusCode(500, new { error = "Failed to broadcast revenue update" });
        }
    }

    /// <summary>
    /// Send system alert to all connected admins
    /// </summary>
    [HttpPost("alert")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SendAlert([FromBody] AlertRequest request)
    {
        try
        {
            await _realTimeService.SendAlertAsync(request.Type, request.Message, request.Data);
            _logger.LogInformation("Alert sent: {AlertType} - {Message}", request.Type, request.Message);
            return Ok(new { message = "Alert sent successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending alert");
            return StatusCode(500, new { error = "Failed to send alert" });
        }
    }

    /// <summary>
    /// Send warning alert
    /// </summary>
    [HttpPost("alert/warning")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SendWarning([FromBody] AlertMessageRequest request)
    {
        try
        {
            await _realTimeService.SendAlertAsync("warning", request.Message, request.Data);
            _logger.LogWarning("Warning alert sent: {Message}", request.Message);
            return Ok(new { message = "Warning sent successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending warning");
            return StatusCode(500, new { error = "Failed to send warning" });
        }
    }

    /// <summary>
    /// Send error alert
    /// </summary>
    [HttpPost("alert/error")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SendError([FromBody] AlertMessageRequest request)
    {
        try
        {
            await _realTimeService.SendAlertAsync("error", request.Message, request.Data);
            _logger.LogError("Error alert sent: {Message}", request.Message);
            return Ok(new { message = "Error alert sent successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending error alert");
            return StatusCode(500, new { error = "Failed to send error alert" });
        }
    }

    /// <summary>
    /// Send performance alert
    /// </summary>
    [HttpPost("alert/performance")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SendPerformanceAlert([FromBody] PerformanceAlertRequest request)
    {
        try
        {
            await _realTimeService.SendPerformanceAlertAsync(
                request.Metric,
                request.Value,
                request.Threshold);

            _logger.LogWarning("Performance alert: {Metric} = {Value} (threshold: {Threshold})",
                request.Metric, request.Value, request.Threshold);

            return Ok(new { message = "Performance alert sent successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending performance alert");
            return StatusCode(500, new { error = "Failed to send performance alert" });
        }
    }

    /// <summary>
    /// Notify user action in real-time
    /// </summary>
    [HttpPost("notify/user-action")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> NotifyUserAction([FromBody] UserActionRequest request)
    {
        try
        {
            await _realTimeService.NotifyUserActionAsync(
                request.UserId,
                request.Action,
                request.Details);

            _logger.LogInformation("User action notified: {UserId} - {Action}",
                request.UserId, request.Action);

            return Ok(new { message = "User action notified" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying user action");
            return StatusCode(500, new { error = "Failed to notify user action" });
        }
    }

    /// <summary>
    /// Notify role assignment
    /// </summary>
    [HttpPost("notify/role-assignment")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> NotifyRoleAssignment([FromBody] RoleAssignmentRequest request)
    {
        try
        {
            await _realTimeService.NotifyRoleAssignmentAsync(
                request.UserId,
                request.RoleName,
                request.Action);

            _logger.LogInformation("Role assignment notified: {UserId} - {RoleName} - {Action}",
                request.UserId, request.RoleName, request.Action);

            return Ok(new { message = "Role assignment notified" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying role assignment");
            return StatusCode(500, new { error = "Failed to notify role assignment" });
        }
    }

    /// <summary>
    /// Notify system event
    /// </summary>
    [HttpPost("notify/system-event")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> NotifySystemEvent([FromBody] SystemEventRequest request)
    {
        try
        {
            await _realTimeService.NotifySystemEventAsync(
                request.EventType,
                request.Description,
                request.Data);

            _logger.LogInformation("System event notified: {EventType} - {Description}",
                request.EventType, request.Description);

            return Ok(new { message = "System event notified" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying system event");
            return StatusCode(500, new { error = "Failed to notify system event" });
        }
    }

    /// <summary>
    /// Health check for real-time service
    /// </summary>
    [HttpGet("health")]
    [AllowAnonymous]
    public IActionResult Health()
    {
        return Ok(new
        {
            status = "healthy",
            service = "real-time",
            timestamp = DateTime.UtcNow,
            websocketEndpoint = "/hubs/admin"
        });
    }
}

// Request DTOs
public class GenerationMetricsRequest
{
    public int WebsitesGenerated { get; set; }
    public int ImagesGenerated { get; set; }
    public int FailedGenerations { get; set; }
    public double AverageGenerationTime { get; set; }
}

public class RevenueUpdateRequest
{
    public decimal DailyRevenue { get; set; }
    public decimal TotalRevenue30d { get; set; }
}

public class AlertRequest
{
    public string Type { get; set; } = string.Empty;  // warning, error, success, info
    public string Message { get; set; } = string.Empty;
    public object? Data { get; set; }
}

public class AlertMessageRequest
{
    public string Message { get; set; } = string.Empty;
    public object? Data { get; set; }
}

public class PerformanceAlertRequest
{
    public string Metric { get; set; } = string.Empty;
    public double Value { get; set; }
    public string Threshold { get; set; } = string.Empty;
}

public class UserActionRequest
{
    public Guid UserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public object? Details { get; set; }
}

public class RoleAssignmentRequest
{
    public Guid UserId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;  // assigned, revoked
}

public class SystemEventRequest
{
    public string EventType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public object? Data { get; set; }
}
