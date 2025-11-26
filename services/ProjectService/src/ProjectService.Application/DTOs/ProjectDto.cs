namespace ProjectService.Application.DTOs;

/// <summary>
/// Project data transfer object
/// </summary>
public class ProjectDto
{
    /// <summary>
    /// Project unique identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Project name
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// Project description
    /// </summary>
    public string Description { get; set; } = "";

    /// <summary>
    /// Target framework
    /// </summary>
    public string Framework { get; set; } = "";

    /// <summary>
    /// Theme style
    /// </summary>
    public string Theme { get; set; } = "";

    /// <summary>
    /// Project creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Current version count
    /// </summary>
    public int VersionCount { get; set; }
}
