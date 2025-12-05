namespace TemplateService.Domain.Interfaces;

/// <summary>
/// Interface for file storage operations (MinIO)
/// </summary>
public interface IFileStorage
{
    /// <summary>
    /// Uploads a stream to MinIO storage
    /// </summary>
    Task<string> UploadStreamAsync(string path, Stream stream, string contentType);

    /// <summary>
    /// Uploads text content to MinIO storage
    /// </summary>
    Task<string> UploadTextAsync(string path, string content);
}
