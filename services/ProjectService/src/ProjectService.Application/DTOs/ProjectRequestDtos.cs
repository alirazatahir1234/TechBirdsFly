namespace ProjectService.Application.DTOs;

/// <summary>
/// Request DTO for creating a new project
/// </summary>
public class CreateProjectRequest
{
    /// <summary>
    /// User who owns the project
    /// </summary>
    public Guid OwnerId { get; set; }

    /// <summary>
    /// Project name
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// Target framework (default: nextjs)
    /// </summary>
    public string Framework { get; set; } = "nextjs";

    /// <summary>
    /// Theme style (default: default)
    /// </summary>
    public string Theme { get; set; } = "default";

    /// <summary>
    /// Optional project description
    /// </summary>
    public string Description { get; set; } = "";
}

/// <summary>
/// Response DTO for project creation
/// </summary>
public class CreateProjectResponse
{
    /// <summary>
    /// Newly created project
    /// </summary>
    public ProjectDto Project { get; set; } = default!;

    /// <summary>
    /// Initial version created
    /// </summary>
    public ProjectVersionDto InitialVersion { get; set; } = default!;
}

/// <summary>
/// Request DTO for renaming a project
/// </summary>
public class RenameProjectRequest
{
    /// <summary>
    /// New project name
    /// </summary>
    public string NewName { get; set; } = default!;
}

/// <summary>
/// Request DTO for updating project settings
/// </summary>
public class UpdateProjectSettingsRequest
{
    /// <summary>
    /// Project description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Framework choice
    /// </summary>
    public string? Framework { get; set; }

    /// <summary>
    /// Theme choice
    /// </summary>
    public string? Theme { get; set; }
}

/// <summary>
/// Project version DTO
/// </summary>
public class ProjectVersionDto
{
    /// <summary>
    /// Version identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Sequential version number
    /// </summary>
    public int VersionNumber { get; set; }

    /// <summary>
    /// Creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Linked artifact count
    /// </summary>
    public int ArtifactCount { get; set; }
}
