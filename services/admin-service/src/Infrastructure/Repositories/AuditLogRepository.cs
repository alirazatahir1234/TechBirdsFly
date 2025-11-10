using Microsoft.EntityFrameworkCore;
using TechBirdsFly.AdminService.Application.DTOs;
using TechBirdsFly.AdminService.Application.Interfaces;
using TechBirdsFly.AdminService.Domain.Entities;

namespace TechBirdsFly.AdminService.Infrastructure.Repositories;

/// <summary>
/// Repository for AuditLog entity persistence and retrieval operations.
/// Implements the IAuditLogRepository interface for data access abstraction.
/// Supports complex filtering and pagination for audit trail queries.
/// </summary>
public class AuditLogRepository : IAuditLogRepository
{
    private readonly AdminDbContext _context;

    public AuditLogRepository(AdminDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<AuditLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
            return null;

        return await _context.AuditLogs
            .Include(a => a.AdminUser)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<AuditLog>> GetByAdminUserIdAsync(Guid adminUserId, CancellationToken cancellationToken = default)
    {
        if (adminUserId == Guid.Empty)
            return Enumerable.Empty<AuditLog>();

        return await _context.AuditLogs
            .Where(a => a.AdminUserId == adminUserId)
            .Include(a => a.AdminUser)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<AuditLog>> GetByResourceAsync(string resourceType, string resourceId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(resourceType) || string.IsNullOrWhiteSpace(resourceId))
            return Enumerable.Empty<AuditLog>();

        return await _context.AuditLogs
            .Where(a => a.ResourceType == resourceType && a.ResourceId == resourceId)
            .Include(a => a.AdminUser)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<AuditLog>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.AuditLogs
            .Include(a => a.AdminUser)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IEnumerable<AuditLog> Items, int TotalCount)> GetAllAsync(
        AuditLogFilterRequest filter,
        CancellationToken cancellationToken = default)
    {
        if (filter == null)
            throw new ArgumentNullException(nameof(filter));

        var query = _context.AuditLogs
            .Include(a => a.AdminUser)
            .AsQueryable();

        // Apply optional filters
        if (filter.AdminUserId.HasValue && filter.AdminUserId != Guid.Empty)
        {
            query = query.Where(a => a.AdminUserId == filter.AdminUserId);
        }

        if (!string.IsNullOrWhiteSpace(filter.Action))
        {
            query = query.Where(a => a.Action == filter.Action);
        }

        if (!string.IsNullOrWhiteSpace(filter.ResourceType))
        {
            query = query.Where(a => a.ResourceType == filter.ResourceType);
        }

        if (filter.FromDate.HasValue)
        {
            query = query.Where(a => a.CreatedAt >= filter.FromDate);
        }

        if (filter.ToDate.HasValue)
        {
            var toDate = filter.ToDate.Value.AddDays(1); // Include entire day
            query = query.Where(a => a.CreatedAt < toDate);
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination
        var pageNumber = Math.Max(1, filter.PageNumber);
        var pageSize = Math.Max(1, Math.Min(filter.PageSize, 100)); // Cap at 100 items per page
        var skip = (pageNumber - 1) * pageSize;

        var items = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<AuditLog> AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default)
    {
        if (auditLog == null)
            throw new ArgumentNullException(nameof(auditLog));

        _context.AuditLogs.Add(auditLog);
        return auditLog;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
