using ExportService.Application.Interfaces;

namespace ExportService.Infrastructure.Storage;

/// <summary>
/// Stores exported code files to local file system
/// </summary>
public class LocalFileStorage : IFileStorage
{
    private readonly string _exportDirectory;
    private readonly ILogger<LocalFileStorage> _logger;

    public LocalFileStorage(IConfiguration configuration, ILogger<LocalFileStorage> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        // Read export directory from configuration, default to ./exports
        _exportDirectory = configuration["Storage:ExportDirectory"] ?? Path.Combine(Directory.GetCurrentDirectory(), "exports");
        
        // Create directory if it doesn't exist
        Directory.CreateDirectory(_exportDirectory);
        
        _logger.LogInformation("File storage initialized at: {ExportDirectory}", _exportDirectory);
    }

    /// <summary>
    /// Saves zip data to local file system and returns file path and download URL
    /// </summary>
    public Task<(string FilePath, string DownloadUrl)> SaveAsync(
        string projectId,
        byte[] zipData,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(projectId))
            throw new ArgumentException("Project ID cannot be empty", nameof(projectId));

        if (zipData == null || zipData.Length == 0)
            throw new ArgumentException("Zip data cannot be empty", nameof(zipData));

        try
        {
            // Create project-specific directory
            var projectDirectory = Path.Combine(_exportDirectory, projectId);
            Directory.CreateDirectory(projectDirectory);

            // Generate unique filename with timestamp
            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var fileName = $"website_{timestamp}.zip";
            var filePath = Path.Combine(projectDirectory, fileName);

            // Write zip file to disk
            File.WriteAllBytes(filePath, zipData);

            _logger.LogInformation(
                "Saved export for project {ProjectId} to {FilePath} ({FileSize} bytes)",
                projectId,
                filePath,
                zipData.Length);

            // Generate relative download URL
            var downloadUrl = $"/exports/{projectId}/{fileName}";

            return Task.FromResult((filePath, downloadUrl));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save export for project {ProjectId}", projectId);
            throw;
        }
    }

    /// <summary>
    /// Deletes all exports for a specific project
    /// </summary>
    public Task<bool> DeleteAsync(string projectId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(projectId))
            throw new ArgumentException("Project ID cannot be empty", nameof(projectId));

        try
        {
            var projectDirectory = Path.Combine(_exportDirectory, projectId);
            
            if (!Directory.Exists(projectDirectory))
            {
                _logger.LogWarning("Project directory not found: {ProjectDirectory}", projectDirectory);
                return Task.FromResult(false);
            }

            Directory.Delete(projectDirectory, recursive: true);
            
            _logger.LogInformation("Deleted exports for project {ProjectId}", projectId);
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete exports for project {ProjectId}", projectId);
            throw;
        }
    }
}

/// <summary>
/// Stores exported code files to Azure Blob Storage
/// </summary>
public class AzureBlobStorage : IFileStorage
{
    private readonly string _containerName;
    private readonly ILogger<AzureBlobStorage> _logger;

    public AzureBlobStorage(IConfiguration configuration, ILogger<AzureBlobStorage> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _containerName = configuration["Storage:AzureContainer"] ?? "exports";
        
        _logger.LogInformation("Azure Blob Storage initialized with container: {ContainerName}", _containerName);
    }

    /// <summary>
    /// Saves zip data to Azure Blob Storage
    /// </summary>
    public Task<(string FilePath, string DownloadUrl)> SaveAsync(
        string projectId,
        byte[] zipData,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(projectId))
            throw new ArgumentException("Project ID cannot be empty", nameof(projectId));

        if (zipData == null || zipData.Length == 0)
            throw new ArgumentException("Zip data cannot be empty", nameof(zipData));

        try
        {
            // TODO: Implement Azure Blob Storage integration
            // Use Azure.Storage.Blobs NuGet package
            // Example:
            // var blobClient = new BlobClient(new Uri(blobUri), new DefaultAzureCredential());
            // await blobClient.UploadAsync(new BinaryData(zipData), overwrite: true);

            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var blobName = $"{projectId}/website_{timestamp}.zip";
            var downloadUrl = $"https://[storage-account].blob.core.windows.net/{_containerName}/{blobName}";

            _logger.LogInformation(
                "Saved export for project {ProjectId} to Azure Blob Storage ({FileSize} bytes)",
                projectId,
                zipData.Length);

            return Task.FromResult((blobName, downloadUrl));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save export to Azure Blob Storage for project {ProjectId}", projectId);
            throw;
        }
    }

    /// <summary>
    /// Deletes all exports for a specific project from Azure Blob Storage
    /// </summary>
    public Task<bool> DeleteAsync(string projectId, CancellationToken cancellationToken = default)
    {
        // TODO: Implement blob deletion logic
        _logger.LogWarning("Azure Blob deletion not yet implemented for project {ProjectId}", projectId);
        return Task.FromResult(false);
    }
}
