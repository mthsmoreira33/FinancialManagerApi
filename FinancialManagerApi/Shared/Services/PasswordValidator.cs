using System.Text.RegularExpressions;
using FinancialManagerApi.Models;
using Microsoft.AspNetCore.Identity;

namespace FinancialManagerApi.Services
{
    public static class PasswordValidator
    {
        private static readonly PasswordHasher<User> _passwordHasher = new PasswordHasher<User>();
        public static bool IsValidPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password)) return false;

            var passwordRegex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$");
            var result = passwordRegex.IsMatch(password);
            return result;
        }
        
        public static bool VerifyPassword(User user, string hashedPassword, string providedPassword)
        {
            ArgumentNullException.ThrowIfNull(user);

            var result = _passwordHasher.VerifyHashedPassword(user, hashedPassword, providedPassword);
            return result == PasswordVerificationResult.Success;
        }

    }
}