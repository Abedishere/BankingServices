using BankingServices.Messages;
using MassTransit;

namespace BankingServices.Consumers
{
    public class TransactionLoggedConsumer : IConsumer<TransactionLoggedEvent>
    {
        private readonly ILogger<TransactionLoggedConsumer> _logger;

        public TransactionLoggedConsumer(ILogger<TransactionLoggedConsumer> logger)
        {
            _logger = logger;
        }

        public Task Consume(ConsumeContext<TransactionLoggedEvent> context)
        {
            var message = context.Message;
            _logger.LogInformation("Consumed transaction logged event: {TransactionLogId} for account {AccountId}, {TransactionType} of {Amount:C}, Status: {Status}",
                message.TransactionLogId,
                message.AccountId, 
                message.TransactionType, 
                message.Amount,
                message.Status);
            
            
            
            return Task.CompletedTask;
        }
    }
}