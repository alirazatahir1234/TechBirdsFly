namespace GeneratorService.Application.DTOs;

/// <summary>
/// Data Transfer Object for Generated Page
/// </summary>
public class GeneratedPageDto
{
    /// <summary>
    /// Unique identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Page title
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// Complete HTML content
    /// </summary>
    public required string Html { get; set; }

    /// <summary>
    /// CSS styles
    /// </summary>
    public required string Css { get; set; }

    /// <summary>
    /// JavaScript code
    /// </summary>
    public required string JavaScript { get; set; }

    /// <summary>
    /// Page version number
    /// </summary>
    public int Version { get; set; }

    /// <summary>
    /// Whether page is published
    /// </summary>
    public bool IsPublished { get; set; }

    /// <summary>
    /// SEO page title
    /// </summary>
    public required string MetaTitle { get; set; }

    /// <summary>
    /// SEO page description
    /// </summary>
    public required string MetaDescription { get; set; }

    /// <summary>
    /// SEO keywords
    /// </summary>
    public required string MetaKeywords { get; set; }

    /// <summary>
    /// Creation timestamp (UTC)
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Last update timestamp (UTC)
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
