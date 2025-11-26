using MediatR;
using ProjectService.Application.Commands;
using ProjectService.Application.DTOs;
using ProjectService.Domain.Entities;
using ProjectService.Infrastructure.Data;

namespace ProjectService.Application.Handlers;

/// <summary>
/// Handler for CreateProjectCommand
/// Creates a new project with initial version 1
/// </summary>
public class CreateProjectHandler : IRequestHandler<CreateProjectCommand, CreateProjectResponse>
{
    private readonly ProjectDbContext _db;

    public CreateProjectHandler(ProjectDbContext db)
    {
        _db = db;
    }

    public async Task<CreateProjectResponse> Handle(CreateProjectCommand cmd, CancellationToken ct)
    {
        // Create project entity
        var project = new Project
        {
            OwnerId = cmd.Request.OwnerId,
            Name = cmd.Request.Name,
            Framework = cmd.Request.Framework,
            Theme = cmd.Request.Theme,
            Description = cmd.Request.Description
        };

        await _db.Projects.AddAsync(project, ct);
        await _db.SaveChangesAsync(ct);

        // Create initial version
        var version = new ProjectVersion
        {
            ProjectId = project.Id,
            VersionNumber = 1
        };

        await _db.Versions.AddAsync(version, ct);
        await _db.SaveChangesAsync(ct);

        return new CreateProjectResponse
        {
            Project = new ProjectDto
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                Framework = project.Framework,
                Theme = project.Theme,
                CreatedAt = project.CreatedAt,
                VersionCount = 1
            },
            InitialVersion = new ProjectVersionDto
            {
                Id = version.Id,
                VersionNumber = version.VersionNumber,
                CreatedAt = version.CreatedAt,
                ArtifactCount = 0
            }
        };
    }
}

/// <summary>
/// Handler for CreateVersionCommand
/// Creates a new version with incremented version number
/// </summary>
public class CreateVersionHandler : IRequestHandler<CreateVersionCommand, ProjectVersionDto>
{
    private readonly ProjectDbContext _db;

    public CreateVersionHandler(ProjectDbContext db)
    {
        _db = db;
    }

    public async Task<ProjectVersionDto> Handle(CreateVersionCommand cmd, CancellationToken ct)
    {
        // Get latest version to determine next version number
        var lastVersion = await _db.Versions
            .Where(v => v.ProjectId == cmd.ProjectId)
            .OrderByDescending(v => v.VersionNumber)
            .FirstOrDefaultAsync(ct);

        var nextVersionNumber = lastVersion?.VersionNumber + 1 ?? 1;

        var newVersion = new ProjectVersion
        {
            ProjectId = cmd.ProjectId,
            VersionNumber = nextVersionNumber
        };

        await _db.Versions.AddAsync(newVersion, ct);
        await _db.SaveChangesAsync(ct);

        return new ProjectVersionDto
        {
            Id = newVersion.Id,
            VersionNumber = newVersion.VersionNumber,
            CreatedAt = newVersion.CreatedAt,
            ArtifactCount = 0
        };
    }
}

/// <summary>
/// Handler for LinkArtifactCommand
/// Links a generated artifact from GeneratorService to a project version
/// </summary>
public class LinkArtifactHandler : IRequestHandler<LinkArtifactCommand, bool>
{
    private readonly ProjectDbContext _db;

    public LinkArtifactHandler(ProjectDbContext db)
    {
        _db = db;
    }

    public async Task<bool> Handle(LinkArtifactCommand cmd, CancellationToken ct)
    {
        var artifact = new ProjectArtifact
        {
            VersionId = cmd.VersionId,
            ArtifactId = cmd.ArtifactId,
            Type = cmd.Type
        };

        await _db.Artifacts.AddAsync(artifact, ct);
        await _db.SaveChangesAsync(ct);

        return true;
    }
}

/// <summary>
/// Handler for RenameProjectCommand
/// </summary>
public class RenameProjectHandler : IRequestHandler<RenameProjectCommand, bool>
{
    private readonly ProjectDbContext _db;

    public RenameProjectHandler(ProjectDbContext db)
    {
        _db = db;
    }

    public async Task<bool> Handle(RenameProjectCommand cmd, CancellationToken ct)
    {
        var project = await _db.Projects.FindAsync(new object[] { cmd.ProjectId }, cancellationToken: ct);
        if (project == null) return false;

        project.Name = cmd.NewName;
        await _db.SaveChangesAsync(ct);

        return true;
    }
}

/// <summary>
/// Handler for DeleteProjectCommand
/// </summary>
public class DeleteProjectHandler : IRequestHandler<DeleteProjectCommand, bool>
{
    private readonly ProjectDbContext _db;

