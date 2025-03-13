namespace BankingServices.Messages
{
    public class TransactionLoggedEvent
    {
        public long TransactionLogId { get; set; }
        public long AccountId { get; set; }
        public string TransactionType { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime Timestamp { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
    }
}