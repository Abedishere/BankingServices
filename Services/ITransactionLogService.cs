using BankingServices.Models;

namespace BankingServices.Services
{
    public interface ITransactionLogService
    {
        Task<TransactionLog> CreateTransactionLogAsync(TransactionLog transactionLog);
        Task<IEnumerable<TransactionLog>> GetTransactionLogsByAccountIdAsync(long accountId);
        IQueryable<TransactionLog> GetTransactionLogsQueryable();
    }
}