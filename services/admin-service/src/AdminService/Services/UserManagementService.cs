using Microsoft.EntityFrameworkCore;
using AdminService.Data;
using AdminService.Models;

namespace AdminService.Services;

public class UserManagementService : IUserManagementService
{
    private readonly AdminDbContext _db;
    private readonly ILogger<UserManagementService> _logger;
    private readonly IAdminService _adminService;

    public UserManagementService(AdminDbContext db, ILogger<UserManagementService> logger, IAdminService adminService)
    {
        _db = db;
        _logger = logger;
        _adminService = adminService;
    }

    public async Task<IEnumerable<AdminUser>> GetAllUsersAsync()
    {
        return await _db.AdminUsers
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync();
    }

    public async Task<AdminUser?> GetUserAsync(Guid userId)
    {
        return await _db.AdminUsers.FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<AdminUser?> GetUserByEmailAsync(string email)
    {
        return await _db.AdminUsers.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<AdminUser> CreateUserAsync(AdminUser user)
    {
        user.Id = Guid.NewGuid();
        user.CreatedAt = DateTime.UtcNow;
        
        _db.AdminUsers.Add(user);
        await _db.SaveChangesAsync();
        
        await _adminService.LogActionAsync(null, "CREATE", "User", user.Id.ToString(), $"Created user: {user.Email}");
        _logger.LogInformation("Created user: {Email}", user.Email);
        
        return user;
    }

    public async Task<AdminUser> UpdateUserAsync(Guid userId, AdminUser user)
    {
        var existing = await GetUserAsync(userId);
        if (existing == null)
            throw new KeyNotFoundException($"User {userId} not found");

        existing.FullName = user.FullName;
        existing.Email = user.Email;
        existing.ProjectCount = user.ProjectCount;
        existing.TotalSpent = user.TotalSpent;

        await _db.SaveChangesAsync();
        await _adminService.LogActionAsync(null, "UPDATE", "User", userId.ToString(), $"Updated user: {user.Email}");
        
        return existing;
    }

    public async Task SuspendUserAsync(Guid userId, string reason)
    {
        var user = await GetUserAsync(userId);
        if (user == null)
            throw new KeyNotFoundException($"User {userId} not found");

        user.Status = "suspended";
        user.SuspendedAt = DateTime.UtcNow;
        user.SuspensionReason = reason;

        await _db.SaveChangesAsync();
        await _adminService.LogActionAsync(null, "SUSPEND", "User", userId.ToString(), $"Suspended user for: {reason}");
        _logger.LogWarning("Suspended user: {UserId}, Reason: {Reason}", userId, reason);
    }

    public async Task UnsuspendUserAsync(Guid userId)
    {
        var user = await GetUserAsync(userId);
        if (user == null)
            throw new KeyNotFoundException($"User {userId} not found");

        user.Status = "active";
        user.SuspendedAt = null;
        user.SuspensionReason = null;

        await _db.SaveChangesAsync();
        await _adminService.LogActionAsync(null, "UNSUSPEND", "User", userId.ToString(), "User reinstated");
        _logger.LogInformation("Unsuspended user: {UserId}", userId);
    }

    public async Task BanUserAsync(Guid userId, string reason)
    {
        var user = await GetUserAsync(userId);
        if (user == null)
            throw new KeyNotFoundException($"User {userId} not found");

        user.Status = "banned";
        user.SuspendedAt = DateTime.UtcNow;
        user.SuspensionReason = reason;

        await _db.SaveChangesAsync();
        await _adminService.LogActionAsync(null, "BAN", "User", userId.ToString(), $"Banned user for: {reason}");
        _logger.LogWarning("Banned user: {UserId}, Reason: {Reason}", userId, reason);
    }

    public async Task DeleteUserAsync(Guid userId)
    {
        var user = await GetUserAsync(userId);
        if (user == null)
            throw new KeyNotFoundException($"User {userId} not found");

        _db.AdminUsers.Remove(user);
        await _db.SaveChangesAsync();
        
        await _adminService.LogActionAsync(null, "DELETE", "User", userId.ToString(), $"Deleted user: {user.Email}");
        _logger.LogInformation("Deleted user: {UserId}", userId);
    }

    public async Task<int> GetTotalUsersCountAsync()
    {
        return await _db.AdminUsers.CountAsync();
    }

    public async Task<int> GetActiveUsersCountAsync()
    {
        return await _db.AdminUsers.CountAsync(u => u.Status == "active");
    }

    public async Task<int> GetNewUsersCountAsync(DateTime since)
    {
        return await _db.AdminUsers.CountAsync(u => u.CreatedAt >= since);
    }
}
