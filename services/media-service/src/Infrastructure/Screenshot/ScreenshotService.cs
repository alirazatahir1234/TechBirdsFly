using Microsoft.Playwright;
using TechBirdsFly.MediaService.Domain.Interfaces;

namespace TechBirdsFly.MediaService.Infrastructure.Screenshot;

public class ScreenshotService : IScreenshotService
{
    private readonly ILogger<ScreenshotService> _logger;

    public ScreenshotService(ILogger<ScreenshotService> logger)
    {
        _logger = logger;
    }

    public async Task<byte[]> CaptureAsync(string html)
    {
        try
        {
            _logger.LogInformation("Starting screenshot capture");

            using var playwright = await Playwright.CreateAsync();

            var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true,
                Args = new[] { "--no-sandbox", "--disable-setuid-sandbox" }
            });

            var page = await browser.NewPageAsync(new BrowserNewPageOptions
            {
                ViewportSize = new ViewportSize { Width = 1280, Height = 720 }
            });

            // Set content with timeout
            await page.SetContentAsync(html, new PageSetContentOptions
            {
                WaitUntil = WaitUntilState.NetworkIdle,
                Timeout = 30000
            });

            // Wait for any pending animations
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            var screenshot = await page.ScreenshotAsync(new PageScreenshotOptions
            {
                FullPage = false,
                Type = ScreenshotType.Png,
                Timeout = 30000
            });

            await browser.CloseAsync();

            _logger.LogInformation("Screenshot captured successfully: {size} bytes", screenshot.Length);

            return screenshot;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to capture screenshot");
            throw;
        }
    }
}
