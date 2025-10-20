using AdminService.Models;

namespace AdminService.Services;

public interface IAdminService
{
    // Audit Logs
    Task<IEnumerable<AuditLog>> GetAuditLogsAsync(int limit = 100);
    Task<IEnumerable<AuditLog>> GetUserAuditLogsAsync(Guid userId, int limit = 50);
    Task LogActionAsync(Guid? userId, string action, string resourceType, string resourceId, string? details = null);

    // Templates
    Task<IEnumerable<Template>> GetActiveTemplatesAsync();
    Task<IEnumerable<Template>> GetTemplatesByCategoryAsync(string category);
    Task<Template?> GetTemplateAsync(Guid id);
    Task<Template> CreateTemplateAsync(Template template);
    Task<Template> UpdateTemplateAsync(Guid id, Template template);
    Task DeleteTemplateAsync(Guid id);

    // System Settings
    Task<string?> GetSettingAsync(string key);
    Task<T?> GetSettingAsync<T>(string key);
    Task SetSettingAsync(string key, string value);
    Task DeleteSettingAsync(string key);
}
