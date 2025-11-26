using MediatR;

namespace GeneratorService.Application.Features.GenerateWebsite;

/// <summary>
/// Command for generating a website using AI
/// </summary>
public class GenerateWebsiteCommand : IRequest<GenerateWebsiteResponse>
{
    public string ProjectName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Industry { get; set; } = string.Empty;
    public List<string> Features { get; set; } = new();
    public string ColorScheme { get; set; } = "blue";
    public bool IncludeContactForm { get; set; } = true;
}

/// <summary>
/// Response from website generation
/// </summary>
public class GenerateWebsiteResponse
{
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string HtmlContent { get; set; } = string.Empty;
    public string CssContent { get; set; } = string.Empty;
    public string JsContent { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
    public string Status { get; set; } = "Success";
}
