using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using EventBusService.Infrastructure.Persistence;
using EventBusService.Infrastructure.Kafka;
using EventBusService.Infrastructure.Repositories;
using EventBusService.Infrastructure.BackgroundServices;
using EventBusService.Application.Interfaces;
using EventBusService.Application.Services;

namespace EventBusService.WebAPI.DI;

/// <summary>
/// Dependency Injection extensions for Event Bus Service
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add Event Bus Services to DI container
    /// </summary>
    public static IServiceCollection AddEventBusServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<EventBusDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Repositories
        services.AddScoped<IOutboxEventRepository, OutboxEventRepository>();
        services.AddScoped<IEventSubscriptionRepository, EventSubscriptionRepository>();

        // Application Services
        services.AddScoped<PublishEventService>();
        services.AddScoped<OutboxPublisherService>();
        services.AddSingleton<EventRouter>();
        services.AddScoped<EventConsumerService>();

        // Event Consumer Settings
        services.AddSingleton(configuration
            .GetSection("EventConsumer")
            .Get<EventConsumerBackgroundSettings>() ?? new EventConsumerBackgroundSettings());

        // Background Services - Event Consumer
        services.AddHostedService<EventConsumerBackgroundService>();

        // Outbox Publisher Settings
        services.AddSingleton(configuration
            .GetSection("OutboxPublisher")
            .Get<OutboxPublisherSettings>() ?? new OutboxPublisherSettings());

        services.AddSingleton(configuration
            .GetSection("OutboxPublisher")
            .Get<OutboxPublisherBackgroundSettings>() ?? new OutboxPublisherBackgroundSettings());

        // Background Services
        services.AddHostedService<OutboxPublisherBackgroundService>();

        // Kafka
        var kafkaConfig = configuration.GetSection("Kafka");
        services.AddSingleton(kafkaConfig.Get<KafkaSettings>() ?? new KafkaSettings());
        services.AddSingleton<IKafkaProducer, KafkaProducer>();
        services.AddSingleton<IKafkaConsumer, KafkaConsumer>();

        // Health Checks
        var kafkaBootstrapServers = kafkaConfig.GetValue<string>("BootstrapServers") ?? "localhost:9092";
        services
            .AddHealthChecks()
            .AddDbContextCheck<EventBusDbContext>(
                name: "EventBusDB",
                failureStatus: Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy);
            //.AddKafka(
            //    kafkaBootstrapServers,
            //    name: "Kafka",
            //    failureStatus: Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Degraded);

        return services;
    }
}
