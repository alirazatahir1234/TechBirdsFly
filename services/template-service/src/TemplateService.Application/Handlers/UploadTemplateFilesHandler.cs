using MediatR;
using TemplateService.Application.Commands;
using TemplateService.Domain.Entities;
using TemplateService.Domain.Interfaces;

namespace TemplateService.Application.Handlers;

/// <summary>
/// Handler for UploadTemplateFilesCommand
/// </summary>
public class UploadTemplateFilesHandler : IRequestHandler<UploadTemplateFilesCommand, bool>
{
    private readonly IFileStorage _fileStorage;
    private readonly ITemplateRepository _repository;

    public UploadTemplateFilesHandler(IFileStorage fileStorage, ITemplateRepository repository)
    {
        _fileStorage = fileStorage;
        _repository = repository;
    }

    public async Task<bool> Handle(UploadTemplateFilesCommand command, CancellationToken cancellationToken)
    {
        foreach (var file in command.Files)
        {
            var minioPath = $"templates/{command.TemplateId}/{file.Key}";
            var url = await _fileStorage.UploadTextAsync(minioPath, file.Value);

            var templateFile = new TemplateFile
            {
                TemplateId = command.TemplateId,
                Path = url,
                Format = DetectFormat(file.Key)
            };

            await _repository.AddFileAsync(templateFile);
        }

        return true;
    }

    private string DetectFormat(string filename)
    {
        if (filename.EndsWith(".html", StringComparison.OrdinalIgnoreCase)) return "html";
        if (filename.EndsWith(".tsx", StringComparison.OrdinalIgnoreCase)) return "next";
        if (filename.EndsWith(".jsx", StringComparison.OrdinalIgnoreCase)) return "react";
        if (filename.EndsWith(".json", StringComparison.OrdinalIgnoreCase)) return "json";
        return "json";
    }
}
