using MediatR;
using TechBirdsFly.MediaService.Domain.Entities;
using TechBirdsFly.MediaService.Domain.Interfaces;

namespace TechBirdsFly.MediaService.Application.Features.UploadImage;

public class UploadImageHandler : IRequestHandler<UploadImageCommand, Guid>
{
    private readonly IFileStorageService _storage;
    private readonly IMediaRepository _repo;
    private readonly ILogger<UploadImageHandler> _logger;

    public UploadImageHandler(IFileStorageService storage, IMediaRepository repo, ILogger<UploadImageHandler> logger)
    {
        _storage = storage;
        _repo = repo;
        _logger = logger;
    }

    public async Task<Guid> Handle(UploadImageCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Uploading image: {FileName}", request.FileName);

        var url = await _storage.SaveAsync(request.FileStream, request.FileName, request.MimeType);

        var file = new MediaFile(request.FileName, url, request.MimeType, request.Size);

        await _repo.AddAsync(file);
        await _repo.SaveChangesAsync();

        _logger.LogInformation("Image uploaded successfully with ID: {Id}", file.Id);
        return file.Id;
    }
}
