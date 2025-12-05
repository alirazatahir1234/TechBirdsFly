namespace TemplateService.Application.DTOs;

/// <summary>
/// DTO for template response
/// </summary>
public class TemplateDto
{
    /// <summary>
    /// Template ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Template name
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// Template category
    /// </summary>
    public string Category { get; set; } = default!;

    /// <summary>
    /// Template description
    /// </summary>
    public string Description { get; set; } = "";

    /// <summary>
    /// Preview image URL from MinIO
    /// </summary>
    public string PreviewImageUrl { get; set; } = "";

    /// <summary>
    /// Creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
