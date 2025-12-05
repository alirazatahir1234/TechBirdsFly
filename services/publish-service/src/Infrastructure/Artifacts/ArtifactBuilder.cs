using PublishService.Domain.Interfaces;
using System.IO.Compression;

namespace PublishService.Infrastructure.Artifacts;

/// <summary>
/// Builds static site artifacts from HTML
/// </summary>
public class ArtifactBuilder : IArtifactBuilder
{
    public async Task<string> BuildStaticSiteAsync(string html)
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "site_" + Guid.NewGuid());
        Directory.CreateDirectory(tempDir);

        // Write main HTML file
        await File.WriteAllTextAsync(Path.Combine(tempDir, "index.html"), html);

        // Add default stylesheet
        var css = """
            /* TechBirdsFly Generated Site */
            * { margin: 0; padding: 0; box-sizing: border-box; }
            body { font-family: system-ui, -apple-system, sans-serif; line-height: 1.6; color: #333; }
            html { scroll-behavior: smooth; }
            """;
        await File.WriteAllTextAsync(Path.Combine(tempDir, "styles.css"), css);

        // Add default script
        var js = """
            // TechBirdsFly Auto-Generated
            console.log('Site loaded successfully');
            """;
        await File.WriteAllTextAsync(Path.Combine(tempDir, "script.js"), js);

        return tempDir;
    }

    public Task<byte[]> BuildZipAsync(string folderPath)
    {
        using var ms = new MemoryStream();
        ZipFile.CreateFromDirectory(folderPath, ms);
        ms.Position = 0;
        return Task.FromResult(ms.ToArray());
    }
}
