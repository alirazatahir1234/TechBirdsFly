namespace GeneratorService.Domain.Entities;

/// <summary>
/// AiGenerationRequest represents a request for AI-based website generation
/// Contains prompt and configuration for the AI model
/// </summary>
public class AiGenerationRequest
{
    public string Prompt { get; private set; }
    public string Industry { get; private set; }
    public string Style { get; private set; }
    public string ColorPalette { get; private set; }
    public DateTime RequestedAt { get; private set; } = DateTime.UtcNow;

    public AiGenerationRequest(string prompt, string industry, string style, string palette)
    {
        if (string.IsNullOrWhiteSpace(prompt))
            throw new ArgumentException("Prompt cannot be empty", nameof(prompt));
        if (!WebsiteIndustry.IsValid(industry))
            throw new ArgumentException($"Invalid industry: {industry}", nameof(industry));
        if (!WebsiteStyle.IsValid(style))
            throw new ArgumentException($"Invalid style: {style}", nameof(style));
        if (string.IsNullOrWhiteSpace(palette))
            throw new ArgumentException("Color palette cannot be empty", nameof(palette));

        Prompt = prompt;
        Industry = industry;
        Style = style;
        ColorPalette = palette;
    }
}
