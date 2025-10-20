using AdminService.Models;

namespace AdminService.Services;

public interface IRoleManagementService
{
    Task<IEnumerable<Role>> GetAllRolesAsync();
    Task<Role?> GetRoleAsync(Guid roleId);
    Task<Role?> GetRoleByNameAsync(string roleName);
    Task<Role> CreateRoleAsync(Role role);
    Task<Role> UpdateRoleAsync(Guid roleId, Role role);
    Task DeleteRoleAsync(Guid roleId);
    Task AssignRoleToUserAsync(Guid userId, Guid roleId);
    Task RevokeRoleFromUserAsync(Guid userId, Guid roleId);
    Task<IEnumerable<Role>> GetUserRolesAsync(Guid userId);
    Task<IEnumerable<string>> GetUserPermissionsAsync(Guid userId);
    Task<bool> UserHasPermissionAsync(Guid userId, string permission);
}
