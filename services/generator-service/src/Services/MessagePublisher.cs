using System.Text;
using System.Text.Json;

namespace GeneratorService.Services
{
    public interface IMessagePublisher
    {
        Task PublishJobAsync<T>(string queue, T message);
    }

    /// <summary>
    /// Simple local message publisher for MVP (no external message bus)
    /// Later: replace with RabbitMQ or Azure Service Bus
    /// </summary>
    public class LocalMessagePublisher : IMessagePublisher
    {
        private readonly ILogger<LocalMessagePublisher> _logger;

        public LocalMessagePublisher(ILogger<LocalMessagePublisher> logger)
        {
            _logger = logger;
        }

        public async Task PublishJobAsync<T>(string queue, T message)
        {
            try
            {
                var json = JsonSerializer.Serialize(message);
                _logger.LogInformation("Published to queue {Queue}: {Message}", queue, json);
                // In production, write to file or real message bus
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing to queue {Queue}", queue);
                throw;
            }
        }
    }
}
