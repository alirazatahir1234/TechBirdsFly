using AdminService.Application.DTOs;
using AdminService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AdminService.WebAPI.Controllers;

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

    /// <summary>
    /// Initializes a new instance of the AdminUsersController.
    /// </summary>
    /// <param name="adminUserService">Service for admin user operations</param>
    /// <param name="auditLogService">Service for audit logging</param>
    /// <param name="logger">Logger instance</param>
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
            var adminUserDtos = adminUsers.Select(MapToDto).ToList();
            return Ok(ApiResponse<IEnumerable<AdminUserDto>>.SuccessResponse(adminUserDtos, "Admin users retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching admin users");
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<IEnumerable<AdminUserDto>>.ErrorResponse("Failed to retrieve admin users", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Get a specific admin user by ID.
    /// </summary>
    /// <param name="id">Admin user ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Admin user details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<AdminUserDto>>> GetAdminUserById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
            return BadRequest(ApiResponse<AdminUserDto>.ErrorResponse("Invalid admin user ID", new List<string> { "ID cannot be empty" }));

        try
        {
            _logger.LogInformation("Fetching admin user with ID: {AdminUserId}", id);
            var adminUser = await _adminUserService.GetAdminUserAsync(id, cancellationToken);

            if (adminUser == null)
            {
                _logger.LogWarning("Admin user not found: {AdminUserId}", id);
                return NotFound(ApiResponse<AdminUserDto>.ErrorResponse("Admin user not found", new List<string> { $"No admin user with ID {id}" }));
            }

            var adminUserDto = MapToDto(adminUser);
            return Ok(ApiResponse<AdminUserDto>.SuccessResponse(adminUserDto, "Admin user retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching admin user {AdminUserId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<AdminUserDto>.ErrorResponse("Failed to retrieve admin user", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Create a new admin user.
    /// </summary>
    /// <param name="request">Admin user creation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created admin user</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<AdminUserDto>>> CreateAdminUser(
        [FromBody] CreateAdminUserRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<AdminUserDto>.ErrorResponse("Invalid request", ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));

        try
        {
            _logger.LogInformation("Creating new admin user: {Email}", request.Email);
            var adminUser = await _adminUserService.CreateAdminUserAsync(request.Email, request.FullName, cancellationToken);

            var adminUserDto = MapToDto(adminUser);
            _logger.LogInformation("Admin user created successfully: {AdminUserId}", adminUser.Id);
            return CreatedAtAction(nameof(GetAdminUserById), new { id = adminUser.Id },
                ApiResponse<AdminUserDto>.SuccessResponse(adminUserDto, "Admin user created successfully"));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Duplicate admin user email: {Email}", request.Email);
            return BadRequest(ApiResponse<AdminUserDto>.ErrorResponse("Invalid request", new List<string> { ex.Message }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating admin user: {Email}", request.Email);
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<AdminUserDto>.ErrorResponse("Failed to create admin user", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Update an existing admin user.
    /// </summary>
    /// <param name="id">Admin user ID</param>
    /// <param name="request">Admin user update request</param>
    /// <param name="cancellationToken">Cancellation token</param>
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
            return BadRequest(ApiResponse<AdminUserDto>.ErrorResponse("Invalid request", new List<string> { "ID cannot be empty" }));

        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<AdminUserDto>.ErrorResponse("Invalid request", ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));

        try
        {
            _logger.LogInformation("Updating admin user: {AdminUserId}", id);
            await _adminUserService.UpdateAdminUserAsync(id, request.FullName, request.ProjectCount, request.TotalSpent, cancellationToken);

            var adminUser = await _adminUserService.GetAdminUserAsync(id, cancellationToken);
            if (adminUser == null)
                return NotFound(ApiResponse<AdminUserDto>.ErrorResponse("Admin user not found", new List<string> { $"No admin user with ID {id}" }));

            var adminUserDto = MapToDto(adminUser);
            _logger.LogInformation("Admin user updated successfully: {AdminUserId}", id);
            return Ok(ApiResponse<AdminUserDto>.SuccessResponse(adminUserDto, "Admin user updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating admin user {AdminUserId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<AdminUserDto>.ErrorResponse("Failed to update admin user", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Suspend an admin user (prevent access but keep data).
    /// </summary>
    /// <param name="id">Admin user ID</param>
    /// <param name="request">Suspension request with reason</param>
    /// <param name="cancellationToken">Cancellation token</param>
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
            return BadRequest(ApiResponse<AdminUserDto>.ErrorResponse("Invalid request", new List<string> { "ID cannot be empty" }));

        try
        {
            _logger.LogInformation("Suspending admin user: {AdminUserId}", id);
            await _adminUserService.SuspendAdminUserAsync(id, request.Reason, cancellationToken);

            var adminUser = await _adminUserService.GetAdminUserAsync(id, cancellationToken);
            if (adminUser == null)
                return NotFound(ApiResponse<AdminUserDto>.ErrorResponse("Admin user not found", new List<string> { $"No admin user with ID {id}" }));

            var adminUserDto = MapToDto(adminUser);
            _logger.LogInformation("Admin user suspended: {AdminUserId}", id);
            return Ok(ApiResponse<AdminUserDto>.SuccessResponse(adminUserDto, "Admin user suspended successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error suspending admin user {AdminUserId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<AdminUserDto>.ErrorResponse("Failed to suspend admin user", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Unsuspend an admin user (restore access).
    /// </summary>
    /// <param name="id">Admin user ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Unsuspended admin user</returns>
    [HttpPost("{id}/unsuspend")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<AdminUserDto>>> UnsuspendAdminUser(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
            return BadRequest(ApiResponse<AdminUserDto>.ErrorResponse("Invalid request", new List<string> { "ID cannot be empty" }));

        try
        {
            _logger.LogInformation("Unsuspending admin user: {AdminUserId}", id);
            await _adminUserService.UnsuspendAdminUserAsync(id, cancellationToken);

            var adminUser = await _adminUserService.GetAdminUserAsync(id, cancellationToken);
            if (adminUser == null)
                return NotFound(ApiResponse<AdminUserDto>.ErrorResponse("Admin user not found", new List<string> { $"No admin user with ID {id}" }));

            var adminUserDto = MapToDto(adminUser);
            _logger.LogInformation("Admin user unsuspended: {AdminUserId}", id);
            return Ok(ApiResponse<AdminUserDto>.SuccessResponse(adminUserDto, "Admin user unsuspended successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unsuspending admin user {AdminUserId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<AdminUserDto>.ErrorResponse("Failed to unsuspend admin user", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Ban an admin user (permanent action).
    /// </summary>
    /// <param name="id">Admin user ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Banned admin user</returns>
    [HttpPost("{id}/ban")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<AdminUserDto>>> BanAdminUser(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
            return BadRequest(ApiResponse<AdminUserDto>.ErrorResponse("Invalid request", new List<string> { "ID cannot be empty" }));

        try
        {
            _logger.LogInformation("Banning admin user: {AdminUserId}", id);
            await _adminUserService.BanAdminUserAsync(id, cancellationToken);

            var adminUser = await _adminUserService.GetAdminUserAsync(id, cancellationToken);
            if (adminUser == null)
                return NotFound(ApiResponse<AdminUserDto>.ErrorResponse("Admin user not found", new List<string> { $"No admin user with ID {id}" }));

            var adminUserDto = MapToDto(adminUser);
            _logger.LogInformation("Admin user banned: {AdminUserId}", id);
            return Ok(ApiResponse<AdminUserDto>.SuccessResponse(adminUserDto, "Admin user banned successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error banning admin user {AdminUserId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<AdminUserDto>.ErrorResponse("Failed to ban admin user", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Helper method to map AdminUser entity to AdminUserDto
    /// </summary>
    private static AdminUserDto MapToDto(dynamic adminUser)
    {
        // Build roles list first
        List<RoleDto> roles = new();
        if (adminUser.Roles != null)
        {
            foreach (var role in adminUser.Roles)
            {
                roles.Add(new RoleDto
                {
                    Id = role.Id,
                    Name = role.Name,
                    Description = role.Description,
                    Permissions = role.Permissions ?? new List<string>(),
                    IsSystem = role.IsSystem,
                    CreatedAt = role.CreatedAt,
                    UpdatedAt = role.UpdatedAt
                });
            }
        }

        // Map AdminUser entity to AdminUserDto
        return new AdminUserDto
        {
            Id = adminUser.Id,
            Email = adminUser.Email,
            FullName = adminUser.FullName,
            Status = adminUser.Status,
            CreatedAt = adminUser.CreatedAt,
            LastLoginAt = adminUser.LastLoginAt,
            SuspendedAt = adminUser.SuspendedAt,
            SuspensionReason = adminUser.SuspensionReason,
            ProjectCount = adminUser.ProjectCount,
            TotalSpent = adminUser.TotalSpent,
            Roles = roles
        };
    }
}
