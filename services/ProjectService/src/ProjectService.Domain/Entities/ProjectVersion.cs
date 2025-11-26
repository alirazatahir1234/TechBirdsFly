namespace ProjectService.Domain.Entities;

/// <summary>
/// ProjectVersion entity - represents a snapshot of a project at a point in time
/// Versions are created when artifacts are generated/linked
/// </summary>
public class ProjectVersion
{
    /// <summary>
    /// Unique version identifier
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Associated project ID
    /// </summary>
    public Guid ProjectId { get; set; }

    /// <summary>
    /// Sequential version number (1, 2, 3, ...)
    /// </summary>
    public int VersionNumber { get; set; }

    /// <summary>
    /// Version creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Collection of artifacts linked to this version
    /// </summary>
    public List<ProjectArtifact> Artifacts { get; set; } = new();
}
