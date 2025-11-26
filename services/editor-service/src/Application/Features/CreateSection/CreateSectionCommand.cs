using MediatR;

namespace TechBirdsFly.EditorService.Application.Features.CreateSection;

public record CreateSectionCommand(
    Guid ProjectId,
    string Type,
    string Html,
    int Order,
    string? Css = null
) : IRequest<Guid>;
