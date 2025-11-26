namespace GeneratorService.Application.Interfaces;

using GeneratorService.Application.DTOs;

/// <summary>
/// Interface for website generation using AI
/// Implemented by Infrastructure layer using Ollama/Llama3
/// </summary>
public interface IWebsiteGenerator
{
    /// <summary>
    /// Generates a complete website based on the provided parameters
    /// </summary>
    /// <param name="prompt">User prompt describing the desired website</param>
    /// <param name="industry">Industry type (Technology, E-Commerce, Portfolio, etc.)</param>
    /// <param name="style">Design style (Modern, Minimal, Bold, etc.)</param>
    /// <param name="palette">Color palette specification</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Generated website DTO with sections and styling</returns>
    Task<GeneratedWebsiteDto> GenerateWebsiteAsync(
        string prompt,
        string industry,
        string style,
        string palette,
        CancellationToken cancellationToken = default
    );
}
