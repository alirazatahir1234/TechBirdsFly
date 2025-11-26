namespace GeneratorService.Domain.Entities;

/// <summary>
/// WebsiteStyle contains allowed styles for website generation
/// </summary>
public class WebsiteStyle
{
    public static readonly string[] Allowed =
    {
        "Modern",
        "Minimal",
        "Bold",
        "Corporate",
        "Creative",
        "Luxury",
        "Playful",
        "Professional"
    };

    public static bool IsValid(string style)
    {
        return !string.IsNullOrWhiteSpace(style) && Allowed.Contains(style);
    }
}
