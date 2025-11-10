namespace AdminService.Application.Interfaces;

using AdminService.Domain.Entities;

/// <summary>
/// Repository for AdminUser entities
/// </summary>
public interface IAdminUserRepository
{
    Task<AdminUser?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AdminUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<IEnumerable<AdminUser>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<AdminUser>> GetByStatusAsync(string status, CancellationToken cancellationToken = default);
    Task AddAsync(AdminUser adminUser, CancellationToken cancellationToken = default);
    Task UpdateAsync(AdminUser adminUser, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository for Role entities
/// </summary>
public interface IRoleRepository
{
    Task<Role?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<IEnumerable<Role>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Role role, CancellationToken cancellationToken = default);
    Task UpdateAsync(Role role, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository for AuditLog entities
/// </summary>
public interface IAuditLogRepository
{
    Task<AuditLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<AuditLog>> GetByAdminUserIdAsync(Guid adminUserId, CancellationToken cancellationToken = default);
    Task<IEnumerable<AuditLog>> GetByResourceAsync(string resourceType, string resourceId, CancellationToken cancellationToken = default);
    Task<IEnumerable<AuditLog>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<(IEnumerable<AuditLog> Items, int TotalCount)> GetAllAsync(AuditLogFilterRequest filter, CancellationToken cancellationToken = default);
    Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Application service for AdminUser business logic
/// </summary>
public interface IAdminUserApplicationService
{
    Task<AdminUser> CreateAdminUserAsync(string email, string fullName, CancellationToken cancellationToken = default);
    Task<AdminUser?> GetAdminUserAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<AdminUser>> GetAllAdminUsersAsync(CancellationToken cancellationToken = default);
    Task UpdateAdminUserAsync(Guid id, string? fullName, int? projectCount, decimal? totalSpent, CancellationToken cancellationToken = default);
    Task SuspendAdminUserAsync(Guid id, string reason, CancellationToken cancellationToken = default);
    Task UnsuspendAdminUserAsync(Guid id, CancellationToken cancellationToken = default);
    Task BanAdminUserAsync(Guid id, CancellationToken cancellationToken = default);
    Task RecordLoginAsync(Guid id, CancellationToken cancellationToken = default);
}

/// <summary>
/// Application service for Role business logic
/// </summary>
public interface IRoleApplicationService
{
    Task<Role> CreateRoleAsync(string name, string description, List<string> permissions, CancellationToken cancellationToken = default);
    Task<Role?> GetRoleAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Role>> GetAllRolesAsync(CancellationToken cancellationToken = default);
    Task UpdateRoleAsync(Guid id, string description, List<string> permissions, CancellationToken cancellationToken = default);
    Task DeleteRoleAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddPermissionToRoleAsync(Guid roleId, string permission, CancellationToken cancellationToken = default);
    Task RemovePermissionFromRoleAsync(Guid roleId, string permission, CancellationToken cancellationToken = default);
}

/// <summary>
/// Application service for AuditLog operations
/// </summary>
public interface IAuditLogApplicationService
{
    Task<AuditLog> LogActionAsync(
        Guid? adminUserId,
        string action,
        string resourceType,
        string resourceId,
        string ipAddress,
        string? userAgent = null,
        string? details = null,
        string? oldValues = null,
        string? newValues = null,
        CancellationToken cancellationToken = default);
    
    Task<AuditLog?> GetAuditLogAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<(IEnumerable<AuditLogDto> Items, int TotalCount)> GetAuditLogsAsync(
        AuditLogFilterRequest filter,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Service for event publishing
/// </summary>
public interface IEventPublisher
{
    Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : class;
}
