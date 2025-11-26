using MediatR;
using GeneratorService.Application.DTOs;

namespace GeneratorService.Application.Features.GetProjectSections;

/// <summary>
/// Query to retrieve all sections for a project
/// </summary>
public record GetProjectSectionsQuery(Guid ProjectId, string? SectionType = null) : IRequest<GetProjectSectionsResponse>;

/// <summary>
/// Response for GetProjectSectionsQuery
/// </summary>
public record GetProjectSectionsResponse(List<SectionDto> Sections, int Total);
