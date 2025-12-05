using MediatR;
using TemplateService.Application.DTOs;

namespace TemplateService.Application.Queries;

/// <summary>
/// CQRS query to retrieve templates with filtering
/// </summary>
public record GetTemplatesQuery(
    string? Category = null,
    string? Search = null
) : IRequest<List<TemplateDto>>;
