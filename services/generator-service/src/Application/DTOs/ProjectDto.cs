namespace GeneratorService.Application.DTOs;

/// <summary>
/// Data Transfer Object for Project
/// </summary>
public class ProjectDto
{
    /// <summary>
    /// Unique identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Project name
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Website industry (Technology, E-Commerce, Portfolio, etc.)
    /// </summary>
    public required string Industry { get; set; }

    /// <summary>
    /// Website style (Modern, Minimal, Bold, etc.)
    /// </summary>
    public required string Style { get; set; }

    /// <summary>
    /// Project description
    /// </summary>
    public required string Description { get; set; }

    /// <summary>
    /// Primary color in hex format
    /// </summary>
    public required string PrimaryColor { get; set; }

    /// <summary>
    /// Secondary color in hex format
    /// </summary>
    public required string SecondaryColor { get; set; }

    /// <summary>
    /// Accent color in hex format
    /// </summary>
    public required string AccentColor { get; set; }

    /// <summary>
    /// Number of sections in project
    /// </summary>
    public int SectionCount { get; set; }

    /// <summary>
    /// Creation timestamp (UTC)
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Last update timestamp (UTC)
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Associated sections
    /// </summary>
    public List<SectionDto> Sections { get; set; } = new();
}
