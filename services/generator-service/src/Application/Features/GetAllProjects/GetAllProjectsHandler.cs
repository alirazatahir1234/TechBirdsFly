using MediatR;
using AutoMapper;
using GeneratorService.Domain.Interfaces;
using GeneratorService.Application.Features.GetAllProjects;

namespace GeneratorService.Application.Features.GetAllProjects;

/// <summary>
/// Handler for GetAllProjectsQuery
/// </summary>
public class GetAllProjectsHandler : IRequestHandler<GetAllProjectsQuery, GetAllProjectsResponse>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAllProjectsHandler> _logger;

    public GetAllProjectsHandler(
        IProjectRepository projectRepository,
        IMapper mapper,
        ILogger<GetAllProjectsHandler> logger)
    {
        _projectRepository = projectRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<GetAllProjectsResponse> Handle(GetAllProjectsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving all projects (Skip: {Skip}, Take: {Take})", request.Skip ?? 0, request.Take ?? 50);

        var projects = (await _projectRepository.GetAllAsync(cancellationToken)).ToList();

        var skip = request.Skip ?? 0;
        var take = request.Take ?? 50;

        var paginatedProjects = projects
            .Skip(skip)
            .Take(take)
            .ToList();

        var projectDtos = _mapper.Map<List<Application.DTOs.ProjectDto>>(paginatedProjects);

        _logger.LogInformation("Retrieved {Count} projects", projectDtos.Count);

        return new GetAllProjectsResponse(projectDtos, projects.Count);
    }
}
