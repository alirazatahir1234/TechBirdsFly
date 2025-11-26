using TechBirdsFly.MediaService.Domain.Common;

namespace TechBirdsFly.MediaService.Domain.Entities;

/// <summary>
/// Represents a media file (image, video, etc.) in the system
/// </summary>
public class MediaFile : BaseEntity
{
    public string FileName { get; private set; }
    public string Url { get; private set; }
    public string MimeType { get; private set; }
    public long Size { get; private set; }
    public string? GeneratedFrom { get; private set; } // Prompt if AI-generated

    public MediaFile(string fileName, string url, string mimeType, long size, string? generatedFrom = null)
    {
        FileName = fileName;
        Url = url;
        MimeType = mimeType;
        Size = size;
        GeneratedFrom = generatedFrom;
    }

    public void UpdateUrl(string url)
    {
        Url = url;
        UpdatedAt = DateTime.UtcNow;
    }
}
