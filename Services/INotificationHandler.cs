using MediatR;
using Microsoft.Extensions.Logging;

namespace BankingServices.Events
{
    public class DomainEventHandler : INotificationHandler<DomainEventNotification>
    {
        private readonly ILogger<DomainEventHandler> _logger;

        public DomainEventHandler(ILogger<DomainEventHandler> logger)
        {
            _logger = logger;
        }

        public Task Handle(DomainEventNotification notification, CancellationToken cancellationToken)
        {
            // For demonstration, just log the event.
            // You can add logic for revert, message bus publishing, etc.
            _logger.LogInformation(
                "Domain Event Received: {EventType} - {Details} (Transaction ID: {TransactionId})",
                notification.Event.EventType,
                notification.Event.Details,
                notification.Event.TransactionId
            );

            return Task.CompletedTask;
        }
    }
}