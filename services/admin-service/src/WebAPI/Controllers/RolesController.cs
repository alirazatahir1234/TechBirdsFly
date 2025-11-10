using Microsoft.AspNetCore.Mvc;
using TechBirdsFly.AdminService.Application.DTOs;
using TechBirdsFly.AdminService.Application.Interfaces;

namespace TechBirdsFly.AdminService.WebAPI.Controllers;

/// <summary>
/// Roles Controller - Handles role management operations.
/// Manages role CRUD operations, permissions, and system role protection.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class RolesController : ControllerBase
{
    private readonly IRoleApplicationService _roleService;
    private readonly ILogger<RolesController> _logger;

    public RolesController(
        IRoleApplicationService roleService,
        ILogger<RolesController> logger)
    {
        _roleService = roleService ?? throw new ArgumentNullException(nameof(roleService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get all roles in the system.
    /// </summary>
    /// <returns>List of all roles</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<RoleDto>>>> GetAllRoles(
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Fetching all roles");
            var roles = await _roleService.GetAllRolesAsync(cancellationToken);
            return Ok(ApiResponse<IEnumerable<RoleDto>>.Success(roles, "Roles retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching roles");
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<IEnumerable<RoleDto>>.ErrorResponse("Failed to retrieve roles", new[] { ex.Message }));
        }
    }

    /// <summary>
    /// Get a specific role by ID.
    /// </summary>
    /// <param name="id">Role ID</param>
    /// <returns>Role details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<RoleDto>>> GetRoleById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
            return BadRequest(ApiResponse<RoleDto>.ErrorResponse("Invalid role ID", new[] { "ID cannot be empty" }));

        try
        {
            _logger.LogInformation("Fetching role with ID: {RoleId}", id);
            var role = await _roleService.GetRoleAsync(id, cancellationToken);

            if (role == null)
            {
                _logger.LogWarning("Role not found: {RoleId}", id);
                return NotFound(ApiResponse<RoleDto>.ErrorResponse("Role not found", new[] { $"No role with ID {id}" }));
            }

            return Ok(ApiResponse<RoleDto>.Success(role, "Role retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching role {RoleId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<RoleDto>.ErrorResponse("Failed to retrieve role", new[] { ex.Message }));
        }
    }

    /// <summary>
    /// Create a new custom role.
    /// Note: System roles (SuperAdmin, Admin, Moderator) cannot be created via API.
    /// </summary>
    /// <param name="request">Role creation request</param>
    /// <returns>Created role</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<RoleDto>>> CreateRole(
        [FromBody] CreateRoleRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<RoleDto>.ErrorResponse("Invalid request", ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToArray()));

        try
        {
            _logger.LogInformation("Creating new role: {RoleName}", request.Name);
            var role = await _roleService.CreateRoleAsync(request.Name, request.Description, request.Permissions, cancellationToken);

            _logger.LogInformation("Role created successfully: {RoleId}", role.Id);
            return CreatedAtAction(nameof(GetRoleById), new { id = role.Id },
                ApiResponse<RoleDto>.Success(role, "Role created successfully"));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Invalid role creation request: {Message}", ex.Message);
            return BadRequest(ApiResponse<RoleDto>.ErrorResponse("Invalid request", new[] { ex.Message }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating role: {RoleName}", request.Name);
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<RoleDto>.ErrorResponse("Failed to create role", new[] { ex.Message }));
        }
    }

    /// <summary>
    /// Update an existing custom role.
    /// Note: System roles (SuperAdmin, Admin, Moderator) cannot be modified.
    /// </summary>
    /// <param name="id">Role ID</param>
    /// <param name="request">Role update request</param>
    /// <returns>Updated role</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<RoleDto>>> UpdateRole(
        Guid id,
        [FromBody] UpdateRoleRequest request,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
            return BadRequest(ApiResponse<RoleDto>.ErrorResponse("Invalid request", new[] { "ID cannot be empty" }));

        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<RoleDto>.ErrorResponse("Invalid request", ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToArray()));

        try
        {
            _logger.LogInformation("Updating role: {RoleId}", id);
            var role = await _roleService.UpdateRoleAsync(id, request.Description, request.Permissions, cancellationToken);

            if (role == null)
                return NotFound(ApiResponse<RoleDto>.ErrorResponse("Role not found", new[] { $"No role with ID {id}" }));

            _logger.LogInformation("Role updated successfully: {RoleId}", id);
            return Ok(ApiResponse<RoleDto>.Success(role, "Role updated successfully"));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Invalid role update request: {Message}", ex.Message);
            return BadRequest(ApiResponse<RoleDto>.ErrorResponse("Invalid request", new[] { ex.Message }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating role {RoleId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<RoleDto>.ErrorResponse("Failed to update role", new[] { ex.Message }));
        }
    }

    /// <summary>
    /// Delete a custom role.
    /// Note: System roles (SuperAdmin, Admin, Moderator) cannot be deleted.
    /// </summary>
    /// <param name="id">Role ID</param>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteRole(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
            return BadRequest(ApiResponse<object>.ErrorResponse("Invalid request", new[] { "ID cannot be empty" }));

        try
        {
            _logger.LogInformation("Deleting role: {RoleId}", id);
            await _roleService.DeleteRoleAsync(id, cancellationToken);

            _logger.LogInformation("Role deleted successfully: {RoleId}", id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Cannot delete system role: {RoleId}", id);
            return BadRequest(ApiResponse<object>.ErrorResponse("Invalid request", new[] { ex.Message }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting role {RoleId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<object>.ErrorResponse("Failed to delete role", new[] { ex.Message }));
        }
    }

    /// <summary>
    /// Add a permission to a role.
    /// Note: System roles cannot be modified.
    /// </summary>
    /// <param name="id">Role ID</param>
    /// <param name="request">Permission to add</param>
    /// <returns>Updated role</returns>
    [HttpPost("{id}/permissions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<RoleDto>>> AddPermissionToRole(
        Guid id,
        [FromBody] PermissionRequest request,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
            return BadRequest(ApiResponse<RoleDto>.ErrorResponse("Invalid request", new[] { "ID cannot be empty" }));

        if (string.IsNullOrWhiteSpace(request.Permission))
            return BadRequest(ApiResponse<RoleDto>.ErrorResponse("Invalid request", new[] { "Permission cannot be empty" }));

        try
        {
            _logger.LogInformation("Adding permission '{Permission}' to role {RoleId}", request.Permission, id);
            var role = await _roleService.AddPermissionToRoleAsync(id, request.Permission, cancellationToken);

            if (role == null)
                return NotFound(ApiResponse<RoleDto>.ErrorResponse("Role not found", new[] { $"No role with ID {id}" }));

            _logger.LogInformation("Permission added to role successfully: {RoleId}", id);
            return Ok(ApiResponse<RoleDto>.Success(role, "Permission added successfully"));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Invalid permission request: {Message}", ex.Message);
            return BadRequest(ApiResponse<RoleDto>.ErrorResponse("Invalid request", new[] { ex.Message }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding permission to role {RoleId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<RoleDto>.ErrorResponse("Failed to add permission", new[] { ex.Message }));
        }
    }

    /// <summary>
    /// Remove a permission from a role.
    /// Note: System roles cannot be modified.
    /// </summary>
    /// <param name="id">Role ID</param>
    /// <param name="request">Permission to remove</param>
    /// <returns>Updated role</returns>
    [HttpDelete("{id}/permissions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<RoleDto>>> RemovePermissionFromRole(
        Guid id,
        [FromBody] PermissionRequest request,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
            return BadRequest(ApiResponse<RoleDto>.ErrorResponse("Invalid request", new[] { "ID cannot be empty" }));

        if (string.IsNullOrWhiteSpace(request.Permission))
            return BadRequest(ApiResponse<RoleDto>.ErrorResponse("Invalid request", new[] { "Permission cannot be empty" }));

        try
        {
            _logger.LogInformation("Removing permission '{Permission}' from role {RoleId}", request.Permission, id);
            var role = await _roleService.RemovePermissionFromRoleAsync(id, request.Permission, cancellationToken);

            if (role == null)
                return NotFound(ApiResponse<RoleDto>.ErrorResponse("Role not found", new[] { $"No role with ID {id}" }));

            _logger.LogInformation("Permission removed from role successfully: {RoleId}", id);
            return Ok(ApiResponse<RoleDto>.Success(role, "Permission removed successfully"));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Invalid permission request: {Message}", ex.Message);
            return BadRequest(ApiResponse<RoleDto>.ErrorResponse("Invalid request", new[] { ex.Message }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing permission from role {RoleId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<RoleDto>.ErrorResponse("Failed to remove permission", new[] { ex.Message }));
        }
    }
}

/// <summary>
/// Permission request DTO for adding/removing permissions.
/// </summary>
public class PermissionRequest
{
    /// <summary>
    /// Permission to add or remove (e.g., "admin.users.view").
    /// </summary>
    public required string Permission { get; set; }
}
