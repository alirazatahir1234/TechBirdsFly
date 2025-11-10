namespace AdminService.Application.Services;

using AdminService.Application.Interfaces;
using AdminService.Domain.Entities;
using Microsoft.Extensions.Logging;

/// <summary>
/// Application service for AuditLog operations
/// </summary>
public class AuditLogApplicationService : IAuditLogApplicationService
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ILogger<AuditLogApplicationService> _logger;

    public AuditLogApplicationService(
        IAuditLogRepository auditLogRepository,
        ILogger<AuditLogApplicationService> logger)
    {
        _auditLogRepository = auditLogRepository;
        _logger = logger;
    }

    public async Task<AuditLog> LogActionAsync(
        Guid? adminUserId,
        string action,
        string resourceType,
        string resourceId,
        string ipAddress,
        string? userAgent = null,
        string? details = null,
        string? oldValues = null,
        string? newValues = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Logging action: {Action} on {ResourceType}:{ResourceId} by user {UserId}",
            action, resourceType, resourceId, adminUserId ?? Guid.Empty);

        try
        {
            var auditLog = AuditLog.Create(
                adminUserId,
                action,
                resourceType,
                resourceId,
                ipAddress,
                userAgent,
                details,
                oldValues,
                newValues);

            await _auditLogRepository.AddAsync(auditLog, cancellationToken);
            await _auditLogRepository.SaveChangesAsync(cancellationToken);

            _logger.LogDebug("✅ Audit log created: {AuditLogId}", auditLog.Id);
            return auditLog;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating audit log for action {Action}", action);
            throw;
        }
    }

    public async Task<AuditLog?> GetAuditLogAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Fetching audit log: {AuditLogId}", id);

        try
        {
            var auditLog = await _auditLogRepository.GetByIdAsync(id, cancellationToken);
            return auditLog;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching audit log {AuditLogId}", id);
            throw;
        }
    }

    public async Task<(IEnumerable<AuditLogDto> Items, int TotalCount)> GetAuditLogsAsync(
        AuditLogFilterRequest filter,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug(
            "Fetching audit logs - UserId: {UserId}, Action: {Action}, ResourceType: {ResourceType}, Page: {Page}/{PageSize}",
            filter.AdminUserId, filter.Action, filter.ResourceType, filter.PageNumber, filter.PageSize);

        try
        {
            var (auditLogs, totalCount) = await _auditLogRepository.GetAllAsync(filter, cancellationToken);

            var dtos = auditLogs.Select(a => new AuditLogDto
            {
                Id = a.Id,
                AdminUserId = a.AdminUserId,
                Action = a.Action,
                ResourceType = a.ResourceType,
                ResourceId = a.ResourceId,
                Details = a.Details,
                OldValues = a.OldValues,
                NewValues = a.NewValues,
                IpAddress = a.IpAddress,
                UserAgent = a.UserAgent,
                CreatedAt = a.CreatedAt
            }).ToList();

            return (dtos, totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching audit logs");
            throw;
        }
    }
}
