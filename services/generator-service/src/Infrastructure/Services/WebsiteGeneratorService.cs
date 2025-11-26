using GeneratorService.Application.DTOs;
using GeneratorService.Application.Interfaces;
using GeneratorService.Infrastructure.AI;

namespace GeneratorService.Infrastructure.Services;

/// <summary>
/// WebsiteGeneratorService implements the IWebsiteGenerator interface
/// Orchestrates the AI-powered website generation pipeline
/// </summary>
public class WebsiteGeneratorService : IWebsiteGenerator
{
    private readonly ILlamaService _llamaService;
    private readonly PromptBuilder _promptBuilder;
    private readonly HtmlTemplateBuilder _htmlTemplateBuilder;

    public WebsiteGeneratorService(
        ILlamaService llamaService,
        PromptBuilder promptBuilder,
        HtmlTemplateBuilder htmlTemplateBuilder)
    {
        _llamaService = llamaService;
        _promptBuilder = promptBuilder;
        _htmlTemplateBuilder = htmlTemplateBuilder;
    }

    /// <summary>
    /// Generates a complete website from specification parameters
    /// </summary>
    public async Task<GeneratedWebsiteDto> GenerateWebsiteAsync(
        string prompt,
        string industry,
        string style,
        string palette,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(prompt);
        ArgumentException.ThrowIfNullOrWhiteSpace(industry);

        try
        {
            // Step 1: Build AI prompt from parameters
            var fullPrompt = _promptBuilder
                .SetContext($"Industry: {industry}, Style: {style}, Color Palette: {palette}")
                .SetTask($"Generate a professional website layout for: {prompt}")
                .SetFormat("Return HTML sections with clear markers")
                .Build();

            // Step 2: Call Llama 3 AI to generate content
            var aiResponse = await _llamaService.GenerateTextAsync(fullPrompt, cancellationToken);

            // Step 3: Generate HTML using template builder
            var htmlBuilder = _htmlTemplateBuilder
                .SetPageTitle(prompt)
                .SetMetaDescription($"Professional website for {prompt}")
                .AddBodyContent(aiResponse);

            var htmlContent = htmlBuilder.BuildHtml();

            // Step 4: Parse color palette
            var colors = palette.Split(',').Select(c => c.Trim()).ToList();
            var primaryColor = colors.Count > 0 ? colors[0] : "#000000";
            var secondaryColor = colors.Count > 1 ? colors[1] : "#FFFFFF";

            // Step 5: Construct and return complete website DTO
            var generatedWebsite = new GeneratedWebsiteDto
            {
                Name = prompt,
                Industry = industry,
                Style = style,
                PrimaryColor = primaryColor,
                SecondaryColor = secondaryColor,
                Sections = new List<SectionDto>
                {
                    new SectionDto
                    {
                        Id = Guid.NewGuid(),
                        Type = "Hero",
                        HtmlContent = "<h1>Welcome to " + prompt + "</h1>",
                        CssClass = "hero-section"
                    }
                },
                Metadata = new MetadataDto
                {
                    Title = $"{prompt} - AI Generated Website",
                    Description = $"Professional website for {prompt}",
                    Keywords = $"{prompt}, {industry}, website"
                },
                FinalHtml = htmlContent
            };

            return generatedWebsite;
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException("Failed to generate website: AI service unavailable", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("An error occurred during website generation", ex);
        }
    }
}
