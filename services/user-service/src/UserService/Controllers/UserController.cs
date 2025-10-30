using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using UserService.Services;
using UserService.Services.Cache;
using UserService.Models;

namespace UserService.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IUserManagementService _userService;
    private readonly ISubscriptionService _subscriptionService;
    private readonly ICacheService _cache;
    private readonly ILogger<UserController> _logger;

    public UserController(
        IUserManagementService userService,
        ISubscriptionService subscriptionService,
        ICacheService cache,
        ILogger<UserController> logger)
    {
        _userService = userService;
        _subscriptionService = subscriptionService;
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Health check endpoint
    /// </summary>
    [HttpGet("health")]
    [AllowAnonymous]
    public IActionResult Health()
    {
        return Ok(new
        {
            status = "healthy",
            service = "user-service",
            timestamp = DateTime.UtcNow,
            version = "1.0.0"
        });
    }

    /// <summary>
    /// Get current user profile
    /// </summary>
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var cacheKey = $"user-profile:{userId}";
            var cachedUser = await _cache.GetAsync<dynamic>(cacheKey);
            if (cachedUser != null)
                return Ok(cachedUser);

            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            await _cache.SetAsync(cacheKey, user, TimeSpan.FromMinutes(30));
            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user");
            return StatusCode(500, new { error = "Failed to get user" });
        }
    }

    /// <summary>
    /// Get user by ID (admin only)
    /// </summary>
    [HttpGet("{userId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetUserById(string userId)
    {
        try
        {
            var cacheKey = $"user:{userId}";
            var cachedUser = await _cache.GetAsync<dynamic>(cacheKey);
            if (cachedUser != null)
                return Ok(cachedUser);

            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { error = "User not found" });
            }

            await _cache.SetAsync(cacheKey, user, TimeSpan.FromMinutes(30));
            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user: {UserId}", userId);
            return StatusCode(500, new { error = "Failed to get user" });
        }
    }

    /// <summary>
    /// Get user by email
    /// </summary>
    [HttpGet("email/{email}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetUserByEmail(string email)
    {
        try
        {
            var cacheKey = $"user-email:{email}";
            var cachedUser = await _cache.GetAsync<dynamic>(cacheKey);
            if (cachedUser != null)
                return Ok(cachedUser);

            var user = await _userService.GetUserByEmailAsync(email);
            if (user == null)
            {
                return NotFound(new { error = "User not found" });
            }

            await _cache.SetAsync(cacheKey, user, TimeSpan.FromMinutes(30));
            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by email: {Email}", email);
            return StatusCode(500, new { error = "Failed to get user" });
        }
    }

    /// <summary>
    /// List all users (admin only)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ListUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            if (page < 1 || pageSize < 1 || pageSize > 100)
            {
                return BadRequest(new { error = "Invalid pagination parameters" });
            }

            var users = await _userService.GetUsersAsync(page, pageSize);
            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing users");
            return StatusCode(500, new { error = "Failed to list users" });
        }
    }

    /// <summary>
    /// Update user profile
    /// </summary>
    [HttpPut("{userId}")]
    public async Task<IActionResult> UpdateUser(string userId, [FromBody] UpdateUserRequest request)
    {
        try
        {
            // Check authorization: user can only update their own profile
            var currentUserId = User.FindFirst("sub")?.Value;
            var role = User.FindFirst("role")?.Value;

            if (currentUserId != userId && role != "Admin")
            {
                return Forbid();
            }

            var user = await _userService.UpdateUserAsync(userId, request);

            // Invalidate cache
            await _cache.RemoveAsync($"user-profile:{userId}");
            await _cache.RemoveAsync($"user:{userId}");

            return Ok(user);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "User not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user: {UserId}", userId);
            return StatusCode(500, new { error = "Failed to update user" });
        }
    }

    /// <summary>
    /// Delete user account
    /// </summary>
    [HttpDelete("{userId}")]
    public async Task<IActionResult> DeleteUser(string userId)
    {
        try
        {
            var currentUserId = User.FindFirst("sub")?.Value;
            var role = User.FindFirst("role")?.Value;

            if (currentUserId != userId && role != "Admin")
            {
                return Forbid();
            }

            var success = await _userService.DeleteUserAsync(userId);
            if (!success)
            {
                return NotFound(new { error = "User not found" });
            }

            return Ok(new { message = "User deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user: {UserId}", userId);
            return StatusCode(500, new { error = "Failed to delete user" });
        }
    }

    /// <summary>
    /// Get user subscription
    /// </summary>
    [HttpGet("{userId}/subscription")]
    public async Task<IActionResult> GetSubscription(string userId)
    {
        try
        {
            var currentUserId = User.FindFirst("sub")?.Value;
            var role = User.FindFirst("role")?.Value;

            if (currentUserId != userId && role != "Admin")
            {
                return Forbid();
            }

            var cacheKey = $"subscription:{userId}";
            var cachedSubscription = await _cache.GetAsync<dynamic>(cacheKey);
            if (cachedSubscription != null)
                return Ok(cachedSubscription);

            var subscription = await _subscriptionService.GetSubscriptionAsync(userId);
            if (subscription == null)
            {
                return NotFound(new { error = "Subscription not found" });
            }

            await _cache.SetAsync(cacheKey, subscription, TimeSpan.FromMinutes(30));
            return Ok(subscription);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subscription: {UserId}", userId);
            return StatusCode(500, new { error = "Failed to get subscription" });
        }
    }

    /// <summary>
    /// Upgrade plan
    /// </summary>
    [HttpPost("{userId}/subscription/upgrade")]
    public async Task<IActionResult> UpgradePlan(string userId, [FromBody] UpgradePlanRequest request)
    {
        try
        {
            var currentUserId = User.FindFirst("sub")?.Value;
            if (currentUserId != userId)
            {
                return Forbid();
            }

            if (string.IsNullOrEmpty(request.PlanType))
            {
                return BadRequest(new { error = "PlanType is required" });
            }

            var subscription = await _subscriptionService.UpgradePlanAsync(userId, request.PlanType);

            // Invalidate cache
            await _cache.RemoveAsync($"subscription:{userId}");

            return Ok(subscription);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "Subscription not found" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error upgrading plan: {UserId}", userId);
            return StatusCode(500, new { error = "Failed to upgrade plan" });
        }
    }

    /// <summary>
    /// Cancel subscription
    /// </summary>
    [HttpPost("{userId}/subscription/cancel")]
    public async Task<IActionResult> CancelSubscription(string userId)
    {
        try
        {
            var currentUserId = User.FindFirst("sub")?.Value;
            if (currentUserId != userId)
            {
                return Forbid();
            }

            var subscription = await _subscriptionService.CancelSubscriptionAsync(userId);

            // Invalidate cache
            await _cache.RemoveAsync($"subscription:{userId}");

            return Ok(subscription);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "Subscription not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling subscription: {UserId}", userId);
            return StatusCode(500, new { error = "Failed to cancel subscription" });
        }
    }

    /// <summary>
    /// Update usage statistics
    /// </summary>
    [HttpPost("{userId}/usage")]
    [Authorize(Roles = "Service")]
    public async Task<IActionResult> UpdateUsage(string userId, [FromBody] UpdateUsageRequest request)
    {
        try
        {
            var success = await _subscriptionService.UpdateUsageAsync(
                userId,
                request.GenerationCount,
                request.StorageUsedGb);

            if (!success)
            {
                return NotFound(new { error = "User not found" });
            }

            return Ok(new { message = "Usage updated" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating usage: {UserId}", userId);
            return StatusCode(500, new { error = "Failed to update usage" });
        }
    }

    /// <summary>
    /// Record user login
    /// </summary>
    [HttpPost("{userId}/login")]
    [AllowAnonymous]
    public async Task<IActionResult> RecordLogin(string userId)
    {
        try
        {
            var success = await _userService.UpdateLastLoginAsync(userId);
            if (!success)
            {
                return NotFound(new { error = "User not found" });
            }

            return Ok(new { message = "Login recorded" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording login: {UserId}", userId);
            return StatusCode(500, new { error = "Failed to record login" });
        }
    }
}

// ============================================================================
// REQUEST DTOs
// ============================================================================

public class UpgradePlanRequest
{
    public string PlanType { get; set; } = string.Empty;
}

public class UpdateUsageRequest
{
    public int GenerationCount { get; set; }
    public decimal StorageUsedGb { get; set; }
}
