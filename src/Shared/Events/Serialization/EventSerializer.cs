using System.Text.Json;

namespace TechBirdsFly.Shared.Events.Serialization;

/// <summary>
/// Event serialization utilities for JSON and Avro formats
/// </summary>
public static class EventSerializer
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Serialize event to JSON
    /// </summary>
    public static string SerializeToJson<T>(T @event) where T : class
    {
        if (@event == null)
            throw new ArgumentNullException(nameof(@event));

        return JsonSerializer.Serialize(@event, JsonOptions);
    }

    /// <summary>
    /// Deserialize event from JSON
    /// </summary>
    public static T? DeserializeFromJson<T>(string json) where T : class
    {
        if (string.IsNullOrWhiteSpace(json))
            throw new ArgumentNullException(nameof(json));

        return JsonSerializer.Deserialize<T>(json, JsonOptions);
    }

    /// <summary>
    /// Serialize event to JSON with formatting
    /// </summary>
    public static string SerializeToJsonPretty<T>(T @event) where T : class
    {
        if (@event == null)
            throw new ArgumentNullException(nameof(@event));

        var options = new JsonSerializerOptions(JsonOptions) { WriteIndented = true };
        return JsonSerializer.Serialize(@event, options);
    }

    /// <summary>
    /// Convert object to dictionary
    /// </summary>
    public static Dictionary<string, object?> ObjectToDictionary<T>(T @object) where T : class
    {
        if (@object == null)
            return new Dictionary<string, object?>();

        var json = SerializeToJson(@object);
        var dict = JsonSerializer.Deserialize<Dictionary<string, object?>>(json, JsonOptions);
        return dict ?? new Dictionary<string, object?>();
    }

    /// <summary>
    /// Convert dictionary to object
    /// </summary>
    public static T? DictionaryToObject<T>(Dictionary<string, object?> dictionary) where T : class
    {
        if (dictionary == null)
            return null;

        var json = JsonSerializer.Serialize(dictionary, JsonOptions);
        return DeserializeFromJson<T>(json);
    }
}
