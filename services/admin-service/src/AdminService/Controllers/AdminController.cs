using Microsoft.AspNetCore.Mvc;
using AdminService.Services;
using AdminService.Services.Cache;
using AdminService.Models;

namespace AdminService.Controllers;

[ApiController]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readonly IUserManagementService _userService;
    private readonly IRoleManagementService _roleService;
    private readonly IAnalyticsService _analyticsService;
    private readonly ICacheService _cache;
    private readonly ILogger<AdminController> _logger;

    public AdminController(
        IAdminService adminService,
        IUserManagementService userService,
        IRoleManagementService roleService,
        IAnalyticsService analyticsService,
        ICacheService cache,
        ILogger<AdminController> logger)
    {
        _adminService = adminService;
        _userService = userService;
        _roleService = roleService;
        _analyticsService = analyticsService;
        _cache = cache;
        _logger = logger;
    }

    // Audit Logs
    [HttpGet("audit-logs")]
    public async Task<IActionResult> GetAuditLogs([FromQuery] int limit = 100)
    {
        var logs = await _adminService.GetAuditLogsAsync(limit);
        return Ok(logs);
    }

    [HttpGet("audit-logs/user/{userId}")]
    public async Task<IActionResult> GetUserAuditLogs(Guid userId, [FromQuery] int limit = 50)
    {
        var logs = await _adminService.GetUserAuditLogsAsync(userId, limit);
        return Ok(logs);
    }

    // Templates
    [HttpGet("templates")]
    public async Task<IActionResult> GetActiveTemplates()
    {
        var cacheKey = "admin-templates:all";
        var cachedTemplates = await _cache.GetAsync<dynamic>(cacheKey);
        if (cachedTemplates != null)
            return Ok(cachedTemplates);

        var templates = await _adminService.GetActiveTemplatesAsync();
        await _cache.SetAsync(cacheKey, templates, TimeSpan.FromHours(1));
        return Ok(templates);
    }

    [HttpGet("templates/category/{category}")]
    public async Task<IActionResult> GetTemplatesByCategory(string category)
    {
        var cacheKey = $"admin-templates:category:{category}";
        var cachedTemplates = await _cache.GetAsync<dynamic>(cacheKey);
        if (cachedTemplates != null)
            return Ok(cachedTemplates);

        var templates = await _adminService.GetTemplatesByCategoryAsync(category);
        await _cache.SetAsync(cacheKey, templates, TimeSpan.FromHours(1));
        return Ok(templates);
    }

    [HttpGet("templates/{id}")]
    public async Task<IActionResult> GetTemplate(Guid id)
    {
        var cacheKey = $"admin-template:{id}";
        var cachedTemplate = await _cache.GetAsync<dynamic>(cacheKey);
        if (cachedTemplate != null)
            return Ok(cachedTemplate);

        var template = await _adminService.GetTemplateAsync(id);
        if (template == null)
            return NotFound();

        await _cache.SetAsync(cacheKey, template, TimeSpan.FromHours(1));
        return Ok(template);
    }

    [HttpPost("templates")]
    public async Task<IActionResult> CreateTemplate([FromBody] CreateTemplateRequest request)
    {
        var template = new Template
        {
            Name = request.Name,
            Category = request.Category,
            Description = request.Description,
            ThumbnailUrl = request.ThumbnailUrl,
            HtmlTemplate = request.HtmlTemplate,
            CssTemplate = request.CssTemplate,
            IsActive = true,
            Priority = request.Priority ?? 0
        };

        var created = await _adminService.CreateTemplateAsync(template);

        // Invalidate cache
        await _cache.RemoveAsync("admin-templates:all");
        await _cache.RemoveAsync($"admin-templates:category:{request.Category}");

        return CreatedAtAction(nameof(GetTemplate), new { id = created.Id }, created);
    }

    [HttpPut("templates/{id}")]
    public async Task<IActionResult> UpdateTemplate(Guid id, [FromBody] UpdateTemplateRequest request)
    {
        try
        {
            var template = new Template
            {
                Name = request.Name,
                Category = request.Category,
                Description = request.Description,
                ThumbnailUrl = request.ThumbnailUrl,
                HtmlTemplate = request.HtmlTemplate,
                CssTemplate = request.CssTemplate,
                IsActive = request.IsActive ?? true,
                Priority = request.Priority ?? 0
            };

            var updated = await _adminService.UpdateTemplateAsync(id, template);

            // Invalidate cache
            await _cache.RemoveAsync($"admin-template:{id}");
            await _cache.RemoveAsync("admin-templates:all");
            await _cache.RemoveAsync($"admin-templates:category:{request.Category}");

            return Ok(updated);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpDelete("templates/{id}")]
    public async Task<IActionResult> DeleteTemplate(Guid id)
    {
        try
        {
            await _adminService.DeleteTemplateAsync(id);

            // Invalidate cache
            await _cache.RemoveAsync($"admin-template:{id}");
            await _cache.RemoveAsync("admin-templates:all");

            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    // System Settings
    [HttpGet("settings/{key}")]
    public async Task<IActionResult> GetSetting(string key)
    {
        var value = await _adminService.GetSettingAsync(key);
        if (value == null)
            return NotFound();
        return Ok(new { key, value });
    }

    [HttpPost("settings")]
    public async Task<IActionResult> SetSetting([FromBody] SetSettingRequest request)
    {
        await _adminService.SetSettingAsync(request.Key, request.Value);
        return Ok(new { message = "Setting updated" });
    }

    [HttpDelete("settings/{key}")]
    public async Task<IActionResult> DeleteSetting(string key)
    {
        await _adminService.DeleteSettingAsync(key);
        return NoContent();
    }

    // User Management (Phase 2)
    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _userService.GetAllUsersAsync();
        return Ok(users);
    }

    [HttpGet("users/{id}")]
    public async Task<IActionResult> GetUser(Guid id)
    {
        try
        {
            var user = await _userService.GetUserAsync(id);
            return Ok(user);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "User not found" });
        }
    }

    [HttpPost("users")]
    public async Task<IActionResult> CreateUser([FromBody] CreateAdminUserRequest request)
    {
        var user = new AdminUser
        {
            Email = request.Email,
            FullName = request.FullName,
            Status = "active",
            CreatedAt = DateTime.UtcNow
        };

        var created = await _userService.CreateUserAsync(user);
        return CreatedAtAction(nameof(GetUser), new { id = created.Id }, created);
    }

    [HttpPut("users/{id}")]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateAdminUserRequest request)
    {
        try
        {
            var user = await _userService.GetUserAsync(id);
            if (user != null)
            {
                user.FullName = request.FullName;
            }

            var updated = await _userService.UpdateUserAsync(id, user!);
            return Ok(updated);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "User not found" });
        }
    }

    [HttpDelete("users/{id}")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        try
        {
            await _userService.DeleteUserAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "User not found" });
        }
    }

    [HttpPost("users/{id}/suspend")]
    public async Task<IActionResult> SuspendUser(Guid id, [FromBody] SuspendUserRequest request)
    {
        try
        {
            await _userService.SuspendUserAsync(id, request.Reason);
            return Ok(new { message = "User suspended" });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "User not found" });
        }
    }

    [HttpPost("users/{id}/unsuspend")]
    public async Task<IActionResult> UnsuspendUser(Guid id)
    {
        try
        {
            await _userService.UnsuspendUserAsync(id);
            return Ok(new { message = "User unsuspended" });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "User not found" });
        }
    }

    [HttpPost("users/{id}/ban")]
    public async Task<IActionResult> BanUser(Guid id, [FromBody] BanUserRequest request)
    {
        try
        {
            await _userService.BanUserAsync(id, request.Reason);
            return Ok(new { message = "User banned" });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "User not found" });
        }
    }

    // Role Management (Phase 2)
    [HttpGet("roles")]
    public async Task<IActionResult> GetAllRoles()
    {
        var roles = await _roleService.GetAllRolesAsync();
        return Ok(roles);
    }

    [HttpGet("roles/{id}")]
    public async Task<IActionResult> GetRole(Guid id)
    {
        try
        {
            var role = await _roleService.GetRoleAsync(id);
            return Ok(role);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Role not found" });
        }
    }

    [HttpPost("roles")]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequest request)
    {
        var role = new Role
        {
            Name = request.Name,
            Description = request.Description,
            Permissions = request.Permissions,
            IsSystem = false,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _roleService.CreateRoleAsync(role);
        return CreatedAtAction(nameof(GetRole), new { id = created.Id }, created);
    }

    [HttpPut("roles/{id}")]
    public async Task<IActionResult> UpdateRole(Guid id, [FromBody] UpdateRoleRequest request)
    {
        try
        {
            var role = await _roleService.GetRoleAsync(id);
            if (role != null)
            {
                role.Description = request.Description;
                role.Permissions = request.Permissions;
                role.UpdatedAt = DateTime.UtcNow;
            }

            var updated = await _roleService.UpdateRoleAsync(id, role!);
            return Ok(updated);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Role not found" });
        }
    }

    [HttpDelete("roles/{id}")]
    public async Task<IActionResult> DeleteRole(Guid id)
    {
        try
        {
            await _roleService.DeleteRoleAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("users/{userId}/roles/{roleId}")]
    public async Task<IActionResult> AssignRoleToUser(Guid userId, Guid roleId)
    {
        try
        {
            await _roleService.AssignRoleToUserAsync(userId, roleId);
            return Ok(new { message = "Role assigned to user" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpDelete("users/{userId}/roles/{roleId}")]
    public async Task<IActionResult> RevokeRoleFromUser(Guid userId, Guid roleId)
    {
        try
        {
            await _roleService.RevokeRoleFromUserAsync(userId, roleId);
            return Ok(new { message = "Role revoked from user" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("users/{userId}/roles")]
    public async Task<IActionResult> GetUserRoles(Guid userId)
    {
        var roles = await _roleService.GetUserRolesAsync(userId);
        return Ok(roles);
    }

    [HttpGet("users/{userId}/permissions")]
    public async Task<IActionResult> GetUserPermissions(Guid userId)
    {
        var permissions = await _roleService.GetUserPermissionsAsync(userId);
        return Ok(new { userId, permissions = permissions.ToList() });
    }

    // Analytics (Phase 2)
    [HttpGet("analytics/daily/{date}")]
    public async Task<IActionResult> GetDailyStats(string date)
    {
        if (!DateTime.TryParse(date, out var parsedDate))
            return BadRequest(new { message = "Invalid date format. Use YYYY-MM-DD" });

        var cacheKey = $"admin-analytics:daily:{date}";
        var cachedStats = await _cache.GetAsync<dynamic>(cacheKey);
        if (cachedStats != null)
            return Ok(cachedStats);

        var stats = await _analyticsService.GetDailyStatsAsync(parsedDate);
        if (stats == null)
            return NotFound(new { message = "No statistics found for this date" });

        await _cache.SetAsync(cacheKey, stats, TimeSpan.FromHours(24));
        return Ok(stats);
    }

    [HttpGet("analytics/range")]
    public async Task<IActionResult> GetStatsRange([FromQuery] string from, [FromQuery] string to)
    {
        if (!DateTime.TryParse(from, out var fromDate) || !DateTime.TryParse(to, out var toDate))
            return BadRequest(new { message = "Invalid date format. Use YYYY-MM-DD" });

        var cacheKey = $"admin-analytics:range:{from}:{to}";
        var cachedStats = await _cache.GetAsync<dynamic>(cacheKey);
        if (cachedStats != null)
            return Ok(cachedStats);

        var stats = await _analyticsService.GetStatsRangeAsync(fromDate, toDate);
        await _cache.SetAsync(cacheKey, stats, TimeSpan.FromHours(24));
        return Ok(stats);
    }

    [HttpGet("analytics/revenue")]
    public async Task<IActionResult> GetTotalRevenue([FromQuery] string from, [FromQuery] string to)
    {
        if (!DateTime.TryParse(from, out var fromDate) || !DateTime.TryParse(to, out var toDate))
            return BadRequest(new { message = "Invalid date format. Use YYYY-MM-DD" });

        var cacheKey = $"admin-analytics:revenue:{from}:{to}";
        var cachedRevenue = await _cache.GetAsync<decimal?>(cacheKey);
        if (cachedRevenue.HasValue)
            return Ok(new { totalRevenue = cachedRevenue, from = fromDate, to = toDate });

        var revenue = await _analyticsService.GetTotalRevenueAsync(fromDate, toDate);
        await _cache.SetAsync(cacheKey, revenue, TimeSpan.FromHours(24));
        return Ok(new { totalRevenue = revenue, from = fromDate, to = toDate });
    }

    [HttpGet("analytics/websites-generated")]
    public async Task<IActionResult> GetWebsitesGenerated([FromQuery] string from, [FromQuery] string to)
    {
        if (!DateTime.TryParse(from, out var fromDate) || !DateTime.TryParse(to, out var toDate))
            return BadRequest(new { message = "Invalid date format. Use YYYY-MM-DD" });

        var cacheKey = $"admin-analytics:websites:{from}:{to}";
        var cachedCount = await _cache.GetAsync<int?>(cacheKey);
        if (cachedCount.HasValue)
            return Ok(new { websitesGenerated = cachedCount, from = fromDate, to = toDate });

        var count = await _analyticsService.GetTotalWebsitesGeneratedAsync(fromDate, toDate);
        await _cache.SetAsync(cacheKey, count, TimeSpan.FromHours(24));
        return Ok(new { websitesGenerated = count, from = fromDate, to = toDate });
    }

    [HttpGet("analytics/images-generated")]
    public async Task<IActionResult> GetImagesGenerated([FromQuery] string from, [FromQuery] string to)
    {
        if (!DateTime.TryParse(from, out var fromDate) || !DateTime.TryParse(to, out var toDate))
            return BadRequest(new { message = "Invalid date format. Use YYYY-MM-DD" });

        var cacheKey = $"admin-analytics:images:{from}:{to}";
        var cachedCount = await _cache.GetAsync<int?>(cacheKey);
        if (cachedCount.HasValue)
            return Ok(new { imagesGenerated = cachedCount, from = fromDate, to = toDate });

        var count = await _analyticsService.GetTotalImagesGeneratedAsync(fromDate, toDate);
        await _cache.SetAsync(cacheKey, count, TimeSpan.FromHours(24));
        return Ok(new { imagesGenerated = count, from = fromDate, to = toDate });
    }

    [HttpGet("analytics/avg-generation-time")]
    public async Task<IActionResult> GetAvgGenerationTime([FromQuery] string from, [FromQuery] string to)
    {
        if (!DateTime.TryParse(from, out var fromDate) || !DateTime.TryParse(to, out var toDate))
            return BadRequest(new { message = "Invalid date format. Use YYYY-MM-DD" });

        var cacheKey = $"admin-analytics:avgtime:{from}:{to}";
        var cachedAvgTime = await _cache.GetAsync<double?>(cacheKey);
        if (cachedAvgTime.HasValue)
            return Ok(new { averageGenerationTimeSeconds = Math.Round(cachedAvgTime.Value, 2), from = fromDate, to = toDate });

        var avgTime = await _analyticsService.GetAverageGenerationTimeAsync(fromDate, toDate);
        await _cache.SetAsync(cacheKey, avgTime, TimeSpan.FromHours(24));
        return Ok(new { averageGenerationTimeSeconds = Math.Round(avgTime, 2), from = fromDate, to = toDate });
    }

    [HttpGet("analytics/failed-generations")]
    public async Task<IActionResult> GetFailedGenerations([FromQuery] string from, [FromQuery] string to)
    {
        if (!DateTime.TryParse(from, out var fromDate) || !DateTime.TryParse(to, out var toDate))
            return BadRequest(new { message = "Invalid date format. Use YYYY-MM-DD" });

        var cacheKey = $"admin-analytics:failed:{from}:{to}";
        var cachedCount = await _cache.GetAsync<int?>(cacheKey);
        if (cachedCount.HasValue)
            return Ok(new { failedGenerations = cachedCount, from = fromDate, to = toDate });

        var count = await _analyticsService.GetFailedGenerationsCountAsync(fromDate, toDate);
        await _cache.SetAsync(cacheKey, count, TimeSpan.FromHours(24));
        return Ok(new { failedGenerations = count, from = fromDate, to = toDate });
    }

    [HttpGet("analytics/summary")]
    public async Task<IActionResult> GetPlatformSummary()
    {
        var cacheKey = "admin-analytics:summary";
        var cachedSummary = await _cache.GetAsync<dynamic>(cacheKey);
        if (cachedSummary != null)
            return Ok(cachedSummary);

        var summary = await _analyticsService.GetPlatformSummaryAsync();
        await _cache.SetAsync(cacheKey, summary, TimeSpan.FromMinutes(30));
        return Ok(summary);
    }

    [HttpPost("analytics/record-daily")]
    public async Task<IActionResult> RecordDailyStats([FromBody] RecordDailyStatsRequest request)
    {
        var stats = new DailyStatistic
        {
            Date = request.Date,
            NewUsersCount = request.NewUsersCount,
            ActiveUsersCount = request.ActiveUsersCount,
            WebsitesGeneratedCount = request.WebsitesGeneratedCount,
            ImagesGeneratedCount = request.ImagesGeneratedCount,
            RevenueTotal = request.RevenueTotal,
            AverageUserSpend = request.AverageUserSpend,
            FailedGenerations = request.FailedGenerations,
            AverageGenerationTime = request.AverageGenerationTime
        };

        await _analyticsService.RecordDailyStatsAsync(stats);
        return Ok(new { message = "Daily statistics recorded" });
    }

    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
    }
}

public class CreateTemplateRequest
{
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public string HtmlTemplate { get; set; } = string.Empty;
    public string CssTemplate { get; set; } = string.Empty;
    public int? Priority { get; set; }
}

public class UpdateTemplateRequest
{
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public string HtmlTemplate { get; set; } = string.Empty;
    public string CssTemplate { get; set; } = string.Empty;
    public bool? IsActive { get; set; }
    public int? Priority { get; set; }
}

public class SetSettingRequest
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

// Phase 2 Request DTOs
public class CreateAdminUserRequest
{
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
}

public class UpdateAdminUserRequest
{
    public string FullName { get; set; } = string.Empty;
}

public class SuspendUserRequest
{
    public string Reason { get; set; } = string.Empty;
}

public class BanUserRequest
{
    public string Reason { get; set; } = string.Empty;
}

public class CreateRoleRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Permissions { get; set; } = new();
}

public class UpdateRoleRequest
{
    public string Description { get; set; } = string.Empty;
    public List<string> Permissions { get; set; } = new();
}

public class RecordDailyStatsRequest
{
    public DateTime Date { get; set; }
    public int NewUsersCount { get; set; }
    public int ActiveUsersCount { get; set; }
    public int WebsitesGeneratedCount { get; set; }
    public int ImagesGeneratedCount { get; set; }
    public decimal RevenueTotal { get; set; }
    public decimal AverageUserSpend { get; set; }
    public int FailedGenerations { get; set; }
    public double AverageGenerationTime { get; set; }
}
