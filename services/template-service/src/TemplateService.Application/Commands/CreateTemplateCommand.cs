using MediatR;
using TemplateService.Application.DTOs;

namespace TemplateService.Application.Commands;

/// <summary>
/// CQRS command to create a new template
/// </summary>
public record CreateTemplateCommand(CreateTemplateRequest Request)
    : IRequest<TemplateDto>;
