using MediatR;
using GeneratorService.Application.DTOs;

namespace GeneratorService.Application.Features.GetGeneratedPage;

/// <summary>
/// Query to retrieve a generated page
/// </summary>
public record GetGeneratedPageQuery(Guid PageId) : IRequest<GetGeneratedPageResponse>;

/// <summary>
/// Response for GetGeneratedPageQuery
/// </summary>
public record GetGeneratedPageResponse(GeneratedPageDto? Page);
