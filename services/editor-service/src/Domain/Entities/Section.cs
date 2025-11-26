using TechBirdsFly.EditorService.Domain.Common;

namespace TechBirdsFly.EditorService.Domain.Entities;

/// <summary>
/// Represents a section of a website project.
/// Can be header, hero, features, testimonials, footer, etc.
/// </summary>
public class Section : BaseEntity
{
    public Guid ProjectId { get; private set; }
    public string Type { get; private set; } // hero, features, testimonials, footer, etc.
    public string Html { get; private set; } // The HTML content of this section
    public string? Css { get; private set; } // Optional custom CSS for this section
    public int Order { get; private set; } // Display order (1, 2, 3, ...)

    public Section(Guid projectId, string type, string html, int order, string? css = null)
    {
        ProjectId = projectId;
        Type = type;
        Html = html;
        Order = order;
        Css = css;
    }

    /// <summary>
    /// Update the HTML content of this section
    /// </summary>
    public void UpdateHtml(string html)
    {
        Html = html;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Update the CSS of this section
    /// </summary>
    public void UpdateCss(string? css)
    {
        Css = css;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Reorder this section
    /// </summary>
    public void UpdateOrder(int order)
    {
        Order = order;
        UpdatedAt = DateTime.UtcNow;
    }
}
