using ExportService.Application.Interfaces;
using ExportService.Application.Models;
using ExportService.Domain.Entities;

namespace ExportService.Application.Services;

/// <summary>
/// Orchestrates the code export process
/// Coordinates between ProjectFetcher, CodeGenerator, and FileStorage
/// </summary>
public class ExportApplicationService : IExportService
{
    private readonly IProjectFetcher _projectFetcher;
    private readonly ICodeGenerator _codeGenerator;
    private readonly IFileStorage _fileStorage;

    public ExportApplicationService(
        IProjectFetcher projectFetcher,
        ICodeGenerator codeGenerator,
        IFileStorage fileStorage)
    {
        _projectFetcher = projectFetcher ?? throw new ArgumentNullException(nameof(projectFetcher));
        _codeGenerator = codeGenerator ?? throw new ArgumentNullException(nameof(codeGenerator));
        _fileStorage = fileStorage ?? throw new ArgumentNullException(nameof(fileStorage));
    }

    /// <summary>
    /// Generates a code export for the specified project
    /// </summary>
    public async Task<ExportResult> GenerateExportAsync(
        string projectId,
        string framework,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(projectId))
            throw new ArgumentException("Project ID cannot be empty", nameof(projectId));

        if (string.IsNullOrWhiteSpace(framework))
            throw new ArgumentException("Framework cannot be empty", nameof(framework));

        try
        {
            // Step 1: Fetch project from GeneratorService
            var project = await _projectFetcher.GetProjectAsync(projectId, cancellationToken);

            if (project == null)
                throw new InvalidOperationException($"Project {projectId} not found");

            // Step 2: Generate code in target framework
            var zipBytes = await _codeGenerator.GenerateAsync(project, framework, cancellationToken);

            if (zipBytes == null || zipBytes.Length == 0)
                throw new InvalidOperationException("Code generation produced empty output");

            // Step 3: Save zip to storage
            var (filePath, downloadUrl) = await _fileStorage.SaveAsync(projectId, zipBytes, cancellationToken);

            // Step 4: Create export record
            var exportId = Guid.NewGuid();
            var result = new ExportResult
            {
                ExportId = exportId,
                ProjectId = projectId,
                Framework = framework,
                DownloadUrl = downloadUrl,
                FileSize = zipBytes.Length,
                CreatedAt = DateTime.UtcNow
            };

            return result;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Export failed for project {projectId}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Retrieves a previously generated export
    /// </summary>
    public Task<ExportResult?> GetExportAsync(
        string projectId,
        string framework,
        CancellationToken cancellationToken = default)
    {
        // TODO: Implement database query to retrieve export history
        // This would check if an export already exists for this projectId + framework combination
        return Task.FromResult<ExportResult?>(null);
    }
}
