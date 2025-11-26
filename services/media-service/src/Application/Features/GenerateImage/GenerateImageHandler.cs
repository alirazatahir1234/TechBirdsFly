using MediatR;
using TechBirdsFly.MediaService.Domain.Entities;
using TechBirdsFly.MediaService.Domain.Interfaces;

namespace TechBirdsFly.MediaService.Application.Features.GenerateImage;

public class GenerateImageHandler : IRequestHandler<GenerateImageCommand, GenerateImageResponse>
{
    private readonly IImageAIService _aiService;
    private readonly IMediaRepository _repo;
    private readonly ILogger<GenerateImageHandler> _logger;

    public GenerateImageHandler(IImageAIService aiService, IMediaRepository repo, ILogger<GenerateImageHandler> logger)
    {
        _aiService = aiService;
        _repo = repo;
        _logger = logger;
    }

    public async Task<GenerateImageResponse> Handle(GenerateImageCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Generating image with prompt: {Prompt}", request.Prompt);

        var imageBytes = await _aiService.GenerateImageAsync(request.Prompt);
        var base64Image = Convert.ToBase64String(imageBytes);

        var prompt = $"{request.Prompt}" + (request.Style != null ? $" - Style: {request.Style}" : "");
        var file = new MediaFile($"generated-{Guid.NewGuid()}.png", "", "image/png", imageBytes.Length, prompt);

        await _repo.AddAsync(file);
        await _repo.SaveChangesAsync();

        _logger.LogInformation("Image generated successfully with ID: {Id}", file.Id);
        return new GenerateImageResponse(file.Id, base64Image, request.Prompt, request.Style);
    }
}
