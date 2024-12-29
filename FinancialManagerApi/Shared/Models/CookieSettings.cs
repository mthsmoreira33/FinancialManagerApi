namespace FinancialManagerApi.Models;

public class CookieSettings
{
    public string Name { get; set; }
    public bool Secure { get; set; }
    public bool HttpOnly { get; set; }
    public string SameSite { get; set; }
    public int ExpirationMinutes { get; set; }
}
