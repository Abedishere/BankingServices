using MassTransit;

namespace BankingServices.Services
{
    public class RabbitMQService : IRabbitMQService
    {
        private readonly IBus _bus;
        private readonly ILogger<RabbitMQService> _logger;

        public RabbitMQService(IBus bus, ILogger<RabbitMQService> logger)
        {
            _bus = bus;
            _logger = logger;
        }

        public async Task PublishAsync<T>(T message, string routingKey) where T : class
        {
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