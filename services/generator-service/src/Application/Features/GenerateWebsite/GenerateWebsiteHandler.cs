using MediatR;
using GeneratorService.Infrastructure.AI;

namespace GeneratorService.Application.Features.GenerateWebsite;

/// <summary>
/// Handler for GenerateWebsiteCommand
/// Orchestrates AI-based website generation
/// </summary>
public class GenerateWebsiteHandler : IRequestHandler<GenerateWebsiteCommand, GenerateWebsiteResponse>
{
    private readonly ILlamaService _llamaService;
    private readonly IPromptBuilder _promptBuilder;
    private readonly IHtmlTemplateBuilder _templateBuilder;

    public GenerateWebsiteHandler(
        ILlamaService llamaService,
        IPromptBuilder promptBuilder,
        IHtmlTemplateBuilder templateBuilder)
    {
        _llamaService = llamaService;
        _promptBuilder = promptBuilder;
        _templateBuilder = templateBuilder;
    }

    public async Task<GenerateWebsiteResponse> Handle(GenerateWebsiteCommand request, CancellationToken cancellationToken)
    {
        // Build AI prompt
        var prompt = _promptBuilder
            .SetContext($"Create a professional website for a {request.Industry} business")
            .SetTask($"Generate HTML, CSS, and JavaScript for a website called '{request.ProjectName}'")
            .AddConstraint($"Description: {request.Description}")
            .AddConstraint($"Color scheme: {request.ColorScheme}")
            .AddConstraint($"Features: {string.Join(", ", request.Features)}")
            .SetFormat("Return as structured HTML, CSS, and JavaScript")
            .Build();

        // Generate content via AI
        var aiResponse = await _llamaService.GenerateTextAsync(prompt, cancellationToken);

        // Build HTML template
        var htmlContent = _templateBuilder
            .SetPageTitle(request.ProjectName)
            .SetMetaDescription(request.Description)
            .AddBodyContent("<h1>" + request.ProjectName + "</h1><p>" + request.Description + "</p>")
            .BuildHtml();

        return new GenerateWebsiteResponse
        {
            ProjectId = Guid.NewGuid(),
            ProjectName = request.ProjectName,
            HtmlContent = htmlContent,
            CssContent = _templateBuilder.BuildCss(),
            JsContent = _templateBuilder.BuildJs(),
            GeneratedAt = DateTime.UtcNow,
            Status = "Success"
        };
    }
}
