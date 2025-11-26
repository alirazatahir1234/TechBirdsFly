using MediatR;
using ProjectService.Application.Commands;
using ProjectService.Application.DTOs;
using ProjectService.Infrastructure;
using ProjectService.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();
builder.Services.AddCors(options => options.AddDefaultPolicy(builder =>
{
    builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
}));

// Infrastructure services
builder.Services.AddInfrastructureServices(builder.Configuration);

var app = builder.Build();

// Initialize database
await app.Services.InitializeDatabaseAsync();

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.MapHealthChecks("/health");

// ============================================================================
// PROJECT ENDPOINTS
// ============================================================================

/// <summary>
/// Create a new project
/// POST /api/projects
/// </summary>
app.MapPost("/api/projects", async (CreateProjectRequest req, IMediator mediator) =>
{
    var result = await mediator.Send(new CreateProjectCommand(req));
    return Results.Created($"/api/projects/{result.Project.Id}", result);
})
.WithName("CreateProject")
.WithOpenApi()
.Produces<CreateProjectResponse>(StatusCodes.Status201Created)
.WithDescription("Create a new project");

/// <summary>
/// Get a specific project
/// GET /api/projects/{projectId}
/// </summary>
app.MapGet("/api/projects/{projectId:guid}", async (Guid projectId, IMediator mediator) =>
{
    var result = await mediator.Send(new GetProjectQuery(projectId));
    return result != null ? Results.Ok(result) : Results.NotFound();
})
.WithName("GetProject")
.WithOpenApi()
.Produces<ProjectDto>()
.WithDescription("Get a specific project by ID");

/// <summary>
/// List all projects for a user
/// GET /api/projects/user/{ownerId}
/// </summary>
app.MapGet("/api/projects/user/{ownerId:guid}", async (Guid ownerId, IMediator mediator) =>
{
    var result = await mediator.Send(new GetUserProjectsQuery(ownerId));
    return Results.Ok(result);
})
.WithName("GetUserProjects")
.WithOpenApi()
.Produces<List<ProjectDto>>()
.WithDescription("Get all projects for a specific user");

/// <summary>
/// Rename a project
/// PUT /api/projects/{projectId}/rename
/// </summary>
app.MapPut("/api/projects/{projectId:guid}/rename", async (Guid projectId, RenameProjectRequest req, IMediator mediator) =>
{
    var result = await mediator.Send(new RenameProjectCommand(projectId, req.NewName));
    return result ? Results.Ok() : Results.NotFound();
})
.WithName("RenameProject")
.WithOpenApi()
.WithDescription("Rename a project");

/// <summary>
/// Update project settings
/// PUT /api/projects/{projectId}/settings
/// </summary>
app.MapPut("/api/projects/{projectId:guid}/settings", async (Guid projectId, UpdateProjectSettingsRequest req, IMediator mediator) =>
{
    var result = await mediator.Send(new UpdateProjectSettingsCommand(projectId, req));
    return Results.Ok(result);
})
.WithName("UpdateProjectSettings")
.WithOpenApi()
.Produces<ProjectDto>()
.WithDescription("Update project settings (description, framework, theme)");

/// <summary>
/// Delete a project
/// DELETE /api/projects/{projectId}
/// </summary>
app.MapDelete("/api/projects/{projectId:guid}", async (Guid projectId, IMediator mediator) =>
{
    var result = await mediator.Send(new DeleteProjectCommand(projectId));
    return result ? Results.Ok() : Results.NotFound();
})
.WithName("DeleteProject")
.WithOpenApi()
.WithDescription("Delete a project");

// ============================================================================
// VERSION ENDPOINTS
// ============================================================================

/// <summary>
/// Create a new version for a project
/// POST /api/projects/{projectId}/versions
/// </summary>
app.MapPost("/api/projects/{projectId:guid}/versions", async (Guid projectId, IMediator mediator) =>
{
    var result = await mediator.Send(new CreateVersionCommand(projectId));
    return Results.Created($"/api/projects/{projectId}/versions/{result.Id}", result);
})
.WithName("CreateVersion")
.WithOpenApi()
.Produces<ProjectVersionDto>(StatusCodes.Status201Created)
.WithDescription("Create a new version for a project");

/// <summary>
/// Get all versions of a project
/// GET /api/projects/{projectId}/versions
/// </summary>
app.MapGet("/api/projects/{projectId:guid}/versions", async (Guid projectId, IMediator mediator) =>
{
    var result = await mediator.Send(new GetProjectVersionsQuery(projectId));
    return Results.Ok(result);
})
.WithName("GetProjectVersions")
.WithOpenApi()
.Produces<List<ProjectVersionDto>>()
.WithDescription("Get all versions of a project");

// ============================================================================
// ARTIFACT ENDPOINTS
// ============================================================================

/// <summary>
/// Link an artifact from GeneratorService to a project version
/// POST /api/projects/versions/link-artifact
/// </summary>
app.MapPost("/api/projects/versions/link-artifact", async (LinkArtifactRequest req, IMediator mediator) =>
{
    var result = await mediator.Send(new LinkArtifactCommand(req.VersionId, req.ArtifactId, req.Type));
    return result ? Results.Ok() : Results.BadRequest();
})
.WithName("LinkArtifact")
.WithOpenApi()
.WithDescription("Link an artifact to a project version");

app.Run();

/// <summary>
/// Request DTO for linking artifacts
/// </summary>
public class LinkArtifactRequest
{
    /// <summary>
    /// Project version ID
    /// </summary>
    public Guid VersionId { get; set; }

    /// <summary>
    /// Artifact ID from GeneratorService
    /// </summary>
    public Guid ArtifactId { get; set; }

    /// <summary>
    /// Artifact type
    /// </summary>
    public string Type { get; set; } = default!;
}
