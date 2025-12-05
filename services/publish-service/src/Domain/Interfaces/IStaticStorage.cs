namespace PublishService.Domain.Interfaces;

/// <summary>
/// Static site storage (TechBirdsFly CDN)
/// </summary>
public interface IStaticStorage
{
    Task<string> UploadStaticSiteAsync(Guid projectId, string folderPath);
}
