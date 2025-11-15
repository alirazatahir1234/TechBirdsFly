using System;
using System.Threading.Tasks;
using CacheService.Application.Interfaces;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace CacheService.Infrastructure.Events;

/// <summary>
/// Kafka event consumer for handling cache invalidation events from other services
/// </summary>
public class KafkaEventConsumer : IKafkaEventConsumer
{
    private readonly IConsumer<string, string> _consumer;
    private readonly ILogger<KafkaEventConsumer> _logger;
    private readonly ICacheApplicationService _cacheApplicationService;
    private bool _running = false;

    public KafkaEventConsumer(
        IConsumer<string, string> consumer,
        ILogger<KafkaEventConsumer> logger,
        ICacheApplicationService cacheApplicationService)
    {
        _consumer = consumer ?? throw new ArgumentNullException(nameof(consumer));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _cacheApplicationService = cacheApplicationService ?? throw new ArgumentNullException(nameof(cacheApplicationService));
    }

    public async Task StartAsync()
    {
        if (_running)
            return;

        _running = true;
        _consumer.Subscribe(new[] { "cache-invalidation-events" });

        _logger.LogInformation("Kafka consumer started, subscribed to cache-invalidation-events");

        await Task.Run(() =>
        {
            while (_running)
            {
                try
                {
                    var consumeResult = _consumer.Consume(TimeSpan.FromSeconds(1));

                    if (consumeResult != null)
                    {
                        _ = HandleCacheInvalidationEventAsync(consumeResult.Message.Value);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error consuming Kafka message");
                }
            }
        });
    }

    public async Task StopAsync()
    {
        _running = false;
        _consumer?.Close();
        await Task.CompletedTask;
        _logger.LogInformation("Kafka consumer stopped");
    }

    public async Task HandleCacheInvalidationEventAsync(string message)
    {
        try
        {
            _logger.LogDebug($"Processing cache invalidation event: {message}");

            // Parse event and invalidate cache accordingly
            // Format: {"pattern":"user:*","service":"UserService"}

            var eventData = System.Text.Json.JsonDocument.Parse(message);
            var root = eventData.RootElement;

            if (root.TryGetProperty("pattern", out var patternElement))
            {
                var pattern = patternElement.GetString();
                var service = root.TryGetProperty("service", out var serviceElement) 
                    ? serviceElement.GetString() 
                    : null;

                if (!string.IsNullOrEmpty(pattern))
                {
                    _ = await _cacheApplicationService.RemoveByPatternAsync(pattern, service);
                    _logger.LogInformation($"Cache invalidated for pattern: {pattern} (Service: {service})");
                }
            }

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling cache invalidation event");
        }
    }
}
