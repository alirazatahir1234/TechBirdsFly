using MediatR;

namespace TechBirdsFly.EditorService.Application.Features.UpdateSection;

public record UpdateSectionCommand(
    Guid Id,
    string Html,
    string? Css = null
) : IRequest;
