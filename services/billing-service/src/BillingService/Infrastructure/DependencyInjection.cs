namespace BillingService.Infrastructure;

using BillingService.Application.Interfaces;
using BillingService.Application.Services;
using BillingService.Infrastructure.ExternalServices;
using BillingService.Infrastructure.Persistence;
using BillingService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Dependency injection extension methods for billing service
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Add billing services to the dependency injection container
    /// </summary>
    public static IServiceCollection AddBillingServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        services.AddDbContext<BillingDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found");

            options.UseSqlite(connectionString, sqliteOptions =>
            {
                sqliteOptions.MigrationsAssembly(typeof(BillingDbContext).Assembly.GetName().Name);
            });
        });

        // Repositories
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IPlanRepository, PlanRepository>();
        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();

        // Application Services
        services.AddScoped<IInvoiceApplicationService, InvoiceApplicationService>();
        services.AddScoped<IPaymentApplicationService, PaymentApplicationService>();
        services.AddScoped<IPlanApplicationService, PlanApplicationService>();
        services.AddScoped<ISubscriptionApplicationService, SubscriptionApplicationService>();

        // External Services
        services.AddScoped<IEventPublisher, EventPublisher>();
        services.AddScoped<IPaymentGatewayService, PaymentGatewayService>();

        return services;
    }
}
