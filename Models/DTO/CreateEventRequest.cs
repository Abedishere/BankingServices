namespace BankingServices.Controllers; 
public class CreateEventRequest
{
    public long TransactionId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}