using BankingServices.Models;

namespace BankingServices.Services
{
    public interface IEventService
    {
        // Create and dispatch a domain event
        Task<TransactionEvent> CreateEventAsync(long transactionId, string eventType, string details, DateTime timestamp);

        // Retrieve all events for a specific transaction
        Task<IEnumerable<TransactionEvent>> GetEventsByTransactionIdAsync(long transactionId);
    }
}