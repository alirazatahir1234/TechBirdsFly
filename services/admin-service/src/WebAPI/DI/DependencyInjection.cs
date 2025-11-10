using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TechBirdsFly.AdminService.Application.Interfaces;
using TechBirdsFly.AdminService.Application.Services;
using TechBirdsFly.AdminService.Infrastructure.ExternalServices;
using TechBirdsFly.AdminService.Infrastructure.Persistence;
using TechBirdsFly.AdminService.Infrastructure.Repositories;

namespace TechBirdsFly.AdminService.WebAPI.DI;

/// <summary>
/// Dependency Injection configuration for Admin Service.
/// Registers all services, repositories, and external service dependencies.
/// Call this method in Program.cs using: builder.Services.AddAdminServices(configuration);
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddAdminServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        if (configuration == null)
            throw new ArgumentNullException(nameof(configuration));

        // Register DbContext
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("ConnectionString 'DefaultConnection' not found in configuration");

        services.AddDbContext<AdminDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly(typeof(AdminDbContext).Assembly.GetName().Name);
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelaySeconds: 10,
                    errorCodesToAdd: null);
            });

            // Enable query tracking behavior for better performance
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll);
        });

        // Register Repositories
        services.AddScoped<IAdminUserRepository, AdminUserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();

        // Register Application Services
        services.AddScoped<IAdminUserApplicationService, AdminUserApplicationService>();
        services.AddScoped<IRoleApplicationService, RoleApplicationService>();
        services.AddScoped<IAuditLogApplicationService, AuditLogApplicationService>();

        // Register Event Publisher with HttpClient
        var eventBusServiceUrl = configuration["EventBusService:Url"] ?? "http://localhost:5020";
        services.AddHttpClient<IEventPublisher, EventPublisher>(client =>
        {
            client.BaseAddress = new Uri(eventBusServiceUrl);
            client.Timeout = TimeSpan.FromSeconds(10);
        })
        .SetHandlerLifetime(TimeSpan.FromMinutes(5));

        return services;
    }
}
