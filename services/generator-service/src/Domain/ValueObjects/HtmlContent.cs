namespace GeneratorService.Domain.ValueObjects;

/// <summary>
/// HtmlContent value object represents HTML content
/// Immutable, strongly typed wrapper for HTML strings
/// </summary>
public record HtmlContent(string Value)
{
    public static HtmlContent Empty => new(string.Empty);

    public static HtmlContent Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("HTML content cannot be empty", nameof(value));

        return new(value);
    }

    public override string ToString() => Value;

    public int Length => Value.Length;

    public bool IsEmpty => string.IsNullOrWhiteSpace(Value);
}
