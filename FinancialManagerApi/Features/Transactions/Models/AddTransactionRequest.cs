namespace FinancialManagerApi.Models
{
    public class AddTransactionRequest
    {
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public string Category { get; set; }
    }
}