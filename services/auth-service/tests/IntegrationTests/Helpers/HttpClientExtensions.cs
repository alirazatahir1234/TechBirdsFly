using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace AuthService.IntegrationTests.Helpers;

/// <summary>
/// Extension methods for HttpClient to simplify response reading
/// </summary>
public static class HttpClientExtensions
{
    /// <summary>
    /// Read response as strongly-typed object using JSON deserialization
    /// </summary>
    /// <typeparam name="T">The type to deserialize to</typeparam>
    /// <param name="response">The HTTP response message</param>
    /// <returns>Deserialized object or null if response body is empty</returns>
    public static async Task<T?> ReadAsAsync<T>(this HttpResponseMessage response)
    {
        return await response.Content.ReadFromJsonAsync<T>();
    }
}
