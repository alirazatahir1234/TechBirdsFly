using MediatR;

namespace TechBirdsFly.EditorService.Application.Features.RegenerateSection;

public record RegenerateSectionCommand(Guid SectionId) : IRequest<string>;
