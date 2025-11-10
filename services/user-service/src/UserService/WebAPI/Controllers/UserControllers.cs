using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UserService.Application.DTOs;
using UserService.Application.Interfaces;

namespace UserService.WebAPI.Controllers;

/// <summary>
/// Authentication controller for user registration, login, and token management
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AuthResponse>> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("User registration attempt for {Username}", request.Username);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _authService.RegisterAsync(request, cancellationToken);

            if (!response.Success)
                return BadRequest(new { message = response.Message });

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user registration");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Registration failed" });
        }
    }

    /// <summary>
    /// Login user with credentials
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AuthResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("User login attempt for {Username}", request.Username);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _authService.LoginAsync(request, cancellationToken);

            if (!response.Success)
                return Unauthorized(new { message = response.Message });

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user login");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Login failed" });
        }
    }

    /// <summary>
    /// Verify user email
    /// </summary>
    [HttpPost("verify-email")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AuthResponse>> VerifyEmail(
        [FromBody] VerifyEmailRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _authService.VerifyEmailAsync(request, cancellationToken);

            if (!response.Success)
                return BadRequest(new { message = response.Message });

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying email");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Email verification failed" });
        }
    }

    /// <summary>
    /// Resend verification email
    /// </summary>
    [HttpPost("resend-verification-email")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse>> ResendVerificationEmail(
        [FromBody] ForgotPasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.ResendVerificationEmailAsync(request.Email, cancellationToken);

            if (!result)
                return BadRequest(new { message = "Unable to resend verification email" });

            return Ok(new ApiResponse(true, "Verification email sent"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resending verification email");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to resend verification email" });
        }
    }

    /// <summary>
    /// Forgot password request
    /// </summary>
    [HttpPost("forgot-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse>> ForgotPassword(
        [FromBody] ForgotPasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.ForgotPasswordAsync(request, cancellationToken);

            if (!result)
                return BadRequest(new { message = "Unable to process forgot password request" });

            return Ok(new ApiResponse(true, "Password reset email sent"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing forgot password");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to process forgot password request" });
        }
    }

    /// <summary>
    /// Reset password with token
    /// </summary>
    [HttpPost("reset-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse>> ResetPassword(
        [FromBody] ResetPasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.ResetPasswordAsync(request, cancellationToken);

            if (!result)
                return BadRequest(new { message = "Unable to reset password" });

            return Ok(new ApiResponse(true, "Password reset successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting password");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to reset password" });
        }
    }

    /// <summary>
    /// Logout user
    /// </summary>
    [Authorize]
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse>> Logout(CancellationToken cancellationToken = default)
    {
        try
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized();

            await _authService.LogoutAsync(userId, cancellationToken);
            return Ok(new ApiResponse(true, "Logged out successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Logout failed" });
        }
    }

    /// <summary>
    /// Validate token
    /// </summary>
    [HttpPost("validate-token")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse>> ValidateToken(
        [FromBody] TokenValidationRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var isValid = await _authService.ValidateTokenAsync(request.Token, cancellationToken);
            return isValid ? Ok(new ApiResponse(true, "Token is valid")) : Unauthorized(new ApiResponse(false, "Invalid token"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating token");
            return Unauthorized(new ApiResponse(false, "Token validation failed"));
        }
    }
}

/// <summary>
/// User management controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IProfileService _profileService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(
        IUserService userService,
        IProfileService profileService,
        ILogger<UsersController> logger)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _profileService = profileService ?? throw new ArgumentNullException(nameof(profileService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetUser(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userService.GetUserByIdAsync(id, cancellationToken);

            if (user == null)
                return NotFound(new ApiResponse<UserDto>(false, null, "User not found"));

            return Ok(new ApiResponse<UserDto>(true, user));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user {UserId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<UserDto>(false, null, "Error retrieving user"));
        }
    }

    /// <summary>
    /// Get current user
    /// </summary>
    [HttpGet("profile/current")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetCurrentUser(CancellationToken cancellationToken = default)
    {
        try
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized();

            var user = await _userService.GetUserByIdAsync(userId, cancellationToken);

            if (user == null)
                return NotFound(new ApiResponse<UserDto>(false, null, "User not found"));

            return Ok(new ApiResponse<UserDto>(true, user));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user");
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<UserDto>(false, null, "Error retrieving user"));
        }
    }

    /// <summary>
    /// Get users with pagination
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<UserListItemDto>>>> GetUsers(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool ascending = true,
        [FromQuery] string? filterByRole = null,
        [FromQuery] string? filterByStatus = null,
        [FromQuery] string? searchTerm = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new ListUsersQuery(pageNumber, pageSize, sortBy, ascending, filterByRole, filterByStatus, searchTerm);
            var result = await _userService.GetUsersAsync(query, cancellationToken);

            return Ok(new ApiResponse<PaginatedResponse<UserListItemDto>>(true, result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting users");
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<PaginatedResponse<UserListItemDto>>(false, null, "Error retrieving users"));
        }
    }

    /// <summary>
    /// Update user profile
    /// </summary>
    [HttpPut("{id}/profile")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<UserDto>>> UpdateProfile(
        [FromRoute] Guid id,
        [FromBody] UpdateProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Verify user can only update their own profile
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var currentUserId))
                return Unauthorized();

            if (currentUserId != id)
                return Forbid();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _userService.UpdateUserAsync(id, request, cancellationToken);
            return Ok(new ApiResponse<UserDto>(true, result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user profile {UserId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<UserDto>(false, null, "Error updating profile"));
        }
    }

    /// <summary>
    /// Change user password
    /// </summary>
    [HttpPost("{id}/change-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse>> ChangePassword(
        [FromRoute] Guid id,
        [FromBody] ChangePasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var currentUserId))
                return Unauthorized();

            if (currentUserId != id)
                return Forbid();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Use auth service to change password
            var authService = HttpContext.RequestServices.GetRequiredService<IAuthService>();
            var result = await authService.ChangePasswordAsync(id, request, cancellationToken);

            if (!result)
                return BadRequest(new ApiResponse(false, "Unable to change password"));

            return Ok(new ApiResponse(true, "Password changed successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password for user {UserId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse(false, "Error changing password"));
        }
    }

    /// <summary>
    /// Deactivate user account
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPost("{id}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse>> DeactivateUser(
        [FromRoute] Guid id,
        [FromBody] DeactivateUserRequest? request = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _userService.DeactivateUserAsync(id, request?.Reason, cancellationToken);

            if (!result)
                return NotFound(new ApiResponse(false, "User not found"));

            return Ok(new ApiResponse(true, "User deactivated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating user {UserId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse(false, "Error deactivating user"));
        }
    }

    /// <summary>
    /// Reactivate user account
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPost("{id}/reactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse>> ReactivateUser(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _userService.ReactivateUserAsync(id, cancellationToken);

            if (!result)
                return NotFound(new ApiResponse(false, "User not found"));

            return Ok(new ApiResponse(true, "User reactivated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reactivating user {UserId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse(false, "Error reactivating user"));
        }
    }

    /// <summary>
    /// Assign role to user
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPost("{id}/assign-role")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse>> AssignRole(
        [FromRoute] Guid id,
        [FromBody] AssignRoleRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var assignedBy))
                return Unauthorized();

            var result = await _userService.AssignRoleAsync(id, request.Role, assignedBy, cancellationToken);

            if (!result)
                return NotFound(new ApiResponse(false, "User not found or invalid role"));

            return Ok(new ApiResponse(true, "Role assigned successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning role to user {UserId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse(false, "Error assigning role"));
        }
    }

    /// <summary>
    /// Get user statistics (Admin only)
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpGet("admin/statistics")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<UserStatisticsDto>>> GetStatistics(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var stats = await _userService.GetUserStatisticsAsync(cancellationToken);
            return Ok(new ApiResponse<UserStatisticsDto>(true, stats));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user statistics");
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<UserStatisticsDto>(false, null, "Error retrieving statistics"));
        }
    }
}

#region Supporting DTOs for Controllers

public record TokenValidationRequest(string Token);

public record DeactivateUserRequest(string? Reason = null);

#endregion
