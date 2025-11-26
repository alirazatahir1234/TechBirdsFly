using MediatR;
using AutoMapper;
using GeneratorService.Domain.Interfaces;
using GeneratorService.Domain.ValueObjects;
using GeneratorService.Domain.Exceptions;
using GeneratorService.Application.Features.GetProjectSections;

namespace GeneratorService.Application.Features.GetProjectSections;

/// <summary>
/// Handler for GetProjectSectionsQuery
/// </summary>
public class GetProjectSectionsHandler : IRequestHandler<GetProjectSectionsQuery, GetProjectSectionsResponse>
{
    private readonly ISectionRepository _sectionRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetProjectSectionsHandler> _logger;

    public GetProjectSectionsHandler(
        ISectionRepository sectionRepository,
        IProjectRepository projectRepository,
        IMapper mapper,
        ILogger<GetProjectSectionsHandler> logger)
    {
        _sectionRepository = sectionRepository;
        _projectRepository = projectRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<GetProjectSectionsResponse> Handle(GetProjectSectionsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving sections for project {ProjectId}, filter: {SectionType}", request.ProjectId, request.SectionType);

        // Verify project exists
        var projectExists = await _projectRepository.ExistsAsync(request.ProjectId, cancellationToken);
        if (!projectExists)
        {
            throw new ResourceNotFoundException("Project", request.ProjectId);
        }

        // Get sections
        var sections = await _sectionRepository.GetByProjectIdAsync(request.ProjectId, cancellationToken);

        // Filter by type if specified
        if (!string.IsNullOrWhiteSpace(request.SectionType))
        {
            if (Enum.TryParse<SectionType>(request.SectionType, out var sectionType))
            {
                sections = sections.Where(s => s.Type == sectionType).ToList();
            }
            else
            {
                _logger.LogWarning("Invalid section type filter: {SectionType}", request.SectionType);
            }
        }

        var sectionDtos = _mapper.Map<List<Application.DTOs.SectionDto>>(sections);

        _logger.LogInformation("Retrieved {Count} sections for project {ProjectId}", sectionDtos.Count, request.ProjectId);

        return new GetProjectSectionsResponse(sectionDtos, sectionDtos.Count);
    }
}
