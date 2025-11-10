namespace GeneratorService.Infrastructure;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using GeneratorService.Application.Interfaces;
using GeneratorService.Application.Services;
using GeneratorService.Infrastructure.Persistence;
using GeneratorService.Infrastructure.Repositories;
using GeneratorService.Infrastructure.ExternalServices;

/// <summary>
/// Dependency Injection extension methods for configuring services
/// Follows the dependency injection patterns established in other microservices
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds application services to the dependency injection container
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register application services
        services.AddScoped<ITemplateApplicationService, TemplateApplicationService>();
        services.AddScoped<IProjectApplicationService, ProjectApplicationService>();
        services.AddScoped<IGenerationApplicationService, GenerationApplicationService>();

        return services;
    }

    /// <summary>
    /// Adds infrastructure services to the dependency injection container
    /// Including repositories, DbContext, and external services
    /// </summary>
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, string connectionString)
    {
        // Configure DbContext with SQLite
        services.AddDbContext<GeneratorDbContext>(options =>
        {
            options.UseSqlite(connectionString);
        });

        // Register repositories
        services.AddScoped<ITemplateRepository, TemplateRepository>();
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<IGenerationRepository, GenerationRepository>();

        // Register external services
        services.AddScoped<IAIGeneratorService, AIGeneratorService>();
        services.AddScoped<IStorageService, StorageService>();
        services.AddScoped<IEventPublisher, EventPublisher>();

        return services;
    }

    /// <summary>
    /// Creates the database schema if it doesn't exist
    /// Should be called during application startup
    /// </summary>
    public static async Task InitializeDatabaseAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<GeneratorDbContext>();

        try
        {
            // Apply any pending migrations
            await context.Database.MigrateAsync();
            Console.WriteLine("Database migration completed successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during database migration: {ex.Message}");
            throw;
        }
    }
}
