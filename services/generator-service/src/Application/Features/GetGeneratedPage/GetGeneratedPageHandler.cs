using MediatR;
using AutoMapper;
using GeneratorService.Domain.Interfaces;
using GeneratorService.Domain.Exceptions;
using GeneratorService.Application.Features.GetGeneratedPage;

namespace GeneratorService.Application.Features.GetGeneratedPage;

/// <summary>
/// Handler for GetGeneratedPageQuery
/// </summary>
public class GetGeneratedPageHandler : IRequestHandler<GetGeneratedPageQuery, GetGeneratedPageResponse>
{
    private readonly IGeneratedPageRepository _pageRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetGeneratedPageHandler> _logger;

    public GetGeneratedPageHandler(
        IGeneratedPageRepository pageRepository,
        IMapper mapper,
        ILogger<GetGeneratedPageHandler> logger)
    {
        _pageRepository = pageRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<GetGeneratedPageResponse> Handle(GetGeneratedPageQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving generated page with ID: {PageId}", request.PageId);

        var page = await _pageRepository.GetByIdAsync(request.PageId, cancellationToken);

        if (page == null)
        {
            _logger.LogWarning("Generated page not found with ID: {PageId}", request.PageId);
            throw new ResourceNotFoundException("GeneratedPage", request.PageId);
        }

        var pageDto = _mapper.Map<Application.DTOs.GeneratedPageDto>(page);
        return new GetGeneratedPageResponse(pageDto);
    }
}
