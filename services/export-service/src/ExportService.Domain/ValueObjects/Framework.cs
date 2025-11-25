namespace ExportService.Domain.ValueObjects;

/// <summary>
/// Value object representing supported code generation frameworks
/// </summary>
public class Framework : IEquatable<Framework>
{
    public static readonly Framework Html = new("html");
    public static readonly Framework React = new("react");
    public static readonly Framework NextJs = new("nextjs");

    public string Value { get; }

    private Framework(string value)
    {
        Value = value;
    }

    public static Framework Create(string value) =>
        value.ToLowerInvariant() switch
        {
            "html" => Html,
            "react" => React,
            "nextjs" => NextJs,
            _ => throw new ArgumentException($"Unsupported framework: {value}")
        };

    public bool Equals(Framework? other) =>
        other is not null && Value == other.Value;

    public override bool Equals(object? obj) =>
        Equals(obj as Framework);

    public override int GetHashCode() =>
        Value.GetHashCode();

    public override string ToString() =>
        Value;
}
