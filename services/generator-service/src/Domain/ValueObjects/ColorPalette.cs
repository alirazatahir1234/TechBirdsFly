namespace GeneratorService.Domain.ValueObjects;

/// <summary>
/// ColorPalette value object for website color schemes
/// Immutable representation of primary, secondary, and accent colors
/// </summary>
public record ColorPalette(string Primary, string Secondary, string Accent)
{
    public static ColorPalette Default => new("#0066CC", "#00D4FF", "#FF6B35");

    public static ColorPalette Create(string primary, string secondary, string accent)
    {
        if (!IsValidColor(primary))
            throw new ArgumentException("Invalid primary color", nameof(primary));
        if (!IsValidColor(secondary))
            throw new ArgumentException("Invalid secondary color", nameof(secondary));
        if (!IsValidColor(accent))
            throw new ArgumentException("Invalid accent color", nameof(accent));

        return new(primary, secondary, accent);
    }

    private static bool IsValidColor(string color)
    {
        // Simple validation for hex colors
        if (string.IsNullOrWhiteSpace(color))
            return false;

        if (color.StartsWith("#") && (color.Length == 7 || color.Length == 4))
            return System.Text.RegularExpressions.Regex.IsMatch(color, @"^#[0-9A-Fa-f]{3}$|^#[0-9A-Fa-f]{6}$");

        // Allow CSS color names
        return System.Drawing.Color.FromName(color).IsKnownColor;
    }

    public override string ToString() => $"Primary: {Primary}, Secondary: {Secondary}, Accent: {Accent}";
}
