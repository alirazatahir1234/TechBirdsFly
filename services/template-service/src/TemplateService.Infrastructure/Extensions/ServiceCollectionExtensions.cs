using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Minio;
using TemplateService.Domain.Interfaces;
using TemplateService.Infrastructure.Data;
using TemplateService.Infrastructure.Repositories;
using TemplateService.Infrastructure.Storage;

namespace TemplateService.Infrastructure.Extensions;

/// <summary>
/// Dependency injection extension for Infrastructure layer
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // PostgreSQL Database
        var connectionString = configuration.GetConnectionString("Postgres");
        services.AddDbContext<TemplateDbContext>(options =>
            options.UseNpgsql(connectionString)
        );

        // Repository
        services.AddScoped<ITemplateRepository, TemplateRepository>();

        // MinIO File Storage
        var minioEndpoint = configuration["Minio:Endpoint"];
        var minioAccessKey = configuration["Minio:AccessKey"];
        var minioSecretKey = configuration["Minio:SecretKey"];

        var minioClient = new MinioClient()
            .WithEndpoint(minioEndpoint)
            .WithCredentials(minioAccessKey, minioSecretKey)
            .Build();

        services.AddSingleton<IMinioClient>(minioClient);
        services.AddScoped<IFileStorage, MinioFileStorage>();

        return services;
    }
}
