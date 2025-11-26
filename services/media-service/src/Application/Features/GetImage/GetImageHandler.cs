using MediatR;
using TechBirdsFly.MediaService.Domain.Exceptions;
using TechBirdsFly.MediaService.Domain.Interfaces;

namespace TechBirdsFly.MediaService.Application.Features.GetImage;

public class GetImageHandler : IRequestHandler<GetImageQuery, GetImageResponse>
{
    private readonly IMediaRepository _repo;
    private readonly ILogger<GetImageHandler> _logger;

    public GetImageHandler(IMediaRepository repo, ILogger<GetImageHandler> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<GetImageResponse> Handle(GetImageQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching image with ID: {Id}", request.Id);

        var file = await _repo.GetByIdAsync(request.Id);
        if (file == null)
            throw new MediaNotFoundException(request.Id);

        return new GetImageResponse(
            file.Id,
            file.FileName,
            file.Url,
            file.MimeType,
            file.Size,
            file.GeneratedFrom,
            file.CreatedAt,
            file.UpdatedAt ?? file.CreatedAt
        );
    }
}
