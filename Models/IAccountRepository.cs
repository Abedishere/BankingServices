using BankingServices.Models;

namespace BankingServices.Repositories
{
    public interface IAccountRepository : IRepository<Account>
    {
        Task<Account?> GetAccountByIdAsync(long accountId);
    }
}