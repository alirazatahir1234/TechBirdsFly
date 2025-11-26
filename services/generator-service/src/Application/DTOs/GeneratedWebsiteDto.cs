namespace GeneratorService.Application.DTOs;

/// <summary>
/// Data Transfer Object for a complete generated website
/// Represents the final output from AI website generation
/// </summary>
public class GeneratedWebsiteDto
{
    /// <summary>
    /// Website project name
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Target industry (Technology, E-Commerce, Portfolio, etc.)
    /// </summary>
    public required string Industry { get; set; }

    /// <summary>
    /// Design style applied (Modern, Minimal, Bold, etc.)
    /// </summary>
    public required string Style { get; set; }

    /// <summary>
    /// Primary brand color in hex format
    /// </summary>
    public required string PrimaryColor { get; set; }

    /// <summary>
    /// Secondary brand color in hex format
    /// </summary>
    public required string SecondaryColor { get; set; }

    /// <summary>
    /// Website sections (Hero, Features, About, etc.)
    /// </summary>
    public List<SectionDto> Sections { get; set; } = new();

    /// <summary>
    /// Website metadata and SEO information
    /// </summary>
    public MetadataDto? Metadata { get; set; }

    /// <summary>
    /// Complete HTML output if page is finalized
    /// </summary>
    public string? FinalHtml { get; set; }

    /// <summary>
    /// Complete CSS for styling
    /// </summary>
    public string? Css { get; set; }

    /// <summary>
    /// JavaScript code for interactivity
    /// </summary>
    public string? JavaScript { get; set; }
}
