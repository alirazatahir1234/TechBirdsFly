using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;

namespace GeneratorService.Infrastructure.AI;

/// <summary>
/// Ollama client for communicating with the Ollama API
/// Handles low-level HTTP communication with Ollama endpoints
/// </summary>
public class OllamaClient
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;

    public OllamaClient(IConfiguration config)
    {
        _config = config;
        var baseUrl = config["Ollama:Endpoint"] ?? "http://localhost:11434";
        _http = new HttpClient { BaseAddress = new Uri(baseUrl) };
        
        // Add API key if configured
        var apiKey = config["Ollama:ApiKey"];
        if (!string.IsNullOrEmpty(apiKey))
        {
            _http.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
        }
    }

    /// <summary>
    /// Generates text based on a prompt using the specified model
    /// </summary>
    public async Task<string> GenerateAsync(string model, string prompt, CancellationToken cancellationToken = default)
    {
        var request = new OllamaGenerateRequest
        {
            Model = model,
            Prompt = prompt,
            Stream = false
        };

        try
        {
            var response = await _http.PostAsJsonAsync("/api/generate", request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadFromJsonAsync<OllamaGenerateResponse>(cancellationToken: cancellationToken);
            return json?.Response ?? string.Empty;
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException($"Failed to call Ollama API at {_http.BaseAddress}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Gets information about available models
    /// </summary>
    public async Task<IEnumerable<string>> ListModelsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _http.GetAsync("/api/tags", cancellationToken);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadFromJsonAsync<OllamaModelsResponse>(cancellationToken: cancellationToken);
            return json?.Models?.Select(m => m.Name) ?? Enumerable.Empty<string>();
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException($"Failed to list models from Ollama API: {ex.Message}", ex);
        }
    }
}

public class OllamaGenerateRequest
{
    public string Model { get; set; } = string.Empty;
    public string Prompt { get; set; } = string.Empty;
    public bool Stream { get; set; } = false;
}

public class OllamaGenerateResponse
{
    public string Response { get; set; } = string.Empty;
}

public class OllamaModelsResponse
{
    public List<OllamaModel>? Models { get; set; }
}

public class OllamaModel
{
    public string Name { get; set; } = string.Empty;
}
