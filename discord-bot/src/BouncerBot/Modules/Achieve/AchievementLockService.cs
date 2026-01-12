using Microsoft.Extensions.Caching.Memory;

internal class AchievementLockService(IMemoryCache cache) : IAchievementLockService
{
    private static string GetCacheKey(ulong guildId) => $"achievement_lock_{guildId}";

    public Task<bool> IsLockedAsync(ulong guildId)
    {
        return Task.FromResult(cache.TryGetValue(GetCacheKey(guildId), out _));
    }

    public Task<DateTimeOffset?> GetLockExpirationAsync(ulong guildId)
    {
        return Task.FromResult<DateTimeOffset?>(
            cache.TryGetValue<DateTimeOffset>(GetCacheKey(guildId), out var expiration) 
                ? expiration 
                : null
        );
    }

    public Task SetLockAsync(ulong guildId, TimeSpan duration)
    {
        var expiration = DateTimeOffset.UtcNow.Add(duration);
        cache.Set(GetCacheKey(guildId), expiration, duration);
        return Task.CompletedTask;
    }

    public Task RemoveLockAsync(ulong guildId)
    {
        cache.Remove(GetCacheKey(guildId));
        return Task.CompletedTask;
    }
}

public interface IAchievementLockService
{
    Task<DateTimeOffset?> GetLockExpirationAsync(ulong guildId);
    Task<bool> IsLockedAsync(ulong guildId);
    Task RemoveLockAsync(ulong guildId);
    Task SetLockAsync(ulong guildId, TimeSpan duration);
}
