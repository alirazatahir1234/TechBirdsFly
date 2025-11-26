namespace ProjectService.Domain.Entities;

/// <summary>
/// Project entity - represents a user's website/app project
/// </summary>
public class Project
{
    /// <summary>
    /// Unique project identifier
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// User who owns this project
    /// </summary>
    public Guid OwnerId { get; set; }

    /// <summary>
    /// Project name/title
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// Project description
    /// </summary>
    public string Description { get; set; } = "";

    /// <summary>
    /// Target framework: nextjs | react | html
    /// </summary>
    public string Framework { get; set; } = "nextjs";

    /// <summary>
    /// Theme/style: default | dark | minimal | premium
    /// </summary>
    public string Theme { get; set; } = "default";

    /// <summary>
    /// Project creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Collection of project versions (versions when artifacts are linked)
    /// </summary>
    public List<ProjectVersion> Versions { get; set; } = new();
}
