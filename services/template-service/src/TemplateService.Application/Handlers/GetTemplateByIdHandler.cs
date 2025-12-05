using MediatR;
using TemplateService.Application.DTOs;
using TemplateService.Application.Queries;
using TemplateService.Domain.Interfaces;

namespace TemplateService.Application.Handlers;

/// <summary>
/// Handler for GetTemplateByIdQuery
/// </summary>
public class GetTemplateByIdHandler : IRequestHandler<GetTemplateByIdQuery, TemplateDto?>
{
    private readonly ITemplateRepository _repository;

    public GetTemplateByIdHandler(ITemplateRepository repository)
    {
        _repository = repository;
    }

    public async Task<TemplateDto?> Handle(GetTemplateByIdQuery query, CancellationToken cancellationToken)
    {
        var template = await _repository.GetTemplateByIdAsync(query.TemplateId);

        if (template == null)
            return null;

        return new TemplateDto
        {
            Id = template.Id,
            Name = template.Name,
            Category = template.Category,
            Description = template.Description,
            PreviewImageUrl = template.PreviewImageUrl,
            CreatedAt = template.CreatedAt
        };
    }
}
