using System;
using CacheService.Application.Interfaces;
using CacheService.Application.Services;
using CacheService.Infrastructure.Events;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace CacheService.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCacheServiceInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Redis Connection
        var redisConnection = configuration.GetConnectionString("Redis") ?? "localhost:6379";
        var redisOptions = ConfigurationOptions.Parse(redisConnection);
        redisOptions.AbortOnConnectFail = false;

        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var connection = ConnectionMultiplexer.Connect(redisOptions);
            return connection;
        });

        // Cache Services
        services.AddScoped<IRedisCacheService, RedisCacheService>();
        services.AddScoped<ICacheApplicationService, CacheApplicationService>();
        services.AddSingleton<IMetricsService, MetricsService>();

        // Kafka Event Consumer
        var kafkaBootstrapServers = configuration.GetSection("Kafka:BootstrapServers").Value ?? "localhost:9092";
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = kafkaBootstrapServers,
            GroupId = "cache-service-consumer-group",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true
        };

        services.AddSingleton<IConsumer<string, string>>(sp =>
            new ConsumerBuilder<string, string>(consumerConfig).Build());

        services.AddSingleton<IKafkaEventConsumer, KafkaEventConsumer>();

        // Add hosted service to start Kafka consumer
        services.AddHostedService<CacheEventConsumerHostedService>();

        return services;
    }
}

/// <summary>
/// Hosted service that manages the Kafka event consumer lifecycle
/// </summary>
public class CacheEventConsumerHostedService : Microsoft.Extensions.Hosting.BackgroundService
{
    private readonly IKafkaEventConsumer _kafkaEventConsumer;
    private readonly Microsoft.Extensions.Logging.ILogger<CacheEventConsumerHostedService> _logger;

    public CacheEventConsumerHostedService(
        IKafkaEventConsumer kafkaEventConsumer,
        Microsoft.Extensions.Logging.ILogger<CacheEventConsumerHostedService> logger)
    {
        _kafkaEventConsumer = kafkaEventConsumer;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(System.Threading.CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogInformation("Starting Kafka event consumer");
            await _kafkaEventConsumer.StartAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting Kafka event consumer");
        }
    }

    public override async Task StopAsync(System.Threading.CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping Kafka event consumer");
        await _kafkaEventConsumer.StopAsync();
        await base.StopAsync(cancellationToken);
    }
}
