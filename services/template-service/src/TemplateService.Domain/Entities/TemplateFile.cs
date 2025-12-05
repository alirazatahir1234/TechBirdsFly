namespace TemplateService.Domain.Entities;

/// <summary>
/// TemplateFile entity representing individual files within a template
/// </summary>
public class TemplateFile
{
    /// <summary>
    /// Unique identifier for the file
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Reference to the parent Template
    /// </summary>
    public Guid TemplateId { get; set; }

    /// <summary>
    /// MinIO file path (e.g., templates/{id}/index.html)
    /// </summary>
    public string Path { get; set; } = default!;

    /// <summary>
    /// File format (html, react, next, json)
    /// </summary>
    public string Format { get; set; } = default!;

    /// <summary>
    /// File creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
