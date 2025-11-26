using MediatR;

namespace TechBirdsFly.MediaService.Application.Features.UploadImage;

public record UploadImageCommand(
    Stream FileStream,
    string FileName,
    string MimeType,
    long Size
) : IRequest<Guid>;
