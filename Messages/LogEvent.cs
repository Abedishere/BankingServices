namespace BankingServices.Messages
{
    public class LogEvent
    {
        public Guid RequestId { get; set; }
        public object? RequestObject { get; set; }
        public string RouteURL { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}