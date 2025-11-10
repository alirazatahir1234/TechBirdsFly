namespace AdminService.Application.Services;

using AdminService.Application.Interfaces;
using AdminService.Domain.Entities;
using AdminService.Domain.Events;
using Microsoft.Extensions.Logging;

/// <summary>
/// Application service for AdminUser business logic
/// </summary>
public class AdminUserApplicationService : IAdminUserApplicationService
{
    private readonly IAdminUserRepository _adminUserRepository;
    private readonly IAuditLogApplicationService _auditLogService;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<AdminUserApplicationService> _logger;

    public AdminUserApplicationService(
        IAdminUserRepository adminUserRepository,
        IAuditLogApplicationService auditLogService,
        IEventPublisher eventPublisher,
        ILogger<AdminUserApplicationService> logger)
    {
        _adminUserRepository = adminUserRepository;
        _auditLogService = auditLogService;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task<AdminUser> CreateAdminUserAsync(string email, string fullName, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating admin user with email: {Email}", email);

        // Check if user already exists
        var existingUser = await _adminUserRepository.GetByEmailAsync(email, cancellationToken);
        if (existingUser != null)
            throw new InvalidOperationException($"Admin user with email {email} already exists");

        // Create the user
        var adminUser = AdminUser.Create(email, fullName);
        await _adminUserRepository.AddAsync(adminUser, cancellationToken);
        await _adminUserRepository.SaveChangesAsync(cancellationToken);

        // Publish event
        var @event = new AdminUserCreatedEvent
        {
            AdminUserId = adminUser.Id,
            Email = adminUser.Email,
            FullName = adminUser.FullName,
            CreatedAt = adminUser.CreatedAt
        };
        
        await _eventPublisher.PublishAsync(@event, cancellationToken);

        _logger.LogInformation("✅ Admin user created: {AdminUserId}", adminUser.Id);
        return adminUser;
    }

    public async Task<AdminUser?> GetAdminUserAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Fetching admin user: {AdminUserId}", id);
        return await _adminUserRepository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<IEnumerable<AdminUser>> GetAllAdminUsersAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Fetching all admin users");
        return await _adminUserRepository.GetAllAsync(cancellationToken);
    }

    public async Task UpdateAdminUserAsync(Guid id, string? fullName, int? projectCount, decimal? totalSpent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating admin user: {AdminUserId}", id);

        var adminUser = await _adminUserRepository.GetByIdAsync(id, cancellationToken);
        if (adminUser == null)
            throw new InvalidOperationException($"Admin user with ID {id} not found");

        if (!string.IsNullOrEmpty(fullName))
            adminUser = new AdminUser { /* ... */ }; // Reflection or factory method

        if (projectCount.HasValue)
            adminUser.UpdateProjectCount(projectCount.Value);

        if (totalSpent.HasValue)
            adminUser.UpdateTotalSpent(totalSpent.Value);

        await _adminUserRepository.UpdateAsync(adminUser, cancellationToken);
        await _adminUserRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("✅ Admin user updated: {AdminUserId}", id);
    }

    public async Task SuspendAdminUserAsync(Guid id, string reason, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Suspending admin user: {AdminUserId}", id);

        var adminUser = await _adminUserRepository.GetByIdAsync(id, cancellationToken);
        if (adminUser == null)
            throw new InvalidOperationException($"Admin user with ID {id} not found");

        adminUser.Suspend(reason);
        await _adminUserRepository.UpdateAsync(adminUser, cancellationToken);
        await _adminUserRepository.SaveChangesAsync(cancellationToken);

        // Publish event
        var @event = new AdminUserSuspendedEvent
        {
            AdminUserId = adminUser.Id,
            Email = adminUser.Email,
            Reason = reason,
            SuspendedAt = adminUser.SuspendedAt ?? DateTime.UtcNow
        };
        
        await _eventPublisher.PublishAsync(@event, cancellationToken);

        _logger.LogInformation("✅ Admin user suspended: {AdminUserId}", id);
    }

    public async Task UnsuspendAdminUserAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Unsuspending admin user: {AdminUserId}", id);

        var adminUser = await _adminUserRepository.GetByIdAsync(id, cancellationToken);
        if (adminUser == null)
            throw new InvalidOperationException($"Admin user with ID {id} not found");

        adminUser.Unsuspend();
        await _adminUserRepository.UpdateAsync(adminUser, cancellationToken);
        await _adminUserRepository.SaveChangesAsync(cancellationToken);

        // Publish event
        var @event = new AdminUserUnsuspendedEvent
        {
            AdminUserId = adminUser.Id,
            Email = adminUser.Email,
            UnsuspendedAt = DateTime.UtcNow
        };
        
        await _eventPublisher.PublishAsync(@event, cancellationToken);

        _logger.LogInformation("✅ Admin user unsuspended: {AdminUserId}", id);
    }

    public async Task BanAdminUserAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Banning admin user: {AdminUserId}", id);

        var adminUser = await _adminUserRepository.GetByIdAsync(id, cancellationToken);
        if (adminUser == null)
            throw new InvalidOperationException($"Admin user with ID {id} not found");

        adminUser.Ban();
        await _adminUserRepository.UpdateAsync(adminUser, cancellationToken);
        await _adminUserRepository.SaveChangesAsync(cancellationToken);

        // Publish event
        var @event = new AdminUserBannedEvent
        {
            AdminUserId = adminUser.Id,
            Email = adminUser.Email,
            BannedAt = DateTime.UtcNow
        };
        
        await _eventPublisher.PublishAsync(@event, cancellationToken);

        _logger.LogInformation("✅ Admin user banned: {AdminUserId}", id);
    }

    public async Task RecordLoginAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Recording login for admin user: {AdminUserId}", id);

        var adminUser = await _adminUserRepository.GetByIdAsync(id, cancellationToken);
        if (adminUser == null)
            throw new InvalidOperationException($"Admin user with ID {id} not found");

        adminUser.RecordLogin();
        await _adminUserRepository.UpdateAsync(adminUser, cancellationToken);
        await _adminUserRepository.SaveChangesAsync(cancellationToken);

        // Publish event
        var @event = new AdminUserLoginEvent
        {
            AdminUserId = adminUser.Id,
            Email = adminUser.Email,
            IpAddress = "0.0.0.0", // Will be captured from HttpContext in controller
            LoginAt = adminUser.LastLoginAt ?? DateTime.UtcNow
        };
        
        await _eventPublisher.PublishAsync(@event, cancellationToken);
    }
}
