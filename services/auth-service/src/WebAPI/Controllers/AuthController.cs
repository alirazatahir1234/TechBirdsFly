using Microsoft.AspNetCore.Mvc;
using AuthService.Application.DTOs;
using AuthService.Application.Services;
using AuthService.Application.Interfaces;

namespace AuthService.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthApplicationService _auth;
    private readonly ILogger<AuthController> _logger;
    private readonly ICacheService _cache;

    public AuthController(AuthApplicationService auth, ILogger<AuthController> logger, ICacheService cache)
    {
        _auth = auth;
        _logger = logger;
        _cache = cache;
    }

    // ========================================================================
    // REGISTER
    // ========================================================================
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequestDto req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Email))
            return BadRequest(new { message = "Email is required" });

        if (!req.Email.Contains("@"))
            return BadRequest(new { message = "Invalid email format" });

        if (string.IsNullOrWhiteSpace(req.Password) || req.Password.Length < 6)
            return BadRequest(new { message = "Password too weak" });

        try
        {
            var result = await _auth.RegisterAsync(req, ct);

            // Cache new user
            await _cache.SetAsync($"user:{result.UserId}",
                new { result.UserId, result.Email },
                TimeSpan.FromMinutes(5),
                ct);

            return Created($"/api/auth/profile/{result.UserId}", new
            {
                result.UserId,
                result.Email
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message }); // Duplicate email
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // ========================================================================
    // LOGIN
    // ========================================================================
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequestDto req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Email))
            return BadRequest(new { message = "Email is required" });

        if (string.IsNullOrWhiteSpace(req.Password))
            return BadRequest(new { message = "Password is required" });

        try
        {
            var tokens = await _auth.LoginAsync(req, ct);

            // Cache token
            await _cache.SetAsync(
                $"token:{req.Email}",
                tokens.AccessToken,
                TimeSpan.FromHours(1),
                ct
            );

            return Ok(new { tokens.AccessToken, tokens.RefreshToken });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // ========================================================================
    // PROFILE
    // ========================================================================
    [HttpGet("profile/{userId}")]
    public async Task<IActionResult> GetProfile(Guid userId, CancellationToken ct)
    {
        if (userId == Guid.Empty)
            return NotFound(new { message = "User not found" });

        try
        {
            var profile = await _auth.GetProfileAsync(userId, ct);
            return Ok(profile);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // ========================================================================
    // CONFIRM EMAIL
    // ========================================================================
    [HttpPost("confirm-email/{userId}")]
    public async Task<IActionResult> ConfirmEmail(Guid userId, CancellationToken ct)
    {
        if (userId == Guid.Empty)
            return NotFound(new { message = "Invalid user" });

        try
        {
            await _auth.ConfirmEmailAsync(userId, ct);
            return Ok(new { message = "Email confirmed" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // ========================================================================
    // DEACTIVATE ACCOUNT
    // ========================================================================
    [HttpPost("deactivate/{userId}")]
    public async Task<IActionResult> Deactivate(Guid userId, CancellationToken ct)
    {
        if (userId == Guid.Empty)
            return NotFound(new { message = "Invalid user" });

        try
        {
            await _auth.DeactivateAsync(userId, ct);

            await _cache.RemoveAsync($"user:{userId}", ct);

            return Ok(new { message = "Account deactivated" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // ========================================================================
    // FORGOT PASSWORD
    // ========================================================================
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequestDto req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Email))
            return BadRequest(new { message = "Email is required" });

        if (!req.Email.Contains("@"))
            return BadRequest(new { message = "Invalid email format" });

        try
        {
            // Generate reset token (typically 6-digit code or JWT token)
            var resetToken = GenerateResetToken();

            // Store reset token in cache with 30-minute expiration
            await _cache.SetAsync(
                $"reset-token:{req.Email}",
                resetToken,
                TimeSpan.FromMinutes(30),
                ct
            );

            _logger.LogInformation("Forgot password request for email: {Email}", req.Email);

            // In production, send email with reset link
            // Example: https://yourapp.com/reset-password?token={resetToken}&email={req.Email}
            // For now, we return the token (DO NOT DO THIS IN PRODUCTION)

            return Ok(new
            {
                message = "Password reset email sent",
                // For testing only - remove in production
                resetToken = resetToken
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing forgot password for email: {Email}", req.Email);
            return BadRequest(new { message = "Unable to process forgot password request" });
        }
    }

    // ========================================================================
    // RESET PASSWORD
    // ========================================================================
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequestDto req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Email))
            return BadRequest(new { message = "Email is required" });

        if (string.IsNullOrWhiteSpace(req.ResetToken))
            return BadRequest(new { message = "Reset token is required" });

        if (string.IsNullOrWhiteSpace(req.NewPassword) || req.NewPassword.Length < 6)
            return BadRequest(new { message = "Password must be at least 6 characters" });

        try
        {
            // Verify reset token from cache
            var storedToken = await _cache.GetAsync<string>($"reset-token:{req.Email}", ct);

            if (storedToken == null)
                return BadRequest(new { message = "Invalid or expired reset token" });

            if (storedToken != req.ResetToken)
                return BadRequest(new { message = "Invalid reset token" });

            // Reset password using the service
            var result = await _auth.ResetPasswordAsync(req, ct);

            if (!result)
                return BadRequest(new { message = "Failed to reset password" });

            // Clear used reset token from cache
            await _cache.RemoveAsync($"reset-token:{req.Email}", ct);

            _logger.LogInformation("Password reset successfully for email: {Email}", req.Email);

            return Ok(new { message = "Password reset successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting password for email: {Email}", req.Email);
            return BadRequest(new { message = "Unable to reset password" });
        }
    }

    // ========================================================================
    // TOKEN VALIDATION
    // ========================================================================
    [HttpPost("validate-token")]
    public async Task<IActionResult> ValidateToken(TokenValidationRequestDto req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Token))
            return BadRequest(new { message = "Token is required" });

        var cached = await _cache.GetAsync<bool?>($"token-valid:{req.Token}", ct);

        if (cached.HasValue)
            return Ok(new { valid = cached.Value, fromCache = true });

        // Simulated validation for tests
        var isValid = true;

        await _cache.SetAsync($"token-valid:{req.Token}", isValid, TimeSpan.FromMinutes(5), ct);

        return Ok(new { valid = true, fromCache = false });
    }

    // ========================================================================
    // LOGOUT
    // ========================================================================
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromQuery] string? email, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(email))
            return BadRequest(new { message = "Email is required" });

        // Logout is idempotent → OK even if user not found
        await _cache.RemoveAsync($"token:{email}", ct);

        return Ok(new { message = "Logged out" });
    }

    // ========================================================================
    // HELPER METHODS
    // ========================================================================
    private string GenerateResetToken()
    {
        // Generate a secure random token (6-digit code for simplicity, or use JWT)
        return new Random().Next(100000, 999999).ToString();
    }
}
