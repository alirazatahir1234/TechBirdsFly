using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ImageService.Application.Interfaces;
using ImageService.Application.Services;
using ImageService.Infrastructure.ExternalServices;
using ImageService.Infrastructure.Persistence;

namespace ImageService.Infrastructure;

/// <summary>
/// Dependency injection extension methods for infrastructure services
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, string connectionString)
    {
        // DbContext
        services.AddDbContext<ImageDbContext>(options =>
            options.UseSqlite(connectionString));

        // Repositories
        services.AddScoped<IImageRepository, ImageRepository>();
        services.AddScoped<IImageMetadataRepository, ImageMetadataRepository>();

        // Application Services
        services.AddScoped<IImageUploadService, ImageUploadService>();
        services.AddScoped<IImageGenerationService, ImageGenerationService>();
        services.AddScoped<IImageMetadataService, ImageMetadataService>();

        // External Services
        services.AddSingleton<IImageStorageService, ImageStorageService>();
        services.AddSingleton<IImageProcessingService, ImageProcessingService>();
        services.AddSingleton<IAIImageGenerationService, AIImageGenerationService>();
        services.AddSingleton<ICdnService, ImageCdnService>();
        services.AddSingleton<IEventPublisher, EventPublisher>();

        return services;
    }

    public static async Task InitializeDatabaseAsync(this IServiceProvider serviceProvider)
    {
        using (var scope = serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ImageDbContext>();
            await context.Database.MigrateAsync();
        }
    }
}
