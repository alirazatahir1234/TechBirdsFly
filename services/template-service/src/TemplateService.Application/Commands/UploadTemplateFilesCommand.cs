using MediatR;

namespace TemplateService.Application.Commands;

/// <summary>
/// CQRS command to upload template files
/// </summary>
public record UploadTemplateFilesCommand(
    Guid TemplateId,
    Dictionary<string, string> Files
) : IRequest<bool>;
