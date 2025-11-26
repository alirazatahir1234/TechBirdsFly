using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TechBirdsFly.EditorService.Application.Interfaces;
using TechBirdsFly.EditorService.Domain.Interfaces;
using TechBirdsFly.EditorService.Infrastructure.AI;
using TechBirdsFly.EditorService.Infrastructure.Persistence;
using TechBirdsFly.EditorService.Infrastructure.Repositories;

namespace TechBirdsFly.EditorService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        // Database
        var connectionString = config.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found");

        services.AddDbContext<EditorDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Repositories
        services.AddScoped<ISectionRepository, SectionRepository>();

        // AI Service
        services.AddScoped<ISectionAIService, SectionAIService>();

        return services;
    }
}
