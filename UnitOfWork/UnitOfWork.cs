using BankingServices.Data;
using BankingServices.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace BankingServices.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly BankingDbContext _context;
        private IDbContextTransaction? _transaction;

        public IAccountRepository AccountRepository { get; }
        public ITransactionRepository TransactionRepository { get; }

        public UnitOfWork(BankingDbContext context)
        {
            _context = context;
            AccountRepository = new AccountRepository(context);
            TransactionRepository = new TransactionRepository(context);
        }

        public async Task BeginTransactionAsync()
        {
            if (_transaction == null)
            {
                _transaction = await _context.Database.BeginTransactionAsync();
            }
        }

        public async Task CommitAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
                if (_transaction != null)
                {
                    await _transaction.CommitAsync();
                }
            }
            catch
            {
                await RollbackAsync();
                throw;
            }
            finally
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }

        public async Task RollbackAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task<int> SaveChangesAsync() =>
            await _context.SaveChangesAsync();

        public void Dispose() =>
            _context.Dispose();
    }
}