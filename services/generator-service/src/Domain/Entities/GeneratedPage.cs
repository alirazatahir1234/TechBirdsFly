using GeneratorService.Domain.Common;
using GeneratorService.Domain.ValueObjects;

namespace GeneratorService.Domain.Entities;

/// <summary>
/// GeneratedPage represents a complete generated website page
/// Contains HTML, CSS, JS, and metadata
/// </summary>
public class GeneratedPage : AuditableEntity
{
    public string Title { get; private set; } = default!;
    public HtmlContent Html { get; private set; } = default!;
    public string Css { get; private set; } = default!;
    public string JavaScript { get; private set; } = default!;
    public Metadata Meta { get; private set; } = default!;
    public int Version { get; private set; } = 1;
    public bool IsPublished { get; private set; } = false;

    private GeneratedPage() { }

    public GeneratedPage(string title, HtmlContent html, string css, string javascript, Metadata meta)
    {
        Title = title ?? throw new ArgumentNullException(nameof(title));
        Html = html ?? throw new ArgumentNullException(nameof(html));
        Css = css ?? string.Empty;
        JavaScript = javascript ?? string.Empty;
        Meta = meta ?? throw new ArgumentNullException(nameof(meta));
    }

    /// <summary>
    /// Updates the page HTML content
    /// Increments version number
    /// </summary>
    public void UpdateContent(HtmlContent newHtml, string newCss, string newJavaScript)
    {
        Html = newHtml ?? throw new ArgumentNullException(nameof(newHtml));
        Css = newCss ?? string.Empty;
        JavaScript = newJavaScript ?? string.Empty;
        Version++;
        Touch();
    }

    /// <summary>
    /// Publishes the page (marks as public)
    /// </summary>
    public void Publish()
    {
        IsPublished = true;
        Touch();
    }

    /// <summary>
    /// Unpublishes the page
    /// </summary>
    public void Unpublish()
    {
        IsPublished = false;
        Touch();
    }

    /// <summary>
    /// Gets the full HTML with embedded CSS and JS
    /// </summary>
    public string GetFullHtml()
    {
        var html = new System.Text.StringBuilder();
        html.AppendLine("<!DOCTYPE html>");
        html.AppendLine("<html lang=\"en\">");
        html.AppendLine("<head>");
        html.AppendLine($"  <title>{Meta.Title}</title>");
        html.AppendLine($"  <meta name=\"description\" content=\"{Meta.Description}\">");
        html.AppendLine($"  <meta name=\"keywords\" content=\"{Meta.Keywords}\">");
        html.AppendLine("  <meta charset=\"UTF-8\">");
        html.AppendLine("  <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");

        if (!string.IsNullOrWhiteSpace(Css))
        {
            html.AppendLine("  <style>");
            html.AppendLine(Css);
            html.AppendLine("  </style>");
        }

        html.AppendLine("</head>");
        html.AppendLine("<body>");
        html.AppendLine(Html.Value);

        if (!string.IsNullOrWhiteSpace(JavaScript))
        {
            html.AppendLine("  <script>");
            html.AppendLine(JavaScript);
            html.AppendLine("  </script>");
        }

        html.AppendLine("</body>");
        html.AppendLine("</html>");

        return html.ToString();
    }
}
