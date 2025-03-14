using MassTransit;

namespace BankingServices.Services
{
    public class RabbitMQService : IRabbitMQService
    {
        private readonly ILogger<RabbitMQService> _logger;
        private readonly IBus? _bus;

        public RabbitMQService(ILogger<RabbitMQService> logger, IBus? bus = null)
        {
            _logger = logger;
            _bus = bus;
        }

        public async Task PublishAsync<T>(T message, string routingKey) where T : class
        {
            if (_bus == null)
            {
                _logger.LogWarning("MassTransit not configured. Message not published with routing key: {RoutingKey}", routingKey);
                return;
            }

            try
            {
                _logger.LogInformation("Publishing message with routing key: {RoutingKey}", routingKey);
                await _bus.Publish(message);
                _logger.LogInformation("Message published successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing message with routing key: {RoutingKey}", routingKey);
                throw;
            }
        }
    }
}