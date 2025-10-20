using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using AdminService.Data;
using AdminService.Models;

namespace AdminService.Services;

public class AdminService : IAdminService
{
    private readonly AdminDbContext _db;
    private readonly ILogger<AdminService> _logger;

    public AdminService(AdminDbContext db, ILogger<AdminService> logger)
    {
        _db = db;
        _logger = logger;
    }

    // Audit Logs
    public async Task<IEnumerable<AuditLog>> GetAuditLogsAsync(int limit = 100)
    {
        return await _db.AuditLogs
            .OrderByDescending(a => a.CreatedAt)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetUserAuditLogsAsync(Guid userId, int limit = 50)
    {
        return await _db.AuditLogs
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .Take(limit)
            .ToListAsync();
    }

    public async Task LogActionAsync(Guid? userId, string action, string resourceType, string resourceId, string? details = null)
    {
        var log = new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Action = action,
            ResourceType = resourceType,
            ResourceId = resourceId,
            Details = details,
            IpAddress = "127.0.0.1"
        };

        _db.AuditLogs.Add(log);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Logged action: {Action} on {ResourceType}", action, resourceType);
    }

    // Templates
    public async Task<IEnumerable<Template>> GetActiveTemplatesAsync()
    {
        return await _db.Templates
            .Where(t => t.IsActive)
            .OrderBy(t => t.Priority)
            .ToListAsync();
    }

    public async Task<IEnumerable<Template>> GetTemplatesByCategoryAsync(string category)
    {
        return await _db.Templates
            .Where(t => t.Category == category && t.IsActive)
            .OrderBy(t => t.Priority)
            .ToListAsync();
    }

    public async Task<Template?> GetTemplateAsync(Guid id)
    {
        return await _db.Templates.FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<Template> CreateTemplateAsync(Template template)
    {
        template.Id = Guid.NewGuid();
        template.CreatedAt = DateTime.UtcNow;
        
        _db.Templates.Add(template);
        await _db.SaveChangesAsync();
        
        await LogActionAsync(null, "CREATE", "Template", template.Id.ToString(), $"Created template: {template.Name}");
        return template;
    }

    public async Task<Template> UpdateTemplateAsync(Guid id, Template template)
    {
        var existing = await GetTemplateAsync(id);
        if (existing == null)
            throw new KeyNotFoundException($"Template {id} not found");

        existing.Name = template.Name;
        existing.Category = template.Category;
        existing.Description = template.Description;
        existing.ThumbnailUrl = template.ThumbnailUrl;
        existing.HtmlTemplate = template.HtmlTemplate;
        existing.CssTemplate = template.CssTemplate;
        existing.IsActive = template.IsActive;
        existing.Priority = template.Priority;
        existing.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        await LogActionAsync(null, "UPDATE", "Template", id.ToString(), $"Updated template: {template.Name}");
        
        return existing;
    }

    public async Task DeleteTemplateAsync(Guid id)
    {
        var template = await GetTemplateAsync(id);
        if (template == null)
            throw new KeyNotFoundException($"Template {id} not found");

        _db.Templates.Remove(template);
        await _db.SaveChangesAsync();
        
        await LogActionAsync(null, "DELETE", "Template", id.ToString(), $"Deleted template: {template.Name}");
    }

    // System Settings
    public async Task<string?> GetSettingAsync(string key)
    {
        var setting = await _db.SystemSettings
            .FirstOrDefaultAsync(s => s.Key == key);
        return setting?.Value;
    }

    public async Task<T?> GetSettingAsync<T>(string key)
    {
        var value = await GetSettingAsync(key);
        if (value == null)
            return default;

        try
        {
            return JsonSerializer.Deserialize<T>(value);
        }
        catch
        {
            return default;
        }
    }

    public async Task SetSettingAsync(string key, string value)
    {
        var setting = await _db.SystemSettings
            .FirstOrDefaultAsync(s => s.Key == key);

        if (setting == null)
        {
            setting = new SystemSetting
            {
                Id = Guid.NewGuid(),
                Key = key,
                Value = value,
                CreatedAt = DateTime.UtcNow
            };
            _db.SystemSettings.Add(setting);
        }
        else
        {
            setting.Value = value;
            setting.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        await LogActionAsync(null, "UPDATE", "SystemSetting", key, $"Updated setting: {key}");
    }

    public async Task DeleteSettingAsync(string key)
    {
        var setting = await _db.SystemSettings
            .FirstOrDefaultAsync(s => s.Key == key);

        if (setting != null)
        {
            _db.SystemSettings.Remove(setting);
            await _db.SaveChangesAsync();
            await LogActionAsync(null, "DELETE", "SystemSetting", key, $"Deleted setting: {key}");
        }
    }
}
