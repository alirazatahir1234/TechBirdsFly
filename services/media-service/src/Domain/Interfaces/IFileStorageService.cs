namespace TechBirdsFly.MediaService.Domain.Interfaces;

public interface IFileStorageService
{
    Task<string> SaveAsync(Stream fileStream, string fileName, string contentType);
    Task<bool> DeleteAsync(string fileName);
    Task<Stream?> GetAsync(string fileName);
}
