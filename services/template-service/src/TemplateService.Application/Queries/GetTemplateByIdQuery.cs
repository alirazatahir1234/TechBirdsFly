using MediatR;
using TemplateService.Application.DTOs;

namespace TemplateService.Application.Queries;

/// <summary>
/// CQRS query to retrieve a specific template by ID
/// </summary>
public record GetTemplateByIdQuery(Guid TemplateId) : IRequest<TemplateDto?>;
