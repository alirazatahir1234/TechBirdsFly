using MediatR;
using AutoMapper;
using GeneratorService.Domain.Interfaces;
using GeneratorService.Domain.Exceptions;
using GeneratorService.Application.Features.GetProject;

namespace GeneratorService.Application.Features.GetProject;

/// <summary>
/// Handler for GetProjectQuery
/// </summary>
public class GetProjectHandler : IRequestHandler<GetProjectQuery, GetProjectResponse>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetProjectHandler> _logger;

    public GetProjectHandler(
        IProjectRepository projectRepository,
        IMapper mapper,
        ILogger<GetProjectHandler> logger)
    {
        _projectRepository = projectRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<GetProjectResponse> Handle(GetProjectQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving project with ID: {ProjectId}", request.ProjectId);

        var project = await _projectRepository.GetByIdAsync(request.ProjectId, cancellationToken);

        if (project == null)
        {
            _logger.LogWarning("Project not found with ID: {ProjectId}", request.ProjectId);
            throw new ResourceNotFoundException("Project", request.ProjectId);
        }

        var projectDto = _mapper.Map<Application.DTOs.ProjectDto>(project);
        return new GetProjectResponse(projectDto);
    }
}
