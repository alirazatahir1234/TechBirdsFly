using MediatR;
using ProjectService.Application.DTOs;

namespace ProjectService.Application.Commands;

/// <summary>
/// Command to create a new project
/// </summary>
public record CreateProjectCommand(CreateProjectRequest Request) : IRequest<CreateProjectResponse>;

/// <summary>
/// Command to create a new version for a project
/// </summary>
public record CreateVersionCommand(Guid ProjectId) : IRequest<ProjectVersionDto>;

/// <summary>
/// Command to link an artifact from GeneratorService to a project version
/// </summary>
public record LinkArtifactCommand(Guid VersionId, Guid ArtifactId, string Type) : IRequest<bool>;

/// <summary>
/// Command to rename a project
/// </summary>
public record RenameProjectCommand(Guid ProjectId, string NewName) : IRequest<bool>;

/// <summary>
/// Command to delete a project
/// </summary>
public record DeleteProjectCommand(Guid ProjectId) : IRequest<bool>;

/// <summary>
/// Command to update project settings
/// </summary>
public record UpdateProjectSettingsCommand(Guid ProjectId, UpdateProjectSettingsRequest Settings) : IRequest<ProjectDto>;

/// <summary>
/// Command to get a specific project
/// </summary>
public record GetProjectQuery(Guid ProjectId) : IRequest<ProjectDto?>;

/// <summary>
/// Command to list all projects for a user
/// </summary>
public record GetUserProjectsQuery(Guid OwnerId) : IRequest<List<ProjectDto>>;

/// <summary>
/// Command to get all versions of a project
/// </summary>
public record GetProjectVersionsQuery(Guid ProjectId) : IRequest<List<ProjectVersionDto>>;
