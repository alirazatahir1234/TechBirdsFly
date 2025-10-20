using Microsoft.EntityFrameworkCore;
using AdminService.Data;
using AdminService.Models;

namespace AdminService.Services;

public class RoleManagementService : IRoleManagementService
{
    private readonly AdminDbContext _db;
    private readonly ILogger<RoleManagementService> _logger;
    private readonly IAdminService _adminService;

    public RoleManagementService(AdminDbContext db, ILogger<RoleManagementService> logger, IAdminService adminService)
    {
        _db = db;
        _logger = logger;
        _adminService = adminService;
    }

    public async Task<IEnumerable<Role>> GetAllRolesAsync()
    {
        return await _db.Roles.OrderBy(r => r.Name).ToListAsync();
    }

    public async Task<Role?> GetRoleAsync(Guid roleId)
    {
        return await _db.Roles.FirstOrDefaultAsync(r => r.Id == roleId);
    }

    public async Task<Role?> GetRoleByNameAsync(string roleName)
    {
        return await _db.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
    }

    public async Task<Role> CreateRoleAsync(Role role)
    {
        role.Id = Guid.NewGuid();
        role.CreatedAt = DateTime.UtcNow;
        
        _db.Roles.Add(role);
        await _db.SaveChangesAsync();
        
        await _adminService.LogActionAsync(null, "CREATE", "Role", role.Id.ToString(), $"Created role: {role.Name}");
        _logger.LogInformation("Created role: {RoleName}", role.Name);
        
        return role;
    }

    public async Task<Role> UpdateRoleAsync(Guid roleId, Role role)
    {
        var existing = await GetRoleAsync(roleId);
        if (existing == null)
            throw new KeyNotFoundException($"Role {roleId} not found");

        if (existing.IsSystem)
            throw new InvalidOperationException("Cannot modify system roles");

        existing.Name = role.Name;
        existing.Description = role.Description;
        existing.Permissions = role.Permissions;
        existing.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        await _adminService.LogActionAsync(null, "UPDATE", "Role", roleId.ToString(), $"Updated role: {role.Name}");
        
        return existing;
    }

    public async Task DeleteRoleAsync(Guid roleId)
    {
        var role = await GetRoleAsync(roleId);
        if (role == null)
            throw new KeyNotFoundException($"Role {roleId} not found");

        if (role.IsSystem)
            throw new InvalidOperationException("Cannot delete system roles");

        // Check if role is assigned to any users
        var assignedCount = await _db.UserRoles.CountAsync(ur => ur.RoleId == roleId && ur.RevokedAt == null);
        if (assignedCount > 0)
            throw new InvalidOperationException($"Cannot delete role assigned to {assignedCount} users");

        _db.Roles.Remove(role);
        await _db.SaveChangesAsync();
        
        await _adminService.LogActionAsync(null, "DELETE", "Role", roleId.ToString(), $"Deleted role: {role.Name}");
        _logger.LogInformation("Deleted role: {RoleId}", roleId);
    }

    public async Task AssignRoleToUserAsync(Guid userId, Guid roleId)
    {
        var role = await GetRoleAsync(roleId);
        if (role == null)
            throw new KeyNotFoundException($"Role {roleId} not found");

        // Check if user already has this role
        var existing = await _db.UserRoles.FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId && ur.RevokedAt == null);
        if (existing != null)
            throw new InvalidOperationException("User already has this role");

        var userRole = new UserRole
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            RoleId = roleId,
            AssignedAt = DateTime.UtcNow
        };

        _db.UserRoles.Add(userRole);
        await _db.SaveChangesAsync();
        
        await _adminService.LogActionAsync(null, "ASSIGN_ROLE", "UserRole", userRole.Id.ToString(), $"Assigned role {role.Name} to user");
        _logger.LogInformation("Assigned role {RoleId} to user {UserId}", roleId, userId);
    }

    public async Task RevokeRoleFromUserAsync(Guid userId, Guid roleId)
    {
        var userRole = await _db.UserRoles.FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId && ur.RevokedAt == null);
        if (userRole == null)
            throw new KeyNotFoundException("User role assignment not found");

        userRole.RevokedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        
        await _adminService.LogActionAsync(null, "REVOKE_ROLE", "UserRole", userRole.Id.ToString(), $"Revoked role from user");
        _logger.LogInformation("Revoked role {RoleId} from user {UserId}", roleId, userId);
    }

    public async Task<IEnumerable<Role>> GetUserRolesAsync(Guid userId)
    {
        return await _db.UserRoles
            .Where(ur => ur.UserId == userId && ur.RevokedAt == null)
            .Select(ur => ur.RoleId)
            .Join(_db.Roles, rid => rid, r => r.Id, (_, r) => r)
            .ToListAsync();
    }

    public async Task<IEnumerable<string>> GetUserPermissionsAsync(Guid userId)
    {
        var roles = await GetUserRolesAsync(userId);
        var permissions = new HashSet<string>();

        foreach (var role in roles)
        {
            foreach (var permission in role.Permissions)
            {
                permissions.Add(permission);
            }
        }

        return permissions;
    }

    public async Task<bool> UserHasPermissionAsync(Guid userId, string permission)
    {
        var permissions = await GetUserPermissionsAsync(userId);
        return permissions.Contains(permission);
    }
}
