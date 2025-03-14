using BankingServices.Models;

namespace BankingServices.Repositories
{
    public interface ITransactionRepository : IRepository<TransactionLog>
    {
        Task AddTransactionAsync(TransactionLog transaction);
    }
}