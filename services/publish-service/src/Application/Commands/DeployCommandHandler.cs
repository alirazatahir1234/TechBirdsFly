using MediatR;
using PublishService.Application.Commands;
using PublishService.Application.DTOs;
using PublishService.Domain.Entities;
using PublishService.Domain.Interfaces;
using System.IO;

namespace PublishService.Application.Handlers;

/// <summary>
/// Handler for deploying websites
/// </summary>
public class DeployCommandHandler : IRequestHandler<DeployCommand, string>
{
    private readonly IArtifactBuilder _builder;
    private readonly IVercelDeployer _vercel;
    private readonly INetlifyDeployer _netlify;
    private readonly IStaticStorage _storage;
    private readonly IPublishRepository _repo;

    public DeployCommandHandler(
        IArtifactBuilder builder,
        IVercelDeployer vercel,
        INetlifyDeployer netlify,
        IStaticStorage storage,
        IPublishRepository repo)
    {
        _builder = builder;
        _vercel = vercel;
        _netlify = netlify;
        _storage = storage;
        _repo = repo;
    }

    public async Task<string> Handle(DeployCommand cmd, CancellationToken ct)
    {
        var req = cmd.Request;
        var record = new PublishRecord(req.ProjectId, req.UserId, req.Provider);

        await _repo.AddAsync(record);
        await _repo.SaveChangesAsync();

        try
        {
            record.MarkInProgress();

            // 1. Build static site from HTML
            var folder = await _builder.BuildStaticSiteAsync(req.Html);

            string publishedUrl = "";

            // 2. Deploy based on provider
            switch (req.Provider.ToLower())
            {
                case "vercel":
                    publishedUrl = await _vercel.DeployAsync(folder, req.Token);
                    break;

                case "netlify":
                    var zipBytes = await _builder.BuildZipAsync(folder);
                    publishedUrl = await _netlify.DeployZipAsync(zipBytes, req.Token);
                    break;

                case "techbirdsfly":
                    publishedUrl = await _storage.UploadStaticSiteAsync(req.ProjectId, folder);
                    break;

                default:
                    throw new InvalidOperationException($"Invalid provider: {req.Provider}");
            }

            // 3. Mark as success
            record.MarkSuccess(publishedUrl);
            await _repo.UpdateAsync(record);
            await _repo.SaveChangesAsync();

            return publishedUrl;
        }
        catch (Exception ex)
        {
            record.MarkFailed(ex.Message);
            await _repo.UpdateAsync(record);
            await _repo.SaveChangesAsync();
            throw;
        }
    }
}
