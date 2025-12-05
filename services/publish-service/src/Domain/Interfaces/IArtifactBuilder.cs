namespace PublishService.Domain.Interfaces;

/// <summary>
/// Builds static site artifacts from HTML
/// </summary>
public interface IArtifactBuilder
{
    Task<string> BuildStaticSiteAsync(string html);
    Task<byte[]> BuildZipAsync(string folderPath);
}
