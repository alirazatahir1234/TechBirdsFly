namespace TechBirdsFly.MediaService.Domain.Interfaces;

public interface IImageAIService
{
    Task<byte[]> GenerateImageAsync(string prompt);
}
