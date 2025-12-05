using MediatR;

namespace TemplateService.Application.Commands;

/// <summary>
/// CQRS command to upload preview image to MinIO
/// </summary>
public record UploadPreviewImageCommand(
    Guid TemplateId,
    Stream File,
    string FileName
) : IRequest<string>;
