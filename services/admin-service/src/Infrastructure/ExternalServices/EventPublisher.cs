using Microsoft.Extensions.Logging;
using TechBirdsFly.AdminService.Application.Interfaces;
using TechBirdsFly.Shared.Events;

namespace TechBirdsFly.AdminService.Infrastructure.ExternalServices;

/// <summary>
/// EventPublisher implementation that publishes domain events to the Event Bus Service.
/// Communicates with the central Event Bus Service via HTTP for event distribution.
/// Events are then routed to other microservices via Kafka topics.
/// </summary>
public class EventPublisher : IEventPublisher
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<EventPublisher> _logger;
    private readonly string _eventBusServiceUrl;

    public EventPublisher(HttpClient httpClient, ILogger<EventPublisher> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        // Default to localhost for local development, can be overridden via HttpClient configuration
        _eventBusServiceUrl = "http://localhost:5020";
    }

    /// <summary>
    /// Publishes a domain event to the Event Bus Service.
    /// The event is wrapped with metadata and sent as JSON.
    /// If publishing fails, it's logged but doesn't throw to maintain service resilience.
    /// </summary>
    /// <typeparam name="T">The domain event type implementing IDomainEvent</typeparam>
    /// <param name="domainEvent">The event instance to publish</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task PublishAsync<T>(T domainEvent, CancellationToken cancellationToken = default) where T : IDomainEvent
    {
        if (domainEvent == null)
        {
            _logger.LogWarning("Cannot publish null domain event");
            return;
        }

        try
        {
            var eventType = typeof(T).Name;
            var publishUrl = $"{_eventBusServiceUrl}/api/events";

            var eventPayload = new
            {
                eventType = eventType,
                eventId = Guid.NewGuid(),
                timestamp = DateTime.UtcNow,
                data = domainEvent
            };

            var content = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(eventPayload),
                System.Text.Encoding.UTF8,
                "application/json");

            _logger.LogInformation(
                "Publishing domain event '{EventType}' with ID '{EventId}' to Event Bus Service",
                eventType,
                eventPayload.eventId);

            var response = await _httpClient.PostAsync(publishUrl, content, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation(
                    "Successfully published domain event '{EventType}' to Event Bus Service",
                    eventType);
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError(
                    "Failed to publish domain event '{EventType}'. Status: {StatusCode}. Response: {Response}",
                    eventType,
                    response.StatusCode,
                    errorContent);
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(
                ex,
                "Network error while publishing domain event '{EventType}'. Event Bus Service may be unavailable",
                typeof(T).Name);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(
                ex,
                "Timeout while publishing domain event '{EventType}' to Event Bus Service",
                typeof(T).Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected error while publishing domain event '{EventType}'",
                typeof(T).Name);
        }
    }
}
