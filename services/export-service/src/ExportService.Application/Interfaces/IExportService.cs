using ExportService.Application.Models;

namespace ExportService.Application.Interfaces;

/// <summary>
/// Fetches project data from the GeneratorService
/// </summary>
public interface IProjectFetcher
{
    Task<ProjectDto> GetProjectAsync(string projectId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Generates code in specified framework format
/// </summary>
public interface ICodeGenerator
{
    Task<byte[]> GenerateAsync(ProjectDto project, string framework, CancellationToken cancellationToken = default);
}

/// <summary>
/// Stores generated code files
/// </summary>
public interface IFileStorage
{
    Task<(string FilePath, string DownloadUrl)> SaveAsync(string projectId, byte[] zipData, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string projectId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Main export service orchestrating code generation
/// </summary>
public interface IExportService
{
    Task<ExportResult> GenerateExportAsync(string projectId, string framework, Guid userId, CancellationToken cancellationToken = default);
    Task<ExportResult?> GetExportAsync(string projectId, string framework, CancellationToken cancellationToken = default);
}
