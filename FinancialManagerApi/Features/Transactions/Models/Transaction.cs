namespace FinancialManagerApi.Models;

public class Transaction
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string Description { get; set; }
    public string Type { get; set; }
    public string Category { get; set; }

    // Foreign Keys
    public Guid UserId { get; set; }
    public User User { get; set; }
}
