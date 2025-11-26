using Microsoft.EntityFrameworkCore;
using TechBirdsFly.MediaService.Domain.Interfaces;
using TechBirdsFly.MediaService.Infrastructure.AI;
using TechBirdsFly.MediaService.Infrastructure.Persistence;
using TechBirdsFly.MediaService.Infrastructure.Repositories;
using TechBirdsFly.MediaService.Infrastructure.Screenshot;
using TechBirdsFly.MediaService.Infrastructure.Storage;

namespace TechBirdsFly.MediaService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var connectionString = config.GetConnectionString("DefaultConnection");
        services.AddDbContext<MediaDbContext>(options =>
            options.UseNpgsql(connectionString, x => x.MigrationsAssembly("TechBirdsFly.MediaService")));

        services.AddScoped<IMediaRepository, MediaRepository>();
        services.AddScoped<IFileStorageService, LocalStorageService>();
        services.AddScoped<IScreenshotService, ScreenshotService>();
        services.AddHttpClient<IImageAIService, ImageAIService>();

        return services;
    }
}
