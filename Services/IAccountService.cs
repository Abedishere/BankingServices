using BankingServices.Models;
using BankingServices.Models.DTOs;

namespace BankingServices.Services
{
    public interface IAccountService
    {
        Task<List<CommonTransactionResponse>> GetCommonTransactionsAsync(List<long> accountIds);
        Task<AccountBalanceSummary> GetAccountBalanceSummaryAsync(long userId);
        
        Task<bool> TransferFundsAsync(long fromAccountId, long toAccountId, decimal amount);
    }
}