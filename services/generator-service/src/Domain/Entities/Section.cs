using GeneratorService.Domain.Common;
using GeneratorService.Domain.ValueObjects;

namespace GeneratorService.Domain.Entities;

/// <summary>
/// Section represents a section of a website (Hero, Features, About, etc.)
/// Aggregate: Section can be part of a Project
/// </summary>
public class Section : AuditableEntity
{
    public Guid ProjectId { get; private set; }
    public SectionType Type { get; private set; }
    public HtmlContent Html { get; private set; }
    public string CssClass { get; private set; } = string.Empty;

    private Section() { }

    public Section(Guid projectId, SectionType type, HtmlContent html, string cssClass = "")
    {
        ProjectId = projectId;
        Type = type;
        Html = html ?? throw new ArgumentNullException(nameof(html));
        CssClass = cssClass ?? string.Empty;
    }

    /// <summary>
    /// Updates the HTML content of the section
    /// </summary>
    public void UpdateHtml(HtmlContent newHtml)
    {
        Html = newHtml ?? throw new ArgumentNullException(nameof(newHtml));
        Touch();
    }

    /// <summary>
    /// Updates the CSS class styling for the section
    /// </summary>
    public void UpdateCssClass(string newCssClass)
    {
        CssClass = newCssClass ?? string.Empty;
        Touch();
    }
}
