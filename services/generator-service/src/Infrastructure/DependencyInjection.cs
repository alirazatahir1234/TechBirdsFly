namespace GeneratorService.Infrastructure;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using GeneratorService.Infrastructure.Persistence;
using GeneratorService.Infrastructure.Repositories;
using GeneratorService.Infrastructure.AI;
using GeneratorService.Infrastructure.Services;
using GeneratorService.Domain.Interfaces;
using GeneratorService.Application.Interfaces;

/// <summary>
/// Dependency Injection extension methods for configuring services
/// Follows the dependency injection patterns established in other microservices
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds infrastructure services to the dependency injection container
    /// Including DbContext, EF Core repositories, AI services, and the website generator
    /// Phase 4: PostgreSQL integration with production-ready persistence layer
    /// </summary>
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("GeneratorDb")
            ?? throw new InvalidOperationException("Connection string 'GeneratorDb' not found.");

        // Configure DbContext with PostgreSQL provider (Npgsql)
        services.AddDbContext<GeneratorDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "public");
                npgsqlOptions.CommandTimeout(30);
            });
        });

        // Register Phase 4: EF Core repositories for PostgreSQL
        services.AddScoped<IProjectRepository, EFProjectRepository>();
        services.AddScoped<ISectionRepository, EFSectionRepository>();
        services.AddScoped<IGeneratedPageRepository, EFGeneratedPageRepository>();

        // Register Unit of Work for transaction management
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // AutoMapper for DTO mapping
        services.AddAutoMapper(typeof(DependencyInjection).Assembly);

        // Register AI services
        services.AddScoped<ILlamaService, LlamaService>();
        services.AddScoped<PromptBuilder>();
        services.AddScoped<IPromptBuilder, PromptBuilder>();
        services.AddScoped<HtmlTemplateBuilder>();
        services.AddScoped<IHtmlTemplateBuilder, HtmlTemplateBuilder>();

        // Register website generator service (implements IWebsiteGenerator)
        services.AddScoped<IWebsiteGenerator, WebsiteGeneratorService>();

        return services;
    }

    /// <summary>
    /// Initializes the database, applying migrations and creating schema
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
            Console.WriteLine("✓ Database migration completed successfully");
            Console.WriteLine("✓ PostgreSQL schema initialized with all entity configurations");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Error during database migration: {ex.Message}");
            throw;
        }
    }
}

