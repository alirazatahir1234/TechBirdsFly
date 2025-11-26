using MediatR;
using GeneratorService.Application.DTOs;

namespace GeneratorService.Application.Features.GetAllProjects;

/// <summary>
/// Query to retrieve all projects
/// </summary>
public record GetAllProjectsQuery(int? Skip = 0, int? Take = 50) : IRequest<GetAllProjectsResponse>;

/// <summary>
/// Response for GetAllProjectsQuery
/// </summary>
public record GetAllProjectsResponse(List<ProjectDto> Projects, int Total);
