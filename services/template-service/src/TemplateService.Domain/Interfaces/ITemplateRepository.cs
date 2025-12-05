using TemplateService.Domain.Entities;

namespace TemplateService.Domain.Interfaces;

/// <summary>
/// Repository interface for template persistence
/// </summary>
public interface ITemplateRepository
{
    /// <summary>
    /// Creates a new template
    /// </summary>
    Task CreateTemplateAsync(Template template);

    /// <summary>
    /// Updates template preview URL
    /// </summary>
    Task UpdatePreviewUrlAsync(Guid templateId, string url);

    /// <summary>
    /// Adds a file to a template
    /// </summary>
    Task AddFileAsync(TemplateFile file);

    /// <summary>
    /// Retrieves templates with optional filtering
    /// </summary>
    Task<List<Template>> GetTemplatesAsync(string? category = null, string? search = null);

    /// <summary>
    /// Retrieves a template by ID
    /// </summary>
    Task<Template?> GetTemplateByIdAsync(Guid id);
}
