using MediatR;

namespace TechBirdsFly.MediaService.Application.Features.GenerateImage;

public record GenerateImageCommand(
    string Prompt,
    string? Style = null,
    int Width = 512,
    int Height = 512
) : IRequest<GenerateImageResponse>;

public record GenerateImageResponse(
    Guid Id,
    string Base64Image,
    string Prompt,
    string? Style
);
