using BankingServices.Data;
using BankingServices.Models;
using Microsoft.EntityFrameworkCore;

namespace BankingServices.Repositories
{
    public class TransactionRepository : Repository<TransactionLog>, ITransactionRepository
    {
        public TransactionRepository(BankingDbContext context) : base(context) { }

        public async Task AddTransactionAsync(TransactionLog transaction)
        {
            await _dbSet.AddAsync(transaction);
        }
    }
}