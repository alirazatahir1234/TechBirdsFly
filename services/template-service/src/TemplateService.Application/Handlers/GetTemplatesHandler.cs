using MediatR;
using TemplateService.Application.DTOs;
using TemplateService.Application.Queries;
using TemplateService.Domain.Interfaces;

namespace TemplateService.Application.Handlers;

/// <summary>
/// Handler for GetTemplatesQuery
/// </summary>
public class GetTemplatesHandler : IRequestHandler<GetTemplatesQuery, List<TemplateDto>>
{
    private readonly ITemplateRepository _repository;

    public GetTemplatesHandler(ITemplateRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<TemplateDto>> Handle(GetTemplatesQuery query, CancellationToken cancellationToken)
    {
        var templates = await _repository.GetTemplatesAsync(query.Category, query.Search);

        return templates.Select(t => new TemplateDto
        {
            Id = t.Id,
            Name = t.Name,
            Category = t.Category,
            Description = t.Description,
            PreviewImageUrl = t.PreviewImageUrl,
            CreatedAt = t.CreatedAt
        }).ToList();
    }
}
