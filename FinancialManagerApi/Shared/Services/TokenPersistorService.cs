using System.Collections.Concurrent;
using System.Text.Json;

namespace FinancialManagerApi.Services;

public interface ITokenPersistorService
{
    Task PersistTokenAsync(string userEmail, string token);
    Task<string?> GetTokenAsync(string userEmail);
    Task RemoveTokenAsync(string userEmail);
}

public abstract class TokenPersistorService : ITokenPersistorService
{
    private readonly ConcurrentDictionary<string, string> _inMemoryStore = new();

    public async Task PersistTokenAsync(string userEmail, string token)
    {
        var tokenData = new TokenData
        {
            Token = token,
            Expiration = DateTime.UtcNow.AddMinutes(30)
        };

        var jsonToken = JsonSerializer.Serialize(tokenData);
        
        _inMemoryStore[userEmail] = jsonToken;

        await Task.CompletedTask;
    }

    public async Task<string?> GetTokenAsync(string userEmail)
    {
        if (!_inMemoryStore.TryGetValue(userEmail, out var jsonToken))
            return null;

        var tokenData = JsonSerializer.Deserialize<TokenData>(jsonToken);

        if (tokenData != null && DateTime.UtcNow <= tokenData.Expiration) return tokenData.Token;
        await RemoveTokenAsync(userEmail);
        return null;

    }

    public async Task RemoveTokenAsync(string userEmail)
    {
        _inMemoryStore.TryRemove(userEmail, out _);
        await Task.CompletedTask;
    }

    private class TokenData
    {
        public string? Token { get; set; }
        public DateTime Expiration { get; set; }
    }
}