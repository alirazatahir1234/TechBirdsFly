using PublishService.Domain.Interfaces;

namespace PublishService.Infrastructure.Storage;

/// <summary>
/// Stores static sites on TechBirdsFly CDN (local file system or Azure Blob)
/// </summary>
public class StaticStorage : IStaticStorage
{
    private readonly string _basePath;

    public StaticStorage(string basePath = "/var/www/techbirdsfly-sites")
    {
        _basePath = basePath;
    }

    public Task<string> UploadStaticSiteAsync(Guid projectId, string folderPath)
    {
        var projectDir = Path.Combine(_basePath, projectId.ToString());

        // Clear old deployment
        if (Directory.Exists(projectDir))
            Directory.Delete(projectDir, true);

        // Create new directory
        Directory.CreateDirectory(projectDir);

        // Copy all files
        CopyDirectoryRecursive(new DirectoryInfo(folderPath), new DirectoryInfo(projectDir));

        // Return public URL
        return Task.FromResult($"https://sites.techbirdsfly.app/{projectId}");
    }

    private void CopyDirectoryRecursive(DirectoryInfo source, DirectoryInfo target)
    {
        Directory.CreateDirectory(target.FullName);

        foreach (var file in source.GetFiles())
        {
            file.CopyTo(Path.Combine(target.FullName, file.Name), true);
        }

        foreach (var dir in source.GetDirectories())
        {
            var newTarget = target.CreateSubdirectory(dir.Name);
            CopyDirectoryRecursive(dir, newTarget);
        }
    }
}
