using BankingServices.Data;
using BankingServices.Models;
using BankingServices.Models.DTOs;
using BankingServices.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace BankingServices.Services
{
    public class AccountService : IAccountService
    {
        private readonly BankingDbContext _context;
        private readonly ILogger<AccountService> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public AccountService(BankingDbContext context, ILogger<AccountService> logger, IUnitOfWork unitOfWork)
        {
            _context = context;
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<List<CommonTransactionResponse>> GetCommonTransactionsAsync(List<long> accountIds)
        {
            if (accountIds == null || accountIds.Count < 2)
            {
                throw new ArgumentException("At least two account IDs are required", nameof(accountIds));
            }

            try
            {
                // Group transactions by amount and type to find common ones
                var transactionsByAmountAndType = await _context.TransactionLogs
                    .Where(t => accountIds.Contains(t.AccountId))
                    .GroupBy(t => new { t.Amount, t.TransactionType })
                    .Where(g => g.Select(t => t.AccountId).Distinct().Count() > 1)
                    .ToListAsync();

                var result = new List<CommonTransactionResponse>();

                foreach (var group in transactionsByAmountAndType)
                {
                    var transactions = group.ToList();
                    var uniqueAccountIds = transactions.Select(t => t.AccountId).Distinct().ToList();

                    if (uniqueAccountIds.Count >= 2)
                    {
                        foreach (var transaction in transactions)
                        {
                            if (!result.Any(r => r.TransactionId == transaction.Id))
                            {
                                result.Add(new CommonTransactionResponse
                                {
                                    TransactionId = transaction.Id,
                                    AccountIds = uniqueAccountIds,
                                    TransactionType = transaction.TransactionType,
                                    Amount = transaction.Amount,
                                    Timestamp = transaction.Timestamp
                                });
                            }
                        }
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving common transactions for account IDs: {AccountIds}", string.Join(", ", accountIds));
                throw;
            }
        }

        public async Task<AccountBalanceSummary> GetAccountBalanceSummaryAsync(long userId)
        {
            try
            {
                var accounts = await _context.Accounts
                    .Where(a => a.UserId == userId)
                    .ToListAsync();

                if (!accounts.Any())
                {
                    return new AccountBalanceSummary
                    {
                        UserId = userId,
                        TotalAccounts = 0,
                        TotalDeposits = 0,
                        TotalWithdrawals = 0,
                        TotalBalance = 0
                    };
                }

                var accountIds = accounts.Select(a => a.Id).ToList();
                var transactions = await _context.TransactionLogs
                    .Where(t => accountIds.Contains(t.AccountId))
                    .ToListAsync();

                var summary = new AccountBalanceSummary
                {
                    UserId = userId,
                    TotalAccounts = accounts.Count,
                    Accounts = new List<AccountSummary>()
                };

                foreach (var account in accounts)
                {
                    var accountTransactions = transactions.Where(t => t.AccountId == account.Id).ToList();

                    var deposits = accountTransactions
                        .Where(t => t.TransactionType == "Deposit" && t.Status == "Completed")
                        .Sum(t => t.Amount);

                    var withdrawals = accountTransactions
                        .Where(t => t.TransactionType == "Withdrawal" && t.Status == "Completed")
                        .Sum(t => t.Amount);

                    var accountSummary = new AccountSummary
                    {
                        AccountId = account.Id,
                        AccountNumber = account.AccountNumber,
                        AccountType = account.AccountType,
                        TotalDeposits = deposits,
                        TotalWithdrawals = withdrawals,
                        CurrentBalance = account.CurrentBalance
                    };

                    summary.Accounts.Add(accountSummary);
                    summary.TotalDeposits += deposits;
                    summary.TotalWithdrawals += withdrawals;
                    summary.TotalBalance += account.CurrentBalance;
                }

                return summary;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving account balance summary for user ID: {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> TransferFundsAsync(long fromAccountId, long toAccountId, decimal amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Amount must be greater than zero.", nameof(amount));

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var fromAccount = await _unitOfWork.AccountRepository.GetAccountByIdAsync(fromAccountId);
                var toAccount = await _unitOfWork.AccountRepository.GetAccountByIdAsync(toAccountId);

                if (fromAccount == null || toAccount == null)
                    throw new Exception("One or both accounts not found.");

                if (fromAccount.CurrentBalance < amount)
                    throw new Exception("Insufficient funds.");

                // Deduct funds from the source account
                fromAccount.CurrentBalance -= amount;
                _unitOfWork.AccountRepository.Update(fromAccount);

                // Add funds to the destination account
                toAccount.CurrentBalance += amount;
                _unitOfWork.AccountRepository.Update(toAccount);

                // Log the transfer as a transaction
                var transactionLog = new TransactionLog
                {
                    AccountId = fromAccountId,
                    TransactionType = "Transfer",
                    Amount = amount,
                    Timestamp = DateTime.UtcNow,
                    Status = "Completed",
                    Details = $"Transferred {amount} from account {fromAccountId} to account {toAccountId}"
                };

                await _unitOfWork.TransactionRepository.AddTransactionAsync(transactionLog);

                await _unitOfWork.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Error during fund transfer from {FromAccountId} to {ToAccountId}", fromAccountId, toAccountId);
                return false;
            }
        }
    }
}
