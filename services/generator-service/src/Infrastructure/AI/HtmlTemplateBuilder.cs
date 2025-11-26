namespace GeneratorService.Infrastructure.AI;

/// <summary>
/// Builds HTML templates based on AI-generated specifications
/// Creates production-ready HTML/CSS/JavaScript from AI outputs
/// </summary>
public interface IHtmlTemplateBuilder
{
    IHtmlTemplateBuilder SetPageTitle(string title);
    IHtmlTemplateBuilder SetMetaDescription(string description);
    IHtmlTemplateBuilder AddHeadContent(string content);
    IHtmlTemplateBuilder AddBodyContent(string content);
    IHtmlTemplateBuilder AddStyle(string css);
    IHtmlTemplateBuilder AddScript(string js);
    string BuildHtml();
    string BuildCss();
    string BuildJs();
    void Clear();
}

public class HtmlTemplateBuilder : IHtmlTemplateBuilder
{
    private string _pageTitle = "Generated Website";
    private string _metaDescription = "AI-generated website";
    private string _headContent = string.Empty;
    private string _bodyContent = string.Empty;
    private string _css = string.Empty;
    private string _js = string.Empty;

    public IHtmlTemplateBuilder SetPageTitle(string title)
    {
        _pageTitle = title;
        return this;
    }

    public IHtmlTemplateBuilder SetMetaDescription(string description)
    {
        _metaDescription = description;
        return this;
    }

    public IHtmlTemplateBuilder AddHeadContent(string content)
    {
        _headContent += content + Environment.NewLine;
        return this;
    }

    public IHtmlTemplateBuilder AddBodyContent(string content)
    {
        _bodyContent += content + Environment.NewLine;
        return this;
    }

    public IHtmlTemplateBuilder AddStyle(string css)
    {
        _css += css + Environment.NewLine;
        return this;
    }

    public IHtmlTemplateBuilder AddScript(string js)
    {
        _js += js + Environment.NewLine;
        return this;
    }

    public string BuildHtml()
    {
        var html = new System.Text.StringBuilder();

        html.AppendLine("<!DOCTYPE html>");
        html.AppendLine("<html lang=\"en\">");
        html.AppendLine("<head>");
        html.AppendLine($"  <title>{_pageTitle}</title>");
        html.AppendLine($"  <meta name=\"description\" content=\"{_metaDescription}\">");
        html.AppendLine("  <meta charset=\"UTF-8\">");
        html.AppendLine("  <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");

        if (!string.IsNullOrWhiteSpace(_css))
        {
            html.AppendLine("  <style>");
            html.AppendLine(_css);
            html.AppendLine("  </style>");
        }

        html.AppendLine(_headContent);
        html.AppendLine("</head>");
        html.AppendLine("<body>");
        html.AppendLine(_bodyContent);

        if (!string.IsNullOrWhiteSpace(_js))
        {
            html.AppendLine("  <script>");
            html.AppendLine(_js);
            html.AppendLine("  </script>");
        }

        html.AppendLine("</body>");
        html.AppendLine("</html>");

        return html.ToString();
    }

    public string BuildCss()
    {
        return _css;
    }

    public string BuildJs()
    {
        return _js;
    }

    public void Clear()
    {
        _pageTitle = "Generated Website";
        _metaDescription = "AI-generated website";
        _headContent = string.Empty;
        _bodyContent = string.Empty;
        _css = string.Empty;
        _js = string.Empty;
    }
}
