using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using AuthService.Application.Interfaces;
using AuthService.Application.Services;
using AuthService.Infrastructure.Persistence;
using AuthService.Infrastructure.Repositories;
using AuthService.Infrastructure.Cache;
using AuthService.Infrastructure.ExternalServices;

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
                ?? "Host=localhost;Port=5432;Database=techbirdsfly_auth;Username=postgres;Password=Alisheikh@123";

            if (connectionString.Contains("Host=", StringComparison.OrdinalIgnoreCase))
            {
                options.UseNpgsql(connectionString);
            }
            else if (connectionString.Contains("sqlite", StringComparison.OrdinalIgnoreCase))
            {
                options.UseSqlite(connectionString);
            }
            else if (connectionString.Contains("Server=", StringComparison.OrdinalIgnoreCase))
            {
                options.UseSqlServer(connectionString);
            }
            else
            {
                // Default to PostgreSQL
                options.UseNpgsql(connectionString);
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

        return services;
    }
}
