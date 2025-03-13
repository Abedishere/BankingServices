using BankingServices.Data;
using BankingServices.Messages;
using BankingServices.Models;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace BankingServices.Services
{
    public class TransactionLogService : ITransactionLogService
    {
        private readonly BankingDbContext _context;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<TransactionLogService> _logger;

        public TransactionLogService(
            BankingDbContext context,
            IPublishEndpoint publishEndpoint,
            ILogger<TransactionLogService> logger)
        {
            _context = context;
            _publishEndpoint = publishEndpoint;
            _logger = logger;
        }

        public async Task<TransactionLog> CreateTransactionLogAsync(TransactionLog transactionLog)
        {
            transactionLog.Timestamp = DateTime.UtcNow;
            
            _context.TransactionLogs.Add(transactionLog);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Transaction log created for account {AccountId}: {TransactionType} of {Amount:C}", 
                transactionLog.AccountId, transactionLog.TransactionType, transactionLog.Amount);

            // Publish the transaction logged event
            await _publishEndpoint.Publish(new TransactionLoggedEvent
            {
                TransactionLogId = transactionLog.Id,
                AccountId = transactionLog.AccountId,
                TransactionType = transactionLog.TransactionType,
                Amount = transactionLog.Amount,
                Timestamp = transactionLog.Timestamp,
                Status = transactionLog.Status,
                Details = transactionLog.Details
            });

            return transactionLog;
        }

        public async Task<IEnumerable<TransactionLog>> GetTransactionLogsByAccountIdAsync(long accountId)
        {
            return await _context.TransactionLogs
                .Where(tl => tl.AccountId == accountId)
                .OrderByDescending(tl => tl.Timestamp)
                .ToListAsync();
        }

        public IQueryable<TransactionLog> GetTransactionLogsQueryable()
        {
            return _context.TransactionLogs.AsQueryable();
        }
    }
}