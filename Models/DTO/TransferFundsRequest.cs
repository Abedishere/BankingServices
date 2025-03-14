namespace BankingServices.Models.DTO;

public class TransferFundsRequest
{
    public long FromAccountId { get; set; }
    public long ToAccountId { get; set; }
    public decimal Amount { get; set; }
}