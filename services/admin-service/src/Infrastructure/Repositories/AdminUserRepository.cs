using Microsoft.EntityFrameworkCore;
using TechBirdsFly.AdminService.Application.Interfaces;
using TechBirdsFly.AdminService.Domain.Entities;

namespace TechBirdsFly.AdminService.Infrastructure.Repositories;

/// <summary>
/// Repository for AdminUser entity persistence and retrieval operations.
/// Implements the IAdminUserRepository interface for data access abstraction.
/// </summary>
public class AdminUserRepository : IAdminUserRepository
{
    private readonly AdminDbContext _context;

    public AdminUserRepository(AdminDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<AdminUser?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
            return null;

        return await _context.AdminUsers
            .Include(a => a.Roles)
            .Include(a => a.AuditLogs)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<AdminUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;

        return await _context.AdminUsers
            .Include(a => a.Roles)
            .Include(a => a.AuditLogs)
            .FirstOrDefaultAsync(a => a.Email == email.ToLower(), cancellationToken);
    }

    public async Task<IEnumerable<AdminUser>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.AdminUsers
            .Include(a => a.Roles)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<AdminUser>> GetByStatusAsync(string status, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(status))
            return Enumerable.Empty<AdminUser>();

        return await _context.AdminUsers
            .Where(a => a.Status == status)
            .Include(a => a.Roles)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<AdminUser> AddAsync(AdminUser adminUser, CancellationToken cancellationToken = default)
    {
        if (adminUser == null)
            throw new ArgumentNullException(nameof(adminUser));

        // Normalize email for consistency
        adminUser = new AdminUser
        {
            Id = adminUser.Id,
            Email = adminUser.Email.ToLower(),
            FullName = adminUser.FullName,
            Status = adminUser.Status,
            LastLoginAt = adminUser.LastLoginAt,
            ProjectCount = adminUser.ProjectCount,
            TotalSpent = adminUser.TotalSpent,
            SuspensionReason = adminUser.SuspensionReason,
            SuspendedAt = adminUser.SuspendedAt,
            CreatedAt = adminUser.CreatedAt,
            UpdatedAt = adminUser.UpdatedAt
        };

        _context.AdminUsers.Add(adminUser);
        return adminUser;
    }

    public async Task<AdminUser> UpdateAsync(AdminUser adminUser, CancellationToken cancellationToken = default)
    {
        if (adminUser == null)
            throw new ArgumentNullException(nameof(adminUser));

        var existingUser = await _context.AdminUsers.FindAsync(new object[] { adminUser.Id }, cancellationToken);
        if (existingUser == null)
            throw new InvalidOperationException($"AdminUser with ID {adminUser.Id} not found");

        _context.AdminUsers.Update(adminUser);
        return adminUser;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
            return;

        var adminUser = await _context.AdminUsers.FindAsync(new object[] { id }, cancellationToken);
        if (adminUser != null)
        {
            _context.AdminUsers.Remove(adminUser);
        }
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
