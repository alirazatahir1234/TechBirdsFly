namespace TechBirdsFly.MediaService.Domain.Interfaces;

/// <summary>
/// Service for capturing HTML screenshots using Playwright
/// </summary>
public interface IScreenshotService
{
    /// <summary>
    /// Captures a screenshot of the provided HTML content
    /// </summary>
    /// <param name="html">The HTML content to capture</param>
    /// <returns>PNG image bytes</returns>
    Task<byte[]> CaptureAsync(string html);
}
