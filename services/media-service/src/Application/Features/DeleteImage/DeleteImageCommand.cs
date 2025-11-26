using MediatR;

namespace TechBirdsFly.MediaService.Application.Features.DeleteImage;

public record DeleteImageCommand(Guid Id) : IRequest<bool>;
