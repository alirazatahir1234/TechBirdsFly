using Microsoft.AspNetCore.Mvc;
using TechBirdsFly.AdminService.Application.DTOs;
using TechBirdsFly.AdminService.Application.Interfaces;

namespace TechBirdsFly.AdminService.WebAPI.Controllers;

/// <summary>
/// Admin Users Controller - Handles CRUD operations for admin users.
/// Manages user creation, updating, suspension, banning, and login tracking.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AdminUsersController : ControllerBase
{
    private readonly IAdminUserApplicationService _adminUserService;
    private readonly IAuditLogApplicationService _auditLogService;
    private readonly ILogger<AdminUsersController> _logger;

    public AdminUsersController(
        IAdminUserApplicationService adminUserService,
        IAuditLogApplicationService auditLogService,
        ILogger<AdminUsersController> logger)
    {
        _adminUserService = adminUserService ?? throw new ArgumentNullException(nameof(adminUserService));
        _auditLogService = auditLogService ?? throw new ArgumentNullException(nameof(auditLogService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get all admin users with their roles.
    /// </summary>
    /// <returns>List of admin users</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<AdminUserDto>>>> GetAllAdminUsers(
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Fetching all admin users");
            var adminUsers = await _adminUserService.GetAllAdminUsersAsync(cancellationToken);
            return Ok(ApiResponse<IEnumerable<AdminUserDto>>.Success(adminUsers, "Admin users retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching admin users");
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<IEnumerable<AdminUserDto>>.ErrorResponse("Failed to retrieve admin users", new[] { ex.Message }));
        }
    }

    /// <summary>
    /// Get a specific admin user by ID.
    /// </summary>
    /// <param name="id">Admin user ID</param>
    /// <returns>Admin user details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<AdminUserDto>>> GetAdminUserById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
            return BadRequest(ApiResponse<AdminUserDto>.ErrorResponse("Invalid admin user ID", new[] { "ID cannot be empty" }));

        try
        {
            _logger.LogInformation("Fetching admin user with ID: {AdminUserId}", id);
            var adminUser = await _adminUserService.GetAdminUserAsync(id, cancellationToken);

            if (adminUser == null)
            {
                _logger.LogWarning("Admin user not found: {AdminUserId}", id);
                return NotFound(ApiResponse<AdminUserDto>.ErrorResponse("Admin user not found", new[] { $"No admin user with ID {id}" }));
            }

            return Ok(ApiResponse<AdminUserDto>.Success(adminUser, "Admin user retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching admin user {AdminUserId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<AdminUserDto>.ErrorResponse("Failed to retrieve admin user", new[] { ex.Message }));
        }
    }

    /// <summary>
    /// Create a new admin user.
    /// </summary>
    /// <param name="request">Admin user creation request</param>
    /// <returns>Created admin user</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<AdminUserDto>>> CreateAdminUser(
        [FromBody] CreateAdminUserRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<AdminUserDto>.ErrorResponse("Invalid request", ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToArray()));

        try
        {
            _logger.LogInformation("Creating new admin user: {Email}", request.Email);
            var adminUser = await _adminUserService.CreateAdminUserAsync(request.Email, request.FullName, cancellationToken);

            _logger.LogInformation("Admin user created successfully: {AdminUserId}", adminUser.Id);
            return CreatedAtAction(nameof(GetAdminUserById), new { id = adminUser.Id },
                ApiResponse<AdminUserDto>.Success(adminUser, "Admin user created successfully"));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Duplicate admin user email: {Email}", request.Email);
            return BadRequest(ApiResponse<AdminUserDto>.ErrorResponse("Invalid request", new[] { ex.Message }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating admin user: {Email}", request.Email);
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<AdminUserDto>.ErrorResponse("Failed to create admin user", new[] { ex.Message }));
        }
    }

    /// <summary>
    /// Update an existing admin user.
    /// </summary>
    /// <param name="id">Admin user ID</param>
    /// <param name="request">Admin user update request</param>
    /// <returns>Updated admin user</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<AdminUserDto>>> UpdateAdminUser(
        Guid id,
        [FromBody] UpdateAdminUserRequest request,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
            return BadRequest(ApiResponse<AdminUserDto>.ErrorResponse("Invalid request", new[] { "ID cannot be empty" }));

        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<AdminUserDto>.ErrorResponse("Invalid request", ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToArray()));

        try
        {
            _logger.LogInformation("Updating admin user: {AdminUserId}", id);
            var adminUser = await _adminUserService.UpdateAdminUserAsync(id, request, cancellationToken);

            if (adminUser == null)
                return NotFound(ApiResponse<AdminUserDto>.ErrorResponse("Admin user not found", new[] { $"No admin user with ID {id}" }));

            _logger.LogInformation("Admin user updated successfully: {AdminUserId}", id);
            return Ok(ApiResponse<AdminUserDto>.Success(adminUser, "Admin user updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating admin user {AdminUserId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<AdminUserDto>.ErrorResponse("Failed to update admin user", new[] { ex.Message }));
        }
    }

    /// <summary>
    /// Suspend an admin user (prevent access but keep data).
    /// </summary>
    /// <param name="id">Admin user ID</param>
    /// <param name="request">Suspension request with reason</param>
    /// <returns>Suspended admin user</returns>
    [HttpPost("{id}/suspend")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<AdminUserDto>>> SuspendAdminUser(
        Guid id,
        [FromBody] SuspendAdminUserRequest request,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
            return BadRequest(ApiResponse<AdminUserDto>.ErrorResponse("Invalid request", new[] { "ID cannot be empty" }));

        try
        {
            _logger.LogInformation("Suspending admin user: {AdminUserId}", id);
            var adminUser = await _adminUserService.SuspendAdminUserAsync(id, request.Reason, cancellationToken);

            if (adminUser == null)
                return NotFound(ApiResponse<AdminUserDto>.ErrorResponse("Admin user not found", new[] { $"No admin user with ID {id}" }));

            _logger.LogInformation("Admin user suspended: {AdminUserId}", id);
            return Ok(ApiResponse<AdminUserDto>.Success(adminUser, "Admin user suspended successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error suspending admin user {AdminUserId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<AdminUserDto>.ErrorResponse("Failed to suspend admin user", new[] { ex.Message }));
        }
    }

    /// <summary>
    /// Unsuspend an admin user (restore access).
    /// </summary>
    /// <param name="id">Admin user ID</param>
    /// <returns>Unsuspended admin user</returns>
    [HttpPost("{id}/unsuspend")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<AdminUserDto>>> UnsuspendAdminUser(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
            return BadRequest(ApiResponse<AdminUserDto>.ErrorResponse("Invalid request", new[] { "ID cannot be empty" }));

        try
        {
            _logger.LogInformation("Unsuspending admin user: {AdminUserId}", id);
            var adminUser = await _adminUserService.UnsuspendAdminUserAsync(id, cancellationToken);

            if (adminUser == null)
                return NotFound(ApiResponse<AdminUserDto>.ErrorResponse("Admin user not found", new[] { $"No admin user with ID {id}" }));

            _logger.LogInformation("Admin user unsuspended: {AdminUserId}", id);
            return Ok(ApiResponse<AdminUserDto>.Success(adminUser, "Admin user unsuspended successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unsuspending admin user {AdminUserId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<AdminUserDto>.ErrorResponse("Failed to unsuspend admin user", new[] { ex.Message }));
        }
    }

    /// <summary>
    /// Ban an admin user (permanent action).
    /// </summary>
    /// <param name="id">Admin user ID</param>
    /// <returns>Banned admin user</returns>
    [HttpPost("{id}/ban")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<AdminUserDto>>> BanAdminUser(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
            return BadRequest(ApiResponse<AdminUserDto>.ErrorResponse("Invalid request", new[] { "ID cannot be empty" }));

        try
        {
            _logger.LogInformation("Banning admin user: {AdminUserId}", id);
            var adminUser = await _adminUserService.BanAdminUserAsync(id, cancellationToken);

            if (adminUser == null)
                return NotFound(ApiResponse<AdminUserDto>.ErrorResponse("Admin user not found", new[] { $"No admin user with ID {id}" }));

            _logger.LogInformation("Admin user banned: {AdminUserId}", id);
            return Ok(ApiResponse<AdminUserDto>.Success(adminUser, "Admin user banned successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error banning admin user {AdminUserId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<AdminUserDto>.ErrorResponse("Failed to ban admin user", new[] { ex.Message }));
        }
    }
}
