using AdminService.Models;

namespace AdminService.Services;

public interface IUserManagementService
{
    Task<IEnumerable<AdminUser>> GetAllUsersAsync();
    Task<AdminUser?> GetUserAsync(Guid userId);
    Task<AdminUser?> GetUserByEmailAsync(string email);
    Task<AdminUser> CreateUserAsync(AdminUser user);
    Task<AdminUser> UpdateUserAsync(Guid userId, AdminUser user);
    Task SuspendUserAsync(Guid userId, string reason);
    Task UnsuspendUserAsync(Guid userId);
    Task BanUserAsync(Guid userId, string reason);
    Task DeleteUserAsync(Guid userId);
    Task<int> GetTotalUsersCountAsync();
    Task<int> GetActiveUsersCountAsync();
    Task<int> GetNewUsersCountAsync(DateTime since);
}
