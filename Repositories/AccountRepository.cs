using BankingServices.Data;
using BankingServices.Models;
using Microsoft.EntityFrameworkCore;

namespace BankingServices.Repositories
{
    public class AccountRepository : Repository<Account>, IAccountRepository
    {
        public AccountRepository(BankingDbContext context) : base(context) { }

        public async Task<Account?> GetAccountByIdAsync(long accountId)
        {
            return await _context.Accounts.FirstOrDefaultAsync(a => a.Id == accountId);
        }
    }
}