    public DeleteProjectHandler(ProjectDbContext db)
    {
        _db = db;
    }

    public async Task<bool> Handle(DeleteProjectCommand cmd, CancellationToken ct)
    {
        var project = await _db.Projects.FindAsync(new object[] { cmd.ProjectId }, cancellationToken: ct);
        if (project == null) return false;

        _db.Projects.Remove(project);
        await _db.SaveChangesAsync(ct);

        return true;
    }
}

/// <summary>
/// Handler for UpdateProjectSettingsCommand
/// </summary>
public class UpdateProjectSettingsHandler : IRequestHandler<UpdateProjectSettingsCommand, ProjectDto>
{
    private readonly ProjectDbContext _db;

    public UpdateProjectSettingsHandler(ProjectDbContext db)
    {
        _db = db;
    }

    public async Task<ProjectDto> Handle(UpdateProjectSettingsCommand cmd, CancellationToken ct)
    {
        var project = await _db.Projects.FindAsync(new object[] { cmd.ProjectId }, cancellationToken: ct);
        if (project == null) throw new InvalidOperationException("Project not found");

        if (!string.IsNullOrEmpty(cmd.Settings.Description))
            project.Description = cmd.Settings.Description;

        if (!string.IsNullOrEmpty(cmd.Settings.Framework))
            project.Framework = cmd.Settings.Framework;

        if (!string.IsNullOrEmpty(cmd.Settings.Theme))
            project.Theme = cmd.Settings.Theme;

        await _db.SaveChangesAsync(ct);

        var versionCount = await _db.Versions.CountAsync(v => v.ProjectId == project.Id, ct);

        return new ProjectDto
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            Framework = project.Framework,
            Theme = project.Theme,
            CreatedAt = project.CreatedAt,
            VersionCount = versionCount
        };
    }
}

/// <summary>
/// Handler for GetProjectQuery
/// </summary>
public class GetProjectHandler : IRequestHandler<GetProjectQuery, ProjectDto?>
{
    private readonly ProjectDbContext _db;

    public GetProjectHandler(ProjectDbContext db)
    {
        _db = db;
    }

    public async Task<ProjectDto?> Handle(GetProjectQuery cmd, CancellationToken ct)
    {
        var project = await _db.Projects.FindAsync(new object[] { cmd.ProjectId }, cancellationToken: ct);
        if (project == null) return null;

        var versionCount = await _db.Versions.CountAsync(v => v.ProjectId == project.Id, ct);

        return new ProjectDto
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            Framework = project.Framework,
            Theme = project.Theme,
            CreatedAt = project.CreatedAt,
            VersionCount = versionCount
        };
    }
}

/// <summary>
/// Handler for GetUserProjectsQuery
/// </summary>
public class GetUserProjectsHandler : IRequestHandler<GetUserProjectsQuery, List<ProjectDto>>
{
    private readonly ProjectDbContext _db;

    public GetUserProjectsHandler(ProjectDbContext db)
    {
        _db = db;
    }

    public async Task<List<ProjectDto>> Handle(GetUserProjectsQuery cmd, CancellationToken ct)
    {
        var projects = await _db.Projects
            .Where(p => p.OwnerId == cmd.OwnerId)
            .ToListAsync(ct);

        var result = new List<ProjectDto>();

        foreach (var project in projects)
        {
            var versionCount = await _db.Versions.CountAsync(v => v.ProjectId == project.Id, ct);

            result.Add(new ProjectDto
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                Framework = project.Framework,
                Theme = project.Theme,
                CreatedAt = project.CreatedAt,
                VersionCount = versionCount
            });
        }

        return result;
    }
}

/// <summary>
/// Handler for GetProjectVersionsQuery
/// </summary>
public class GetProjectVersionsHandler : IRequestHandler<GetProjectVersionsQuery, List<ProjectVersionDto>>
{
    private readonly ProjectDbContext _db;

    public GetProjectVersionsHandler(ProjectDbContext db)
    {
        _db = db;
    }

    public async Task<List<ProjectVersionDto>> Handle(GetProjectVersionsQuery cmd, CancellationToken ct)
    {
        var versions = await _db.Versions
            .Where(v => v.ProjectId == cmd.ProjectId)
            .OrderByDescending(v => v.VersionNumber)
            .ToListAsync(ct);

        var result = new List<ProjectVersionDto>();

        foreach (var version in versions)
        {
            var artifactCount = await _db.Artifacts.CountAsync(a => a.VersionId == version.Id, ct);

            result.Add(new ProjectVersionDto
            {
                Id = version.Id,
                VersionNumber = version.VersionNumber,
                CreatedAt = version.CreatedAt,
                ArtifactCount = artifactCount
            });
        }

        return result;
    }
}
