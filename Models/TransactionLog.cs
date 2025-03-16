namespace BankingServices.Models
{
    public class TransactionLog
    {
        public long Id { get; set; }
        public long AccountId { get; set; }
        
        // Multi-language description fields
        public string DescriptionEn { get; set; } = string.Empty;
        public string DescriptionEs { get; set; } = string.Empty;
        public string DescriptionFr { get; set; } = string.Empty;

        // Transaction type, amount, timestamps, etc.
        public string TransactionType { get; set; } = string.Empty; // e.g., Deposit, Withdrawal, Transfer
        public decimal Amount { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = string.Empty; // e.g., Pending, Completed, Failed
        public string Details { get; set; } = string.Empty;
    }
}