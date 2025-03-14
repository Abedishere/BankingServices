// Models/DTOs/CommonTransactionResponse.cs
namespace BankingServices.Models.DTOs
{
    public class CommonTransactionResponse
    {
        public long TransactionId { get; set; }
        public List<long> AccountIds { get; set; } = new List<long>();
        public string TransactionType { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime Timestamp { get; set; }
    }
}