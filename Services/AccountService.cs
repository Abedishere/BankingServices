using BankingServices.Data;
using BankingServices.Models;
using BankingServices.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace BankingServices.Services
{
    public class AccountService : IAccountService
    {
        private readonly BankingDbContext _context;
        private readonly ILogger<AccountService> _logger;

        public AccountService(BankingDbContext context, ILogger<AccountService> logger)
        {
            _context = context;
            _logger = logger;
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
                    .Where(g => g.Select(t => t.AccountId).Distinct().Count() > 1) // Ensure transactions come from different accounts
                    .ToListAsync();

                var result = new List<CommonTransactionResponse>();

                foreach (var group in transactionsByAmountAndType)
                {
                    var transactions = group.ToList();
                    var uniqueAccountIds = transactions.Select(t => t.AccountId).Distinct().ToList();

                    // Only include if at least 2 of the requested accounts have this transaction pattern
                    if (uniqueAccountIds.Count >= 2)
                    {
                        // For each transaction in this group
                        foreach (var transaction in transactions)
                        {
                            // Check if we already added this transaction
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
                // Get all accounts for the user
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

                // Get all transactions for these accounts
                var transactions = await _context.TransactionLogs
                    .Where(t => accountIds.Contains(t.AccountId))
                    .ToListAsync();

                var summary = new AccountBalanceSummary
                {
                    UserId = userId,
                    TotalAccounts = accounts.Count,
                    Accounts = new List<AccountSummary>()
                };

                // Calculate totals for each account
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
    }
}