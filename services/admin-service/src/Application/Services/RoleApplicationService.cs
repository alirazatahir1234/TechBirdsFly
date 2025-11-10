namespace AdminService.Application.Services;

using AdminService.Application.Interfaces;
using AdminService.Domain.Entities;
using Microsoft.Extensions.Logging;

/// <summary>
/// Application service for Role business logic
/// </summary>
public class RoleApplicationService : IRoleApplicationService
{
    private readonly IRoleRepository _roleRepository;
    private readonly ILogger<RoleApplicationService> _logger;

    public RoleApplicationService(
        IRoleRepository roleRepository,
        ILogger<RoleApplicationService> logger)
    {
        _roleRepository = roleRepository;
        _logger = logger;
    }

    public async Task<Role> CreateRoleAsync(string name, string description, List<string> permissions, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating role: {RoleName}", name);

        // Check if role already exists
        var existingRole = await _roleRepository.GetByNameAsync(name, cancellationToken);
        if (existingRole != null)
            throw new InvalidOperationException($"Role with name {name} already exists");

        // Create the role
        var role = Role.Create(name, description, permissions ?? new());
        await _roleRepository.AddAsync(role, cancellationToken);
        await _roleRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("✅ Role created: {RoleId}", role.Id);
        return role;
    }

    public async Task<Role?> GetRoleAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Fetching role: {RoleId}", id);
        return await _roleRepository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<IEnumerable<Role>> GetAllRolesAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Fetching all roles");
        return await _roleRepository.GetAllAsync(cancellationToken);
    }

    public async Task UpdateRoleAsync(Guid id, string description, List<string> permissions, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating role: {RoleId}", id);

        var role = await _roleRepository.GetByIdAsync(id, cancellationToken);
        if (role == null)
            throw new InvalidOperationException($"Role with ID {id} not found");

        role.Update(description, permissions ?? new());
        await _roleRepository.UpdateAsync(role, cancellationToken);
        await _roleRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("✅ Role updated: {RoleId}", id);
    }

    public async Task DeleteRoleAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting role: {RoleId}", id);

        var role = await _roleRepository.GetByIdAsync(id, cancellationToken);
        if (role == null)
            throw new InvalidOperationException($"Role with ID {id} not found");

        if (role.IsSystem)
            throw new InvalidOperationException("System roles cannot be deleted");

        await _roleRepository.DeleteAsync(id, cancellationToken);
        await _roleRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("✅ Role deleted: {RoleId}", id);
    }

    public async Task AddPermissionToRoleAsync(Guid roleId, string permission, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Adding permission {Permission} to role: {RoleId}", permission, roleId);

        var role = await _roleRepository.GetByIdAsync(roleId, cancellationToken);
        if (role == null)
            throw new InvalidOperationException($"Role with ID {roleId} not found");

        role.AddPermission(permission);
        await _roleRepository.UpdateAsync(role, cancellationToken);
        await _roleRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("✅ Permission added to role: {RoleId}", roleId);
    }

    public async Task RemovePermissionFromRoleAsync(Guid roleId, string permission, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Removing permission {Permission} from role: {RoleId}", permission, roleId);

        var role = await _roleRepository.GetByIdAsync(roleId, cancellationToken);
        if (role == null)
            throw new InvalidOperationException($"Role with ID {roleId} not found");

        role.RemovePermission(permission);
        await _roleRepository.UpdateAsync(role, cancellationToken);
        await _roleRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("✅ Permission removed from role: {RoleId}", roleId);
    }
}
