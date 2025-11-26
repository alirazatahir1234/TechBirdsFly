namespace GeneratorService.Application.DTOs;

/// <summary>
/// Data Transfer Object for Section
/// </summary>
public class SectionDto
{
    /// <summary>
    /// Unique identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Project ID this section belongs to
    /// </summary>
    public Guid ProjectId { get; set; }

    /// <summary>
    /// Section type (Hero, Features, About, Pricing, Contact, etc.)
    /// </summary>
    public required string Type { get; set; }

    /// <summary>
    /// HTML content of the section
    /// </summary>
    public required string HtmlContent { get; set; }

    /// <summary>
    /// CSS class for styling
    /// </summary>
    public required string CssClass { get; set; }

    /// <summary>
    /// Creation timestamp (UTC)
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Last update timestamp (UTC)
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
