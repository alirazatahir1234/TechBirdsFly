using System.Text;
using System.Text.Json;
using TechBirdsFly.MediaService.Domain.Interfaces;

namespace TechBirdsFly.MediaService.Infrastructure.AI;

public class ImageAIService : IImageAIService
{
    private readonly HttpClient _httpClient;
    private readonly string _llamaBaseUrl;
    private readonly ILogger<ImageAIService> _logger;

    public ImageAIService(HttpClient httpClient, IConfiguration config, ILogger<ImageAIService> logger)
    {
        _httpClient = httpClient;
        _llamaBaseUrl = config["AI:LlamaBaseUrl"] ?? "http://localhost:11434";
        _logger = logger;
    }

    public async Task<byte[]> GenerateImageAsync(string prompt)
    {
        _logger.LogInformation("Generating image with prompt: {Prompt}", prompt);

        try
        {
            var payload = new { model = "llava", prompt };
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_llamaBaseUrl}/api/generate", content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var imageBytes = Encoding.UTF8.GetBytes(responseContent);

            _logger.LogInformation("Image generated successfully. Size: {Size} bytes", imageBytes.Length);
            return imageBytes;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating image with prompt: {Prompt}", prompt);
            throw;
        }
    }
}
