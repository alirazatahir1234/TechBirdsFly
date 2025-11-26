using MediatR;
using GeneratorService.Application.DTOs;

namespace GeneratorService.Application.Features.GetProject;

/// <summary>
/// Query to retrieve a single project by ID
/// </summary>
public record GetProjectQuery(Guid ProjectId) : IRequest<GetProjectResponse>;

/// <summary>
/// Response for GetProjectQuery
/// </summary>
public record GetProjectResponse(ProjectDto? Project);
