using MediatR;
using TemplateService.Application.Commands;
using TemplateService.Application.DTOs;
using TemplateService.Domain.Entities;
using TemplateService.Domain.Interfaces;

namespace TemplateService.Application.Handlers;

/// <summary>
/// Handler for CreateTemplateCommand
/// </summary>
public class CreateTemplateHandler : IRequestHandler<CreateTemplateCommand, TemplateDto>
{
    private readonly ITemplateRepository _repository;

    public CreateTemplateHandler(ITemplateRepository repository)
    {
        _repository = repository;
    }

    public async Task<TemplateDto> Handle(CreateTemplateCommand command, CancellationToken cancellationToken)
    {
        var template = new Template
        {
            Name = command.Request.Name,
            Category = command.Request.Category.ToLower(),
            Description = command.Request.Description
        };

        await _repository.CreateTemplateAsync(template);

        return new TemplateDto
        {
            Id = template.Id,
            Name = template.Name,
            Category = template.Category,
            Description = template.Description,
            CreatedAt = template.CreatedAt
        };
    }
}
