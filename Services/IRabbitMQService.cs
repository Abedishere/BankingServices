namespace BankingServices.Services
{
    public interface IRabbitMQService
    {
        Task PublishAsync<T>(T message, string routingKey) where T : class;
    }
}