using GeneratorService.Domain.Common;
using GeneratorService.Domain.ValueObjects;

namespace GeneratorService.Domain.Entities;

/// <summary>
/// Project represents a website project
/// Aggregate Root: manages sections and metadata for a website
/// </summary>
public class Project : AuditableEntity
{
    public string Name { get; private set; }
    public string Industry { get; private set; }
    public string Style { get; private set; }
    public ColorPalette Palette { get; private set; }
    public string Description { get; private set; }

    private readonly List<Section> _sections = new();
    public IReadOnlyCollection<Section> Sections => _sections.AsReadOnly();

    private Project() { }

    public Project(string name, string industry, string style, ColorPalette palette, string description = "")
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Project name cannot be empty", nameof(name));
        if (!WebsiteIndustry.IsValid(industry))
            throw new ArgumentException($"Invalid industry: {industry}", nameof(industry));
        if (!WebsiteStyle.IsValid(style))
            throw new ArgumentException($"Invalid style: {style}", nameof(style));

        Name = name;
        Industry = industry;
        Style = style;
        Palette = palette ?? throw new ArgumentNullException(nameof(palette));
        Description = description ?? string.Empty;
    }

    /// <summary>
    /// Adds a section to the project
    /// </summary>
    public void AddSection(Section section)
    {
        if (section == null)
            throw new ArgumentNullException(nameof(section));
        if (section.ProjectId != Id)
            throw new InvalidOperationException("Section does not belong to this project");

        _sections.Add(section);
        Touch();
    }

    /// <summary>
    /// Removes a section from the project
    /// </summary>
    public void RemoveSection(Section section)
    {
        if (section == null)
            throw new ArgumentNullException(nameof(section));

        _sections.Remove(section);
        Touch();
    }

    /// <summary>
    /// Checks if project has a section of a specific type
    /// </summary>
    public bool HasSectionType(SectionType type)
    {
        return _sections.Any(s => s.Type == type);
    }

    /// <summary>
    /// Gets all sections of a specific type
    /// </summary>
    public IReadOnlyCollection<Section> GetSectionsByType(SectionType type)
    {
        return _sections.Where(s => s.Type == type).ToList().AsReadOnly();
    }

    /// <summary>
    /// Updates project metadata
    /// </summary>
    public void Update(string name, string style, ColorPalette palette, string description = "")
    {
        if (!string.IsNullOrWhiteSpace(name))
            Name = name;
        if (!string.IsNullOrWhiteSpace(style) && WebsiteStyle.IsValid(style))
            Style = style;
        if (palette != null)
            Palette = palette;
        if (!string.IsNullOrWhiteSpace(description))
            Description = description;

        Touch();
    }

    /// <summary>
    /// Gets total number of sections
    /// </summary>
    public int SectionCount => _sections.Count;
}
