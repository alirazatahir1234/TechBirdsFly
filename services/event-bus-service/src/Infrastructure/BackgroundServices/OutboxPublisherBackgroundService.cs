using EventBusService.Application.Services;

namespace EventBusService.Infrastructure.BackgroundServices;

/// <summary>
/// Background service that periodically publishes outbox events
/// Runs on a configurable interval to poll unpublished events
/// </summary>
public class OutboxPublisherBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OutboxPublisherBackgroundService> _logger;
    private readonly OutboxPublisherBackgroundSettings _settings;

    public OutboxPublisherBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<OutboxPublisherBackgroundService> logger,
        OutboxPublisherBackgroundSettings settings)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
    }

    /// <summary>
    /// Main execution loop for the background service
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "🚀 Outbox Publisher Background Service started - " +
            "IntervalSeconds: {IntervalSeconds}, Enabled: {Enabled}",
            _settings.IntervalSeconds,
            _settings.Enabled);

        // Initial delay before first run (allows service startup to complete)
        await Task.Delay(_settings.StartupDelaySeconds * 1000, stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (_settings.Enabled)
                {
                    await ProcessOutboxEventsAsync(stoppingToken);
                }
                else
                {
                    _logger.LogDebug("⏸️ Outbox Publisher is disabled (set Enabled=true in configuration)");
                }

                // Wait before next iteration
                await Task.Delay(_settings.IntervalSeconds * 1000, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("📤 Outbox Publisher Background Service cancelled");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "❌ Unhandled exception in Outbox Publisher Background Service");

                // Continue running despite errors
                await Task.Delay(_settings.ErrorRetryDelaySeconds * 1000, stoppingToken);
            }
        }

        _logger.LogInformation("🛑 Outbox Publisher Background Service stopped");
    }

    /// <summary>
    /// Process outbox events using scoped services
    /// </summary>
    private async Task ProcessOutboxEventsAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var publisherService = scope.ServiceProvider.GetRequiredService<OutboxPublisherService>();

        var result = await publisherService.PublishPendingEventsAsync(stoppingToken);

        if (result.IsSuccess && result.TotalProcessed == 0)
        {
            _logger.LogDebug("✅ No pending events found");
        }
        else if (result.IsSuccess)
        {
            _logger.LogInformation(
                "✅ Outbox batch completed - Successful: {Successful}, Failed: {Failed}, " +
                "Cancelled: {Cancelled}, DeadLetter: {DeadLetter}",
                result.SuccessfulCount,
                result.FailedCount,
                result.CancelledCount,
                result.DeadLetterCount);
        }
        else
        {
            _logger.LogError(
                "❌ Outbox batch failed - Error: {Error}",
                result.ErrorMessage);
        }
    }
}

/// <summary>
/// Settings for outbox publisher background service
/// </summary>
public class OutboxPublisherBackgroundSettings
{
    /// <summary>
    /// Whether the background service is enabled
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Interval in seconds between publishing cycles
    /// </summary>
    public int IntervalSeconds { get; set; } = 10;

    /// <summary>
    /// Delay in seconds before first run (allows application startup)
    /// </summary>
    public int StartupDelaySeconds { get; set; } = 5;

    /// <summary>
    /// Delay in seconds before retrying after an error
    /// </summary>
    public int ErrorRetryDelaySeconds { get; set; } = 30;
}
