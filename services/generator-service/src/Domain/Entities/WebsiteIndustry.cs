namespace GeneratorService.Domain.Entities;

/// <summary>
/// WebsiteIndustry contains allowed industries for website generation
/// </summary>
public class WebsiteIndustry
{
    public static readonly string[] Allowed = new[]
    {
        "Technology",
        "E-Commerce",
        "Portfolio",
        "Blog",
        "Agency",
        "SaaS",
        "Healthcare",
        "Finance",
        "Education",
        "Real Estate",
        "Hospitality",
        "Retail"
    };

    public static bool IsValid(string industry)
    {
        return !string.IsNullOrWhiteSpace(industry) && Allowed.Contains(industry);
    }
}
