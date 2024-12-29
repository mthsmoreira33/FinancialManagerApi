namespace FinancialManagerApi.Models;

public class UpdateTransactionRequest
{
    public decimal Amount { get; set; }
    public string Description { get; set; } = "";
    public DateTime Date { get; set; }
}