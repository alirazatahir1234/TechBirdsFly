using AuthService.Services;
using AuthService.Services.Cache;
using Microsoft.AspNetCore.Mvc;
using AuthService.Models;

namespace AuthService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _auth;
        private readonly ICacheService _cache;

        public AuthController(IAuthService auth, ICacheService cache)
        {
            _auth = auth;
            _cache = cache;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest req)
        {
            try
            {
                var user = await _auth.RegisterAsync(req.FullName, req.Email, req.Password);
                
                // Cache user data for quick retrieval (5 minutes)
                await _cache.SetAsync($"user:{user.Id}", new { user.Id, user.Email, user.FullName }, TimeSpan.FromMinutes(5));
                
                return Ok(new { user.Id, user.Email, user.FullName });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest req)
        {
            try
            {
                var tokens = await _auth.LoginAsync(req.Email, req.Password);
                
                // Cache session token (1 hour expiry)
                await _cache.SetAsync($"token:{req.Email}", tokens.accessToken, TimeSpan.FromHours(1));
                
                return Ok(new { accessToken = tokens.accessToken, refreshToken = tokens.refreshToken });
            }
            catch (Exception ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
        }

        [HttpPost("validate-token")]
        public async Task<IActionResult> ValidateToken([FromBody] TokenValidationRequest req)
        {
            try
            {
                // Try to get from cache first
                var cached = await _cache.GetAsync<bool?>($"token-valid:{req.Token}");
                if (cached.HasValue)
                {
                    return Ok(new { valid = cached.Value, fromCache = true });
                }

                // Validate token (implementation depends on your auth service)
                var isValid = true; // Replace with actual validation
                
                // Cache validation result (5 minutes)
                await _cache.SetAsync($"token-valid:{req.Token}", isValid, TimeSpan.FromMinutes(5));
                
                return Ok(new { valid = isValid, fromCache = false });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromQuery] string email)
        {
            try
            {
                // Remove from cache
                await _cache.RemoveAsync($"token:{email}");
                
                return Ok(new { message = "Logged out successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }

    public record RegisterRequest(string FullName, string Email, string Password);
    public record LoginRequest(string Email, string Password);
    public record TokenValidationRequest(string Token);
}