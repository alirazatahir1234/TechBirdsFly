namespace GeneratorService.Domain.ValueObjects;

/// <summary>
/// Metadata value object for SEO and page metadata
/// Immutable representation of page title, description, and keywords
/// </summary>
public record Metadata(string Title, string Description, string Keywords)
{
    public static Metadata Create(string title, string description, string keywords)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty", nameof(title));
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be empty", nameof(description));
        if (string.IsNullOrWhiteSpace(keywords))
            throw new ArgumentException("Keywords cannot be empty", nameof(keywords));

        return new(title, description, keywords);
    }

    public override string ToString() => $"Title: {Title}, Description: {Description}";
}
