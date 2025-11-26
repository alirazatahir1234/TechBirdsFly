using MediatR;

namespace TechBirdsFly.EditorService.Application.Features.DeleteSection;

public record DeleteSectionCommand(Guid Id) : IRequest;
