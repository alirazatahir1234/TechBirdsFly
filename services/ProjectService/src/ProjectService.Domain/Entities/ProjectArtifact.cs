namespace ProjectService.Domain.Entities;

/// <summary>
/// ProjectArtifact entity - links generated artifacts from GeneratorService to project versions
/// </summary>
public class ProjectArtifact
{
    /// <summary>
    /// Unique artifact link identifier
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Associated version ID
    /// </summary>
    public Guid VersionId { get; set; }

    /// <summary>
    /// Artifact ID from GeneratorService
    /// </summary>
    public Guid ArtifactId { get; set; }

    /// <summary>
    /// Artifact type: page | component | template
    /// </summary>
    public string Type { get; set; } = default!;

    /// <summary>
    /// When artifact was linked to this version
    /// </summary>
    public DateTime LinkedAt { get; set; } = DateTime.UtcNow;
}
