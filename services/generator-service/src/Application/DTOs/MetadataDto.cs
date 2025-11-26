namespace GeneratorService.Application.DTOs;

/// <summary>
/// Data Transfer Object for website metadata and SEO information
/// </summary>
public class MetadataDto
{
    /// <summary>
    /// Page title for SEO and browser tab
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// Page description for search engine snippets
    /// </summary>
    public required string Description { get; set; }

    /// <summary>
    /// Keywords for SEO optimization
    /// </summary>
    public string? Keywords { get; set; }

    /// <summary>
    /// Open Graph image URL for social sharing
    /// </summary>
    public string? OgImage { get; set; }

    /// <summary>
    /// Open Graph title for social sharing
    /// </summary>
    public string? OgTitle { get; set; }

    /// <summary>
    /// Canonical URL to prevent duplicate content issues
    /// </summary>
    public string? CanonicalUrl { get; set; }
}
