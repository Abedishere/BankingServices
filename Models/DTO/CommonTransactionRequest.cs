// Models/DTOs/CommonTransactionRequest.cs
namespace BankingServices.Models.DTOs
{
    public class CommonTransactionRequest
    {
        public List<long> AccountIds { get; set; } = new List<long>();
    }
}



