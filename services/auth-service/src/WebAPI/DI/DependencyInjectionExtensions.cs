using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using AuthService.Application.Interfaces;
using AuthService.Application.Services;
using AuthService.Infrastructure.Persistence;
using AuthService.Infrastructure.Repositories;
using AuthService.Infrastructure.Cache;
using AuthService.Infrastructure.ExternalServices;
using AuthService.Infrastructure.EventBus;

namespace AuthService.WebAPI.DI;

/// <summary>
/// Dependency Injection extension methods for Auth Service
/// </summary>
public static class DependencyInjectionExtensions
{
    /// <summary>
    /// Add application layer services (business logic)
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<AuthApplicationService>();
        services.AddScoped<AuthEventPublisherService>();
        return services;
    }

    /// <summary>
    /// Add infrastructure layer services (data access, external services)
    /// </summary>
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        services.AddDbContext<AuthDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? "Data Source=auth.db";

            if (connectionString.Contains("Host=", StringComparison.OrdinalIgnoreCase))
            {
                options.UseNpgsql(connectionString);
            }
            else if (connectionString.Contains("Server=", StringComparison.OrdinalIgnoreCase))
            {
                options.UseSqlServer(connectionString);
            }
            else if (connectionString.Contains("Data Source=", StringComparison.OrdinalIgnoreCase) || 
                     connectionString.Contains("sqlite", StringComparison.OrdinalIgnoreCase) ||
                     connectionString.EndsWith(".db", StringComparison.OrdinalIgnoreCase))
            {
                options.UseSqlite(connectionString);
            }
            else
            {
                // Default to SQLite for local development
                options.UseSqlite(connectionString);
            }
        });

        // Redis Cache - Register IDistributedCache properly
        var redisConnection = configuration.GetConnectionString("Redis") ?? "localhost:6379";
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnection;
            options.InstanceName = "auth:";
        });

        // Repositories
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUserRepository, UserRepository>();

        // External Services
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<ICacheService, RedisCacheService>();

        var jwtSecret = configuration["Jwt:Secret"] ?? "your-super-secret-key-that-is-at-least-32-characters-long";
        var jwtIssuer = configuration["Jwt:Issuer"] ?? "TechBirdsFly";
        var jwtAudience = configuration["Jwt:Audience"] ?? "TechBirdsFly";
        var jwtExpiration = int.Parse(configuration["Jwt:ExpirationMinutes"] ?? "60");

        services.AddScoped<ITokenService>(provider =>
            new JwtTokenService(jwtSecret, jwtIssuer, jwtAudience, jwtExpiration)
        );

        // Event Bus Integration
        var eventBusBaseUrl = configuration["EventBus:BaseUrl"] ?? "http://localhost:5020";
        services.AddHttpClient<IEventPublisher, EventBusHttpPublisher>(client =>
        {
            client.BaseAddress = new Uri(eventBusBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        return services;
    }
}
