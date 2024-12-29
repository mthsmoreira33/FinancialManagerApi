using System.Collections.Concurrent;
using FinancialManagerApi.Data;
using Microsoft.EntityFrameworkCore;

namespace FinancialManagerApi.Services
{
    public interface ITokenBlacklistService
    {
        Task AddToBlacklistAsync(string token, DateTime expiration);
        Task<bool> IsBlacklistedAsync(string token);
        void CleanupExpiredTokens();
        
        Task<bool>? IsUserBlacklisted(Guid userId);
    }

    public class TokenBlacklistService : ITokenBlacklistService
    {
        private readonly ConcurrentDictionary<string, DateTime> _blacklist = new();
        private readonly AppDbContext _context;

        public async Task AddToBlacklistAsync(string token, DateTime expiration)
        {
            _blacklist[token] = expiration;
            await Task.CompletedTask;
        }

        public async Task<bool> IsBlacklistedAsync(string token)
        {
            if (!_blacklist.TryGetValue(token, out var expiration)) return await Task.FromResult(false);

            if (DateTime.UtcNow <= expiration) return await Task.FromResult(true);

            _blacklist.TryRemove(token, out _);
            return await Task.FromResult(false);
        }

        public void CleanupExpiredTokens()
        {
            var expiredTokens = _blacklist
                .Where(kvp => DateTime.UtcNow > kvp.Value)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var token in expiredTokens)
            {
                _blacklist.TryRemove(token, out _);
            }
        }
        
        public async Task<bool> IsUserBlacklisted(Guid userId)
        {
            // Example: Check if the user is blacklisted in the database
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            return user?.IsBlacklisted ?? false; // Assume there's a `IsBlacklisted` property on User
        }
    }
}