namespace TemplateService.Application.DTOs;

/// <summary>
/// DTO for creating a new template
/// </summary>
public class CreateTemplateRequest
{
    /// <summary>
    /// Template name
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// Template category (landing, starter, component)
    /// </summary>
    public string Category { get; set; } = default!;

    /// <summary>
    /// Template description
    /// </summary>
    public string Description { get; set; } = "";
}
