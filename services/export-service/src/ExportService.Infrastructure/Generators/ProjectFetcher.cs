using ExportService.Application.Interfaces;
using ExportService.Application.Models;
using Microsoft.Extensions.Logging;

namespace ExportService.Infrastructure.Generators;

/// <summary>
/// Fetches project information from the GeneratorService API
/// </summary>
public class ProjectFetcher : IProjectFetcher
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ProjectFetcher> _logger;

    public ProjectFetcher(HttpClient httpClient, ILogger<ProjectFetcher> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Fetches project from GeneratorService
    /// Falls back to mock data for testing if service unavailable
    /// </summary>
    public async Task<ProjectDto> GetProjectAsync(string projectId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(projectId))
            throw new ArgumentException("Project ID cannot be empty", nameof(projectId));

        try
        {
            _logger.LogInformation("Fetching project {ProjectId} from GeneratorService", projectId);

            // Try to fetch from GeneratorService
            var response = await _httpClient.GetAsync(
                $"http://generator-service:5003/api/projects/{projectId}",
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                var project = System.Text.Json.JsonSerializer.Deserialize<ProjectDto>(json);
                _logger.LogInformation("Successfully fetched project {ProjectId}", projectId);
                return project ?? throw new InvalidOperationException("Invalid project data");
            }

            _logger.LogWarning("GeneratorService returned {StatusCode} for project {ProjectId}",
                response.StatusCode, projectId);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Failed to reach GeneratorService for project {ProjectId}. Using mock data.", projectId);
        }

        // Return mock project for development/testing
        return GetMockProject(projectId);
    }

    /// <summary>
    /// Returns mock project data for development/testing
    /// </summary>
    private ProjectDto GetMockProject(string projectId) =>
        new()
        {
            Id = projectId,
            Name = $"Project {projectId}",
            Description = "Mock project for testing export service",
            Html = @"
<div class=""hero"">
    <h1>Welcome to Your Website</h1>
    <p>This is a generated website from TechBirdsFly</p>
    <button class=""cta"">Get Started</button>
</div>
",
            Css = @"
* {
    margin: 0;
    padding: 0;
    box-sizing: border-box;
}

body {
    font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, sans-serif;
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    min-height: 100vh;
    display: flex;
    align-items: center;
    justify-content: center;
}

.hero {
    text-align: center;
    color: white;
    animation: fadeIn 0.5s ease-in;
}

.hero h1 {
    font-size: 3rem;
    margin-bottom: 1rem;
}

.hero p {
    font-size: 1.2rem;
    margin-bottom: 2rem;
    opacity: 0.9;
}

.cta {
    background: white;
    color: #667eea;
    border: none;
    padding: 12px 32px;
    font-size: 1rem;
    border-radius: 50px;
    cursor: pointer;
    transition: all 0.3s ease;
    font-weight: 600;
}

.cta:hover {
    transform: scale(1.05);
    box-shadow: 0 10px 25px rgba(0, 0, 0, 0.2);
}

@keyframes fadeIn {
    from {
        opacity: 0;
        transform: translateY(20px);
    }
    to {
        opacity: 1;
        transform: translateY(0);
    }
}
",
            Json = "{\"components\": [], \"pages\": []}",
            Components = new Dictionary<string, object>()
        };
}
