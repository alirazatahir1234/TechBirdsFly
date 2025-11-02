using System.Text;
using System.Text.Json;
using TechBirdsFly.Shared.Events.Contracts;
using AuthService.Application.Interfaces;

namespace AuthService.Infrastructure.EventBus;

/// <summary>
/// HTTP-based event publisher that communicates with the Event Bus service
/// </summary>
public class EventBusHttpPublisher : IEventPublisher
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<EventBusHttpPublisher> _logger;

    public EventBusHttpPublisher(
        HttpClient httpClient,
        ILogger<EventBusHttpPublisher> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Publish an event to the Event Bus service
    /// </summary>
    public async Task PublishEventAsync(
        IEventContract @event,
        CancellationToken cancellationToken = default)
    {
        await PublishEventAsync(@event, @event.CorrelationId ?? Guid.NewGuid().ToString(), cancellationToken);
    }

    /// <summary>
    /// Publish an event with specific correlation ID
    /// </summary>
    public async Task PublishEventAsync(
        IEventContract @event,
        string correlationId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (@event == null)
                throw new ArgumentNullException(nameof(@event));

            _logger.LogInformation(
                "📤 Publishing event to Event Bus - EventId: {EventId}, EventType: {EventType}",
                @event.EventId,
                @event.EventType);

            // Serialize event
            var eventJson = JsonSerializer.Serialize(@event, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = false
            });

            // Create publish request
            var publishRequest = new
            {
                eventType = @event.EventType,
                eventData = eventJson,
                correlationId = correlationId ?? @event.CorrelationId
            };

            var requestJson = JsonSerializer.Serialize(publishRequest);
            var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

            // Send to Event Bus
            var response = await _httpClient.PostAsync(
                "/api/events/publish",
                content,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError(
                    "❌ Event Bus returned error - Status: {Status}, Content: {Content}",
                    response.StatusCode,
                    errorContent);

                throw new InvalidOperationException(
                    $"Event Bus returned {response.StatusCode}: {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogInformation(
                "✅ Event published successfully - EventId: {EventId}, Response: {Response}",
                @event.EventId,
                responseContent);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(
                ex,
                "❌ HTTP error publishing event to Event Bus - EventType: {EventType}",
                @event.EventType);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "❌ Error publishing event to Event Bus - EventId: {EventId}, Error: {Error}",
                @event.EventId,
                ex.Message);
            throw;
        }
    }
}
