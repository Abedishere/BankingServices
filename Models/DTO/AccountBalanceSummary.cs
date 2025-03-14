// Models/DTOs/AccountBalanceSummary.cs
namespace BankingServices.Models.DTOs
{
    public class AccountBalanceSummary
    {
        public long UserId { get; set; }
        public int TotalAccounts { get; set; }
        public decimal TotalDeposits { get; set; }
        public decimal TotalWithdrawals { get; set; }
        public decimal TotalBalance { get; set; }
        public List<AccountSummary> Accounts { get; set; } = new List<AccountSummary>();
    }

    public class AccountSummary
    {
        public long AccountId { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public string AccountType { get; set; } = string.Empty;
        public decimal TotalDeposits { get; set; }
        public decimal TotalWithdrawals { get; set; }
        public decimal CurrentBalance { get; set; }
    }
}