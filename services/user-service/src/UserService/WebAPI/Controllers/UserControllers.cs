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
/// User Management Controller
/// 
/// Handles all user-related operations including profile management, role assignment,
/// and account status operations. Authentication-related operations (login, register, etc.)
/// are handled by the AuthController.
/// 
/// Authorization:
/// - [Authorize] - Requires authentication token
/// - [Authorize(Roles = "Admin")] - Requires Admin role
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IProfileService _profileService;
    private readonly ILogger<UsersController> _logger;

    /// <summary>
    /// Initializes a new instance of the UsersController
    /// </summary>
    public UsersController(
        IUserService userService,
        IProfileService profileService,
        ILogger<UsersController> logger)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _profileService = profileService ?? throw new ArgumentNullException(nameof(profileService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // =========================================================================
    // GET USER BY ID (Admin + User Owner)
    // =========================================================================

    /// <summary>
    /// Get user by ID
    /// 
    /// Only Admin users or the user owner can retrieve a user profile.
    /// </summary>
    /// <param name="id">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User details or error</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetUser(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Ensure user can access only their profile unless Admin
            var userId = GetUserId();

            if (userId != id && !User.IsInRole("Admin"))
                return Forbid();

            var user = await _userService.GetUserByIdAsync(id, cancellationToken);

            if (user == null)
                return NotFound(new ApiResponse<UserDto>(false, null, "User not found"));

            _logger.LogInformation("User {RequestorId} retrieved user {UserId}", userId, id);
            return Ok(new ApiResponse<UserDto>(true, user));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by ID");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new ApiResponse<UserDto>(false, null, "Error retrieving user"));
        }
    }

    // =========================================================================
    // CURRENT USER PROFILE (Self)
    // =========================================================================

    /// <summary>
    /// Get current authenticated user's profile
    /// 
    /// Returns the profile of the currently authenticated user.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Current user's profile</returns>
    [HttpGet("profile/me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetCurrentUser(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetUserId();
            var user = await _userService.GetUserByIdAsync(userId, cancellationToken);

            if (user == null)
                return NotFound(new ApiResponse<UserDto>(false, null, "User profile not found"));

            return Ok(new ApiResponse<UserDto>(true, user));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving current user profile");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new ApiResponse<UserDto>(false, null, "Error retrieving profile"));
        }
    }

    // =========================================================================
    // UPDATE PROFILE (Self Only)
    // =========================================================================

    /// <summary>
    /// Update current user's profile
    /// 
    /// Users can only update their own profile (firstName, lastName, etc.).
    /// Password changes are handled by the AuthController.
    /// </summary>
    /// <param name="request">Profile update request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated user profile</returns>
    [HttpPut("profile/update")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<UserDto>>> UpdateProfile(
        [FromBody] UpdateProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetUserId();

            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<UserDto>(false, null, "Invalid request"));

            var updated = await _profileService.UpdateProfileAsync(userId, request, cancellationToken);

            _logger.LogInformation("User {UserId} updated their profile", userId);
            return Ok(new ApiResponse<UserDto>(true, updated));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating profile");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new ApiResponse<UserDto>(false, null, "Error updating profile"));
        }
    }

    // =========================================================================
    // LIST USERS (Admin Only)
    // =========================================================================

    /// <summary>
    /// Get paginated list of users with filtering and sorting
    /// 
    /// Admin only. Supports pagination, filtering by role/status, and search.
    /// </summary>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20)</param>
    /// <param name="sortBy">Sort by field (default: null)</param>
    /// <param name="ascending">Sort order (default: true)</param>
    /// <param name="filterByRole">Filter by role (default: null)</param>
    /// <param name="search">Search term (default: null)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of users</returns>
    [Authorize(Roles = "Admin")]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<UserListItemDto>>>> GetUsers(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool ascending = true,
        [FromQuery] string? filterByRole = null,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new ListUsersQuery(pageNumber, pageSize, sortBy, ascending, filterByRole, search);
            var result = await _userService.GetUsersAsync(query, cancellationToken);

            _logger.LogInformation("Admin retrieved user list: page {PageNumber}, size {PageSize}", pageNumber, pageSize);
            return Ok(new ApiResponse<PaginatedResponse<UserListItemDto>>(true, result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing users");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new ApiResponse<PaginatedResponse<UserListItemDto>>(false, null, "Error retrieving users"));
        }
    }

    // =========================================================================
    // ADMIN: DEACTIVATE USER
    // =========================================================================

    /// <summary>
    /// Deactivate a user account
    /// 
    /// Admin only. Sets the user's IsActive flag to false.
    /// </summary>
    /// <param name="id">User ID to deactivate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success or error</returns>
    [Authorize(Roles = "Admin")]
    [HttpPost("{id}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse>> DeactivateUser(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var adminId = GetUserId();
            var success = await _userService.DeactivateUserAsync(id, cancellationToken);

            if (!success)
                return NotFound(new ApiResponse(false, "User not found"));

            _logger.LogInformation("Admin {AdminId} deactivated user {UserId}", adminId, id);
            return Ok(new ApiResponse(true, "User deactivated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating user");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new ApiResponse(false, "Error deactivating user"));
        }
    }

    // =========================================================================
    // ADMIN: REACTIVATE USER
    // =========================================================================

    /// <summary>
    /// Reactivate a deactivated user account
    /// 
    /// Admin only. Sets the user's IsActive flag back to true.
    /// </summary>
    /// <param name="id">User ID to reactivate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success or error</returns>
    [Authorize(Roles = "Admin")]
    [HttpPost("{id}/reactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse>> ReactivateUser(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var adminId = GetUserId();
            var result = await _userService.ReactivateUserAsync(id, cancellationToken);

            if (!result)
                return NotFound(new ApiResponse(false, "User not found"));

            _logger.LogInformation("Admin {AdminId} reactivated user {UserId}", adminId, id);
            return Ok(new ApiResponse(true, "User reactivated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reactivating user");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new ApiResponse(false, "Error reactivating user"));
        }
    }

    // =========================================================================
    // ADMIN: ASSIGN ROLE
    // =========================================================================

    /// <summary>
    /// Assign a role to a user
    /// 
    /// Admin only. Assigns the specified role to the target user.
    /// </summary>
    /// <param name="id">User ID</param>
    /// <param name="request">Role assignment request containing the role</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success or error</returns>
    [Authorize(Roles = "Admin")]
    [HttpPost("{id}/assign-role")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse>> AssignRole(
        [FromRoute] Guid id,
        [FromBody] AssignRoleRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var adminId = GetUserId();

            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse(false, "Invalid request"));

            var result = await _userService.AssignRoleAsync(id, request.Role, adminId, cancellationToken);

            if (!result)
                return NotFound(new ApiResponse(false, "User not found or invalid role"));

            _logger.LogInformation("Admin {AdminId} assigned role {Role} to user {UserId}", adminId, request.Role, id);
            return Ok(new ApiResponse(true, "Role assigned successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning role");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new ApiResponse(false, "Error assigning role"));
        }
    }

    // =========================================================================
    // ADMIN: USER STATISTICS
    // =========================================================================

    /// <summary>
    /// Get user statistics and analytics
    /// 
    /// Admin only. Returns statistics like total users, active users, by role, etc.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User statistics</returns>
    [Authorize(Roles = "Admin")]
    [HttpGet("statistics")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<UserStatisticsDto>>> GetStatistics(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var stats = await _userService.GetUserStatisticsAsync(cancellationToken);
            _logger.LogInformation("Admin retrieved user statistics");
            return Ok(new ApiResponse<UserStatisticsDto>(true, stats));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving statistics");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new ApiResponse<UserStatisticsDto>(false, null, "Error retrieving statistics"));
        }
    }

    // =========================================================================
    // HELPER METHODS
    // =========================================================================

    /// <summary>
    /// Extract user ID from JWT token claims
    /// </summary>
    /// <returns>User ID as GUID</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown if token is invalid</exception>
    private Guid GetUserId()
    {
        var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);

        if (claim == null || !Guid.TryParse(claim.Value, out var userId))
            throw new UnauthorizedAccessException("Invalid token: user ID not found");

        return userId;
    }
}

#region Supporting DTOs for User Controller

/// <summary>
/// Request for assigning a role to a user
/// </summary>
public record AssignRoleRequest(string Role);

/// <summary>
/// Request for deactivating a user account
/// </summary>
public record DeactivateUserRequest(string? Reason = null);

#endregion
