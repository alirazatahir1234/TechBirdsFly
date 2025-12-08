using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MediatR;
using ProjectService.Infrastructure.Data;

namespace ProjectService.Infrastructure;

/// <summary>
/// Extension methods for registering infrastructure services
/// </summary>
public static class InfrastructureServiceCollectionExtensions
{
    /// <summary>
    /// Add infrastructure services to the dependency injection container
    /// </summary>
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Add DbContext
        var connectionString = configuration.GetConnectionString("ProjectServiceDatabase")
            ?? "Host=localhost;Port=5432;Database=project_service;Username=postgres;Password=Alisheikh@123";

        services.AddDbContext<ProjectDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Add MediatR
        services.AddMediatR(typeof(InfrastructureServiceCollectionExtensions).Assembly);

        return services;
    }

    /// <summary>
    /// Initialize the database (create if not exists, run migrations)
    /// </summary>
    public static async Task InitializeDatabaseAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ProjectDbContext>();
        await dbContext.Database.MigrateAsync();
    }
}
