using TechBirdsFly.EditorService.Application.Interfaces;

namespace TechBirdsFly.EditorService.Infrastructure.AI;

public interface ILlamaClient
{
    Task<string> GenerateAsync(string prompt);
}

public class SectionAIService : ISectionAIService
{
    private readonly ILlamaClient _llama;
    private readonly ILogger<SectionAIService> _logger;

    public SectionAIService(ILlamaClient llama, ILogger<SectionAIService> logger)
    {
        _llama = llama;
        _logger = logger;
    }

    public async Task<string> RegenerateHtmlAsync(string type, string oldHtml)
    {
        _logger.LogInformation("Regenerating section HTML for type: {Type}", type);

        var prompt = $@"
Regenerate ONLY the HTML of this section type: {type}.
Optimize and improve readability. Use TailwindCSS for styling.
Do NOT include any explanation, only return the HTML.
Keep the same structure but make it better.

Here is the existing HTML:

{oldHtml}
";

        var newHtml = await _llama.GenerateAsync(prompt);
        _logger.LogInformation("Section HTML regenerated successfully");
        
        return newHtml;
    }
}
