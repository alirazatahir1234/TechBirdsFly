using MediatR;
using TechBirdsFly.MediaService.Domain.Exceptions;
using TechBirdsFly.MediaService.Domain.Interfaces;

namespace TechBirdsFly.MediaService.Application.Features.DeleteImage;

public class DeleteImageHandler : IRequestHandler<DeleteImageCommand, bool>
{
    private readonly IMediaRepository _repo;
    private readonly IFileStorageService _storage;
    private readonly ILogger<DeleteImageHandler> _logger;

    public DeleteImageHandler(IMediaRepository repo, IFileStorageService storage, ILogger<DeleteImageHandler> logger)
    {
        _repo = repo;
        _storage = storage;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteImageCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting image with ID: {Id}", request.Id);

        var file = await _repo.GetByIdAsync(request.Id);
        if (file == null)
            throw new MediaNotFoundException(request.Id);

        if (!string.IsNullOrEmpty(file.Url))
        {
            await _storage.DeleteAsync(file.Url);
        }

        await _repo.DeleteAsync(file);
        await _repo.SaveChangesAsync();

        _logger.LogInformation("Image deleted successfully with ID: {Id}", request.Id);
        return true;
    }
}
