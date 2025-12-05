using MediatR;
using TemplateService.Application.Commands;
using TemplateService.Domain.Interfaces;

namespace TemplateService.Application.Handlers;

/// <summary>
/// Handler for UploadPreviewImageCommand
/// </summary>
public class UploadPreviewImageHandler : IRequestHandler<UploadPreviewImageCommand, string>
{
    private readonly IFileStorage _fileStorage;
    private readonly ITemplateRepository _repository;

    public UploadPreviewImageHandler(IFileStorage fileStorage, ITemplateRepository repository)
    {
        _fileStorage = fileStorage;
        _repository = repository;
    }

    public async Task<string> Handle(UploadPreviewImageCommand command, CancellationToken cancellationToken)
    {
        var path = $"templates/{command.TemplateId}/preview.png";
        var url = await _fileStorage.UploadStreamAsync(path, command.File, "image/png");

        await _repository.UpdatePreviewUrlAsync(command.TemplateId, url);

        return url;
    }
}
