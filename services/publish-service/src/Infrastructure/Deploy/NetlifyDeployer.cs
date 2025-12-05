using PublishService.Domain.Interfaces;
using System.Net.Http.Json;
using System.Text.Json;

namespace PublishService.Infrastructure.Deploy;

/// <summary>
/// Deploys to Netlify
/// </summary>
public class NetlifyDeployer : INetlifyDeployer
{
    private readonly HttpClient _http;
    private const string NetlifyApiUrl = "https://api.netlify.com/api/v1/sites";

    public NetlifyDeployer(HttpClient http)
    {
        _http = http;
    }

    public async Task<string> DeployZipAsync(byte[] zipBytes, string netlifyToken)
    {
        if (string.IsNullOrWhiteSpace(netlifyToken))
            throw new ArgumentException("Netlify token is required");

        var req = new HttpRequestMessage(HttpMethod.Post, NetlifyApiUrl);
        req.Headers.Add("Authorization", $"Bearer {netlifyToken}");
        req.Content = new ByteArrayContent(zipBytes);
        req.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/zip");

        var res = await _http.SendAsync(req);

        if (!res.IsSuccessStatusCode)
        {
            var errContent = await res.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Netlify deployment failed: {errContent}");
        }

        var json = JsonSerializer.Deserialize<JsonElement>(await res.Content.ReadAsStringAsync());
        var url = json.GetProperty("ssl_url").GetString();

        if (string.IsNullOrEmpty(url))
            throw new InvalidOperationException("Netlify did not return a deployment URL");

        return url;
    }
}
