using BankingServices.Models;
using MediatR;

namespace BankingServices.Events
{
    public class DomainEventNotification : INotification
    {
        public TransactionEvent Event { get; }

        public DomainEventNotification(TransactionEvent domainEvent)
        {
            Event = domainEvent;
        }
    }
}