using TechBirdsFly.MediaService.Domain.Interfaces;

namespace TechBirdsFly.MediaService.Infrastructure.Storage;

public class LocalStorageService : IFileStorageService
{
    private readonly string _storagePath;
    private readonly ILogger<LocalStorageService> _logger;

    public LocalStorageService(IConfiguration config, ILogger<LocalStorageService> logger)
    {
        _storagePath = config["Storage:LocalPath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "uploads");
        _logger = logger;

        if (!Directory.Exists(_storagePath))
            Directory.CreateDirectory(_storagePath);
    }

    public async Task<string> SaveAsync(Stream fileStream, string fileName, string mimeType)
    {
        _logger.LogInformation("Saving file to local storage: {FileName}", fileName);

        var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
        var filePath = Path.Combine(_storagePath, uniqueFileName);

        using (var file = new FileStream(filePath, FileMode.Create, FileAccess.Write))
        {
            await fileStream.CopyToAsync(file);
        }

        var relativeUrl = $"/uploads/{uniqueFileName}";
        _logger.LogInformation("File saved successfully at: {Url}", relativeUrl);

        return relativeUrl;
    }

    public async Task<bool> DeleteAsync(string fileUrl)
    {
        _logger.LogInformation("Deleting file: {Url}", fileUrl);

        try
        {
            var fileName = Path.GetFileName(fileUrl);
            var filePath = Path.Combine(_storagePath, fileName);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                _logger.LogInformation("File deleted successfully: {Url}", fileUrl);
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file: {Url}", fileUrl);
            return false;
        }
    }

    public async Task<Stream?> GetAsync(string fileUrl)
    {
        _logger.LogInformation("Retrieving file: {Url}", fileUrl);

        var fileName = Path.GetFileName(fileUrl);
        var filePath = Path.Combine(_storagePath, fileName);

        if (!File.Exists(filePath))
        {
            _logger.LogWarning("File not found: {Url}", fileUrl);
            return null;
        }

        return File.OpenRead(filePath);
    }
}
