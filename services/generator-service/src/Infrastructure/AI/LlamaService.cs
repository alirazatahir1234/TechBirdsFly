using Microsoft.Extensions.Configuration;

namespace GeneratorService.Infrastructure.AI;

/// <summary>
/// Llama service for high-level AI text generation
/// Wraps OllamaClient with business logic and model management
/// </summary>
public interface ILlamaService
{
    Task<string> GenerateTextAsync(string prompt, CancellationToken cancellationToken = default);
    Task<IEnumerable<string>> ListAvailableModelsAsync(CancellationToken cancellationToken = default);
}

public class LlamaService : ILlamaService
{
    private readonly IConfiguration _config;
    private readonly OllamaClient _client;

    public LlamaService(IConfiguration config)
    {
        _config = config;
        _client = new OllamaClient(config);
    }

    /// <summary>
    /// Generates text using the configured Llama model
    /// </summary>
    public async Task<string> GenerateTextAsync(string prompt, CancellationToken cancellationToken = default)
    {
        var model = _config["Ollama:Model"] ?? "llama3";
        return await _client.GenerateAsync(model, prompt, cancellationToken);
    }

    /// <summary>
    /// Lists all available models from the Ollama instance
    /// </summary>
    public async Task<IEnumerable<string>> ListAvailableModelsAsync(CancellationToken cancellationToken = default)
    {
        return await _client.ListModelsAsync(cancellationToken);
    }
}
