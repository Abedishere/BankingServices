using BankingServices.Data;
using BankingServices.Events;
using BankingServices.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BankingServices.Services
{
    public class EventService : IEventService
    {
        private readonly BankingDbContext _dbContext;
        private readonly IMediator _mediator;
        private readonly ILogger<EventService> _logger;

        public EventService(BankingDbContext dbContext, IMediator mediator, ILogger<EventService> logger)
        {
            _dbContext = dbContext;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<TransactionEvent> CreateEventAsync(
            long transactionId,
            string eventType,
            string details,
            DateTime timestamp)
        {
            var domainEvent = new TransactionEvent
            {
                TransactionId = transactionId,
                EventType = eventType,
                Details = details,
                Timestamp = timestamp
            };

            try
            {
                _dbContext.TransactionEvents.Add(domainEvent);
                await _dbContext.SaveChangesAsync();

                // Publish domain event to MediatR
                await _mediator.Publish(new DomainEventNotification(domainEvent));

                return domainEvent;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating TransactionEvent");
                throw;
            }
        }

        public async Task<IEnumerable<TransactionEvent>> GetEventsByTransactionIdAsync(long transactionId)
        {
            try
            {
                return await _dbContext.TransactionEvents
                    .Where(e => e.TransactionId == transactionId)
                    .OrderByDescending(e => e.Timestamp)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving TransactionEvents for transactionId: {TransactionId}", transactionId);
                throw;
            }
        }
    }
}
