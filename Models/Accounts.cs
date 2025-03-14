namespace BankingServices.Models
{
    public class Account
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string AccountType { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public decimal CurrentBalance { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation property for transactions
        public virtual ICollection<TransactionLog> Transactions { get; set; } = new List<TransactionLog>();
    }
}