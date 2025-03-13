namespace BankingServices.Models
{
    public class TransactionLog
    {
        public long Id { get; set; }
        public long AccountId { get; set; }
        public string TransactionType { get; set; } = string.Empty; // Deposit, Withdrawal
        public decimal Amount { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = string.Empty; // Pending, Completed, Failed
        public string Details { get; set; } = string.Empty;
    }
}