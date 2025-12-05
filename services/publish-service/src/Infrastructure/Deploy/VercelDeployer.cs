using PublishService.Domain.Interfaces;
using System.Net.Http.Json;
using System.Text.Json;

namespace PublishService.Infrastructure.Deploy;

/// <summary>
/// Deploys to Vercel
/// </summary>
public class VercelDeployer : IVercelDeployer
{
    private readonly HttpClient _http;
    private const string VercelApiUrl = "https://api.vercel.com/v13/deployments";

    public VercelDeployer(HttpClient http)
    {
        _http = http;
    }

    public async Task<string> DeployAsync(string folderPath, string vercelToken)
    {
        if (string.IsNullOrWhiteSpace(vercelToken))
            throw new ArgumentException("Vercel token is required");

        var files = Directory.GetFiles(folderPath);

        var filesPayload = new List<dynamic>();
        foreach (var file in files)
        {
            var content = await File.ReadAllBytesAsync(file);
            filesPayload.Add(new
            {
                file = Path.GetFileName(file),
                data = Convert.ToBase64String(content)
            });
        }

        var payload = new
        {
            files = filesPayload,
            name = "techbirdsfly-site-" + Guid.NewGuid().ToString().Substring(0, 8)
        };

        var req = new HttpRequestMessage(HttpMethod.Post, VercelApiUrl);
        req.Headers.Add("Authorization", $"Bearer {vercelToken}");
        req.Content = JsonContent.Create(payload);

        var res = await _http.SendAsync(req);

        if (!res.IsSuccessStatusCode)
        {
            var errContent = await res.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Vercel deployment failed: {errContent}");
        }

        var json = JsonSerializer.Deserialize<JsonElement>(await res.Content.ReadAsStringAsync());
        var url = json.GetProperty("url").GetString();

        if (string.IsNullOrEmpty(url))
            throw new InvalidOperationException("Vercel did not return a deployment URL");

        return $"https://{url}";
    }
}
