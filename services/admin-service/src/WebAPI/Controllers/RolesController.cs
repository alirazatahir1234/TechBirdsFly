using Microsoft.AspNetCore.Mvc;
using AdminService.Application.DTOs;
using AdminService.Application.Interfaces;
using AdminService.Domain.Entities;

namespace AdminService.WebAPI.Controllers;

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
    /// Maps a Role entity to RoleDto
    /// </summary>
    private static RoleDto MapToDto(Role role)
    {
        return new RoleDto
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            Permissions = role.Permissions?.ToList() ?? new(),
            IsSystem = role.IsSystem,
            CreatedAt = role.CreatedAt,
            UpdatedAt = role.UpdatedAt
        };
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
            var roleDtos = roles.Select(MapToDto).ToList();
            return Ok(ApiResponse<IEnumerable<RoleDto>>.SuccessResponse(roleDtos, "Roles retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching roles");
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<IEnumerable<RoleDto>>.ErrorResponse("Failed to retrieve roles", new List<string> { ex.Message }));
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
            return BadRequest(ApiResponse<RoleDto>.ErrorResponse("Invalid role ID", new List<string> { "ID cannot be empty" }));

        try
        {
            _logger.LogInformation("Fetching role with ID: {RoleId}", id);
            var role = await _roleService.GetRoleAsync(id, cancellationToken);

            if (role == null)
            {
                _logger.LogWarning("Role not found: {RoleId}", id);
                return NotFound(ApiResponse<RoleDto>.ErrorResponse("Role not found", new List<string> { $"No role with ID {id}" }));
            }

            return Ok(ApiResponse<RoleDto>.SuccessResponse(MapToDto(role), "Role retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching role {RoleId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<RoleDto>.ErrorResponse("Failed to retrieve role", new List<string> { ex.Message }));
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
            return BadRequest(ApiResponse<RoleDto>.ErrorResponse("Invalid request", ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));

        try
        {
            _logger.LogInformation("Creating new role: {RoleName}", request.Name);
            var role = await _roleService.CreateRoleAsync(request.Name, request.Description, request.Permissions, cancellationToken);

            _logger.LogInformation("Role created successfully: {RoleId}", role.Id);
            return CreatedAtAction(nameof(GetRoleById), new { id = role.Id },
                ApiResponse<RoleDto>.SuccessResponse(MapToDto(role), "Role created successfully"));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Invalid role creation request: {Message}", ex.Message);
            return BadRequest(ApiResponse<RoleDto>.ErrorResponse("Invalid request", new List<string> { ex.Message }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating role: {RoleName}", request.Name);
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<RoleDto>.ErrorResponse("Failed to create role", new List<string> { ex.Message }));
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
            return BadRequest(ApiResponse<RoleDto>.ErrorResponse("Invalid request", new List<string> { "ID cannot be empty" }));

        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<RoleDto>.ErrorResponse("Invalid request", ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));

        try
        {
            _logger.LogInformation("Updating role: {RoleId}", id);
            await _roleService.UpdateRoleAsync(id, request.Description, request.Permissions, cancellationToken);
            var role = await _roleService.GetRoleAsync(id, cancellationToken);

            if (role == null)
                return NotFound(ApiResponse<RoleDto>.ErrorResponse("Role not found", new List<string> { $"No role with ID {id}" }));

            _logger.LogInformation("Role updated successfully: {RoleId}", id);
            return Ok(ApiResponse<RoleDto>.SuccessResponse(MapToDto(role), "Role updated successfully"));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Invalid role update request: {Message}", ex.Message);
            return BadRequest(ApiResponse<RoleDto>.ErrorResponse("Invalid request", new List<string> { ex.Message }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating role {RoleId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<RoleDto>.ErrorResponse("Failed to update role", new List<string> { ex.Message }));
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
            return BadRequest(ApiResponse<object>.ErrorResponse("Invalid request", new List<string> { "ID cannot be empty" }));

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
            return BadRequest(ApiResponse<object>.ErrorResponse("Invalid request", new List<string> { ex.Message }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting role {RoleId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<object>.ErrorResponse("Failed to delete role", new List<string> { ex.Message }));
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
            return BadRequest(ApiResponse<RoleDto>.ErrorResponse("Invalid request", new List<string> { "ID cannot be empty" }));

        if (string.IsNullOrWhiteSpace(request.Permission))
            return BadRequest(ApiResponse<RoleDto>.ErrorResponse("Invalid request", new List<string> { "Permission cannot be empty" }));

        try
        {
            _logger.LogInformation("Adding permission '{Permission}' to role {RoleId}", request.Permission, id);
            await _roleService.AddPermissionToRoleAsync(id, request.Permission, cancellationToken);
            var role = await _roleService.GetRoleAsync(id, cancellationToken);

            if (role == null)
                return NotFound(ApiResponse<RoleDto>.ErrorResponse("Role not found", new List<string> { $"No role with ID {id}" }));

            _logger.LogInformation("Permission added to role successfully: {RoleId}", id);
            return Ok(ApiResponse<RoleDto>.SuccessResponse(MapToDto(role), "Permission added successfully"));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Invalid permission request: {Message}", ex.Message);
            return BadRequest(ApiResponse<RoleDto>.ErrorResponse("Invalid request", new List<string> { ex.Message }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding permission to role {RoleId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<RoleDto>.ErrorResponse("Failed to add permission", new List<string> { ex.Message }));
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
            return BadRequest(ApiResponse<RoleDto>.ErrorResponse("Invalid request", new List<string> { "ID cannot be empty" }));

        if (string.IsNullOrWhiteSpace(request.Permission))
            return BadRequest(ApiResponse<RoleDto>.ErrorResponse("Invalid request", new List<string> { "Permission cannot be empty" }));

        try
        {
            _logger.LogInformation("Removing permission '{Permission}' from role {RoleId}", request.Permission, id);
            await _roleService.RemovePermissionFromRoleAsync(id, request.Permission, cancellationToken);
            var role = await _roleService.GetRoleAsync(id, cancellationToken);

            if (role == null)
                return NotFound(ApiResponse<RoleDto>.ErrorResponse("Role not found", new List<string> { $"No role with ID {id}" }));

            _logger.LogInformation("Permission removed from role successfully: {RoleId}", id);
            return Ok(ApiResponse<RoleDto>.SuccessResponse(MapToDto(role), "Permission removed successfully"));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Invalid permission request: {Message}", ex.Message);
            return BadRequest(ApiResponse<RoleDto>.ErrorResponse("Invalid request", new List<string> { ex.Message }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing permission from role {RoleId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<RoleDto>.ErrorResponse("Failed to remove permission", new List<string> { ex.Message }));
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
