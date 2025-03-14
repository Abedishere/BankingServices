using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BankingServices.Models
{
    public class TransactionEvent
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        
        // Tie events to a specific transaction or account
        public long TransactionId { get; set; }
        
        // E.g., "DepositCreated", "WithdrawalCompleted", "AccountStatusChanged"
        public string EventType { get; set; } = string.Empty;
        
        // JSON or text describing details of the event
        public string Details { get; set; } = string.Empty;
        
        // Timestamp for when the event was triggered
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}