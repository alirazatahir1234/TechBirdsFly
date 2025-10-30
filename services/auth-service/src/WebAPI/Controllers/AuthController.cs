using Microsoft.AspNetCore.Mvc;
using AuthService.Application.DTOs;
using AuthService.Application.Services;
using AuthService.Application.Interfaces;

namespace AuthService.WebAPI.Controllers;

/// <summary>
/// Authentication endpoints
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthApplicationService _auth;
    private readonly ILogger<AuthController> _logger;


    private readonly ICacheService _cache;

    public AuthController(AuthApplicationService authService, ILogger<AuthController> logger, ICacheService cache)
    {
        _auth = authService;
        _logger = logger;
        _cache = cache;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequestDto req, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _auth.RegisterAsync(req, cancellationToken);

            // Cache user data for quick retrieval (5 minutes)
            await _cache.SetAsync($"user:{user.UserId}", new { user.UserId, user.Email }, TimeSpan.FromMinutes(5), cancellationToken);

            return Ok(new { user.UserId, user.Email, user.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequestDto req, CancellationToken cancellationToken)
    {
        try
        {
            var tokens = await _auth.LoginAsync(req, cancellationToken);

            // Cache session token (1 hour expiry)
            await _cache.SetAsync($"token:{req.Email}", tokens.AccessToken, TimeSpan.FromHours(1), cancellationToken);

            return Ok(new { accessToken = tokens.AccessToken, refreshToken = tokens.RefreshToken });
        }
        catch (Exception ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get current user profile
    /// </summary>
    [HttpGet("profile/{userId}")]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProfile(Guid userId, CancellationToken cancellationToken)
    {
        try
        {
            var profile = await _auth.GetProfileAsync(userId, cancellationToken);
            return Ok(profile);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("User not found: {UserId}", userId);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving profile for user: {UserId}", userId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred" });
        }
    }

    /// <summary>
    /// Confirm email address
    /// </summary>
    [HttpPost("confirm-email/{userId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ConfirmEmail(Guid userId, CancellationToken cancellationToken)
    {
        try
        {
            await _auth.ConfirmEmailAsync(userId, cancellationToken);
            return Ok(new { message = "Email confirmed successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming email for user: {UserId}", userId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred" });
        }
    }

    /// <summary>
    /// Deactivate user account
    /// </summary>
    [HttpPost("deactivate/{userId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(Guid userId, CancellationToken cancellationToken)
    {
        try
        {
            await _auth.DeactivateAsync(userId, cancellationToken);

            // Invalidate all related caches
            await _cache.RemoveAsync($"user_profile_{userId}", cancellationToken);

            return Ok(new { message = "User account deactivated successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating user: {UserId}", userId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred" });
        }
    }

    [HttpPost("validate-token")]
    public async Task<IActionResult> ValidateToken([FromBody] TokenValidationRequestDto req, CancellationToken cancellationToken)
    {
        try
        {
            // Try to get from cache first
            var cached = await _cache.GetAsync<bool?>($"token-valid:{req.Token}", cancellationToken);
            if (cached.HasValue)
            {
                return Ok(new { valid = cached.Value, fromCache = true });
            }

            // Validate token (implementation depends on your auth service)
            var isValid = true; // Replace with actual validation

            // Cache validation result (5 minutes)
            await _cache.SetAsync($"token-valid:{req.Token}", isValid, TimeSpan.FromMinutes(5), cancellationToken);

            return Ok(new { valid = isValid, fromCache = false });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromQuery] string email, CancellationToken cancellationToken)
    {
        try
        {
            // Remove from cache
            await _cache.RemoveAsync($"token:{email}", cancellationToken);

            return Ok(new { message = "Logged out successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

}
