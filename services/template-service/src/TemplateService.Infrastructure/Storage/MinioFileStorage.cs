using Minio;
using TemplateService.Domain.Interfaces;

namespace TemplateService.Infrastructure.Storage;

/// <summary>
/// MinIO implementation of file storage
/// </summary>
public class MinioFileStorage : IFileStorage
{
    private readonly IMinioClient _minioClient;
    private const string BucketName = "techbirdsfly-storage";

    public MinioFileStorage(IMinioClient minioClient)
    {
        _minioClient = minioClient;
    }

    public async Task<string> UploadStreamAsync(string path, Stream stream, string contentType)
    {
        try
        {
            var bucketExists = await _minioClient.BucketExistsAsync(new BucketExistsArgs().WithBucket(BucketName));
            if (!bucketExists)
            {
                await _minioClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(BucketName));
            }

            await _minioClient.PutObjectAsync(new PutObjectArgs()
                .WithBucket(BucketName)
                .WithObject(path)
                .WithStreamData(stream)
                .WithObjectSize(stream.Length)
                .WithContentType(contentType));

            return $"{BucketName}/{path}";
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to upload file to MinIO: {ex.Message}", ex);
        }
    }

    public async Task<string> UploadTextAsync(string path, string content)
    {
        using var memoryStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
        return await UploadStreamAsync(path, memoryStream, "text/plain");
    }
}
