using System.ComponentModel.DataAnnotations;

namespace FinancialManagerApi.Models;

public class User
{
    
    [Key]
    public Guid Id { get; init; }
    
    [MaxLength(100, ErrorMessage = "User name cannot be longer than 100 characters.")]
    public string Name { get; set; }
    
    [MaxLength(100, ErrorMessage = "Name cannot be longer than 100 characters.")]
    public string Email { get; set; }
    
    [MaxLength(100, ErrorMessage = "Email cannot be longer than 100 characters.")]
    public string Password { get; set; }
    
    public bool IsBlacklisted { get; set; }
    public ICollection<Transaction> Transactions { get; set; }

    public User(string name, string email, string password)
    {
        Id = Guid.NewGuid();
        Name = name;
        Email = email;
        Password = password;
        Transactions = new List<Transaction>();
    }
}