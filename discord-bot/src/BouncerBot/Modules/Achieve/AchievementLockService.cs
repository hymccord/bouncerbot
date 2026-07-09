using BouncerBot;
using Microsoft.Extensions.Caching.Memory;

internal class AchievementLockService(IMemoryCache cache) : IAchievementLockService
{
    private static string GetCacheKey(ulong guildId) => $"achievement_lock_{guildId}";

    public bool IsAchievementLockable(AchievementRole achievement)
        => achievement switch
        {
            AchievementRole.Fabled => false,
            _ => false
        };

    public Task<bool> IsGuildLockedAsync(ulong guildId)
    {
        return Task.FromResult(cache.TryGetValue(GetCacheKey(guildId), out _));
    }

    public Task<DateTimeOffset?> GetGuildLockExpirationAsync(ulong guildId)
    {
        return Task.FromResult<DateTimeOffset?>(
            cache.TryGetValue<DateTimeOffset>(GetCacheKey(guildId), out var expiration) 
                ? expiration 
                : null
        );
    }

    public Task SetGuildLockDurationAsync(ulong guildId, TimeSpan duration)
    {
        var expiration = DateTimeOffset.UtcNow.Add(duration);
        cache.Set(GetCacheKey(guildId), expiration, duration);
        return Task.CompletedTask;
    }

    public Task RemoveGuildLockAsync(ulong guildId)
    {
        cache.Remove(GetCacheKey(guildId));
        return Task.CompletedTask;
    }
}

public interface IAchievementLockService
{
    Task<DateTimeOffset?> GetGuildLockExpirationAsync(ulong guildId);
    bool IsAchievementLockable(AchievementRole achievement);
    Task<bool> IsGuildLockedAsync(ulong guildId);
    Task RemoveGuildLockAsync(ulong guildId);
    Task SetGuildLockDurationAsync(ulong guildId, TimeSpan duration);
}
