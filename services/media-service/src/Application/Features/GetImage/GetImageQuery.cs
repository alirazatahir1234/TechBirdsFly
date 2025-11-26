using MediatR;

namespace TechBirdsFly.MediaService.Application.Features.GetImage;

public record GetImageQuery(Guid Id) : IRequest<GetImageResponse>;

public record GetImageResponse(
    Guid Id,
    string FileName,
    string Url,
    string MimeType,
    long Size,
    string? GeneratedFrom,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
