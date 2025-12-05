namespace TemplateService.Domain.Entities;

/// <summary>
/// Template entity representing a reusable website/component template
/// </summary>
public class Template
{
    /// <summary>
    /// Unique identifier for the template
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Template name/title
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// Template category (landing, starter, component, etc.)
    /// </summary>
    public string Category { get; set; } = default!;

    /// <summary>
    /// Template description
    /// </summary>
    public string Description { get; set; } = "";

    /// <summary>
    /// MinIO URL to preview image
    /// </summary>
    public string PreviewImageUrl { get; set; } = "";

    /// <summary>
    /// Template creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Collection of template files (HTML, React, Next.js, JSON)
    /// </summary>
    public List<TemplateFile> Files { get; set; } = new();
}
