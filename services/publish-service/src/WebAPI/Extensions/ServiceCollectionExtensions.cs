using Microsoft.EntityFrameworkCore;
using MediatR;
using PublishService.Application.Handlers;
using PublishService.Domain.Interfaces;
using PublishService.Infrastructure.Artifacts;
using PublishService.Infrastructure.Data;
using PublishService.Infrastructure.Deploy;
using PublishService.Infrastructure.Storage;

namespace PublishService.WebAPI.Extensions;

/// <summary>
/// Dependency injection extensions
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPublishServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection not configured");

        services.AddDbContext<PublishDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Repository
        services.AddScoped<IPublishRepository, PublishRepository>();

        // Domain services
        services.AddScoped<IArtifactBuilder, ArtifactBuilder>();
        services.AddScoped<IStaticStorage, StaticStorage>(sp =>
            new StaticStorage(configuration["StaticStorage:BasePath"] ?? "/var/www/techbirdsfly-sites"));

        // HTTP clients for external services
        services.AddHttpClient<IVercelDeployer, VercelDeployer>();
        services.AddHttpClient<INetlifyDeployer, NetlifyDeployer>();

        // MediatR
        services.AddMediatR(typeof(ServiceCollectionExtensions).Assembly);

        return services;
    }
}
