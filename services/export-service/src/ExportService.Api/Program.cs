using ExportService.Application.Interfaces;
using ExportService.Application.Models;
using ExportService.Application.Services;
using ExportService.Infrastructure.Generators;
using ExportService.Infrastructure.Storage;
using System.Net.Http.Json;

var builder = WebApplication.CreateBuilder(args);

// Add logging
builder.Services.AddLogging(config =>
{
    config.ClearProviders();
    config.AddConsole();
    config.AddDebug();
});

// Register application services
builder.Services.AddScoped<IExportService, ExportApplicationService>();

// Register infrastructure services
builder.Services.AddScoped<IProjectFetcher, ProjectFetcher>();
builder.Services.AddScoped<IFileStorage, LocalFileStorage>();
// Uncomment for Azure Blob Storage: builder.Services.AddScoped<IFileStorage, AzureBlobStorage>();

// Register HTTP client for ProjectFetcher with timeout
builder.Services.AddHttpClient<ProjectFetcher>()
    .ConfigureHttpClient(client =>
    {
        client.Timeout = TimeSpan.FromSeconds(30);
    });

// Add CORS for frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Add health checks
builder.Services.AddHealthChecks();

// Build the app
var app = builder.Build();

// Use CORS
app.UseCors("AllowFrontend");

// Health check endpoint
app.MapHealthChecks("/health");

// Export endpoints
app.MapPost("/api/export/{projectId}/{framework}", ExportEndpoints.GenerateExport)
    .WithName("GenerateExport")
    .WithOpenApi()
    .WithDescription("Generate code export for a project in specified framework");

app.MapGet("/api/export/{projectId}/{framework}", ExportEndpoints.GetExport)
    .WithName("GetExport")
    .WithOpenApi()
    .WithDescription("Retrieve previously generated export");

app.MapDelete("/api/export/{projectId}", ExportEndpoints.DeleteExports)
    .WithName("DeleteExports")
    .WithOpenApi()
    .WithDescription("Delete all exports for a project");

app.MapGet("/api/frameworks", ExportEndpoints.GetSupportedFrameworks)
    .WithName("GetFrameworks")
    .WithOpenApi()
    .WithDescription("Get list of supported frameworks");

// Serve exported files
app.UseStaticFiles();

app.Run();

/// <summary>
/// Export API endpoints
/// </summary>
public static class ExportEndpoints
{
    /// <summary>
    /// Generates code export for a project
    /// </summary>
    public static async Task<IResult> GenerateExport(
        string projectId,
        string framework,
        IExportService exportService,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger("ExportEndpoints");
        try
        {
            logger.LogInformation(
                "Export requested: ProjectId={ProjectId}, Framework={Framework}",
                projectId,
                framework);

            // Validate input
            if (string.IsNullOrWhiteSpace(projectId))
                return Results.BadRequest("Project ID is required");

            if (string.IsNullOrWhiteSpace(framework))
                return Results.BadRequest("Framework is required");

            var validFrameworks = new[] { "html", "react", "nextjs" };
            if (!validFrameworks.Contains(framework.ToLowerInvariant()))
                return Results.BadRequest($"Unsupported framework. Supported: {string.Join(", ", validFrameworks)}");

            // Generate export
            var result = await exportService.GenerateExportAsync(
                projectId,
                framework,
                Guid.NewGuid(), // TODO: Get actual user ID from JWT
                cancellationToken);

            logger.LogInformation(
                "Export completed: ProjectId={ProjectId}, Framework={Framework}, Size={FileSize}",
                projectId,
                framework,
                result.FileSize);

            return Results.Ok(result);
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning(ex, "Invalid export request");
            return Results.BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Export failed for ProjectId={ProjectId}, Framework={Framework}", projectId, framework);
            return Results.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Retrieves a previously generated export
    /// </summary>
    public static async Task<IResult> GetExport(
        string projectId,
        string framework,
        IExportService exportService,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger("ExportEndpoints");
        try
        {
            var result = await exportService.GetExportAsync(projectId, framework, cancellationToken);

            if (result == null)
                return Results.NotFound("Export not found");

            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve export");
            return Results.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Deletes all exports for a project
    /// </summary>
    public static async Task<IResult> DeleteExports(
        string projectId,
        IFileStorage fileStorage,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger("ExportEndpoints");
        try
        {
            var deleted = await fileStorage.DeleteAsync(projectId, cancellationToken);

            if (!deleted)
                return Results.NotFound("No exports found for deletion");

            return Results.Ok(new { message = "Exports deleted successfully" });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to delete exports");
            return Results.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Gets list of supported frameworks
    /// </summary>
    public static Task<IResult> GetSupportedFrameworks(ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger("ExportEndpoints");
        var frameworks = new[]
        {
            new { name = "html", description = "Plain HTML/CSS" },
            new { name = "react", description = "React JSX Components" },
            new { name = "nextjs", description = "Next.js App Router" }
        };

        return Task.FromResult(Results.Ok(frameworks));
    }
}
