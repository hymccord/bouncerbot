using BouncerBot.Rest.Models;
using Microsoft.Extensions.Caching.Memory;

namespace BouncerBot.Rest;
public partial class MouseHuntRestClient
{
    public async Task<Mouse[]> GetAllMiceAsync(CancellationToken cancellationToken = default)
    {
        const string CacheKey = "mousehunt_all_mice";
        if (_memoryCache.TryGetValue<Mouse[]>(CacheKey, out var mice))
        {
            return mice!;
        }

        mice = await SendRequestAsync<Mouse[]>(HttpMethod.Post, null, "get/mouse/all", cancellationToken);

        if (mice.Length > 0)
        {
            _memoryCache.Set(CacheKey, mice, TimeSpan.FromMinutes(15));
        }

        return mice;
    }
}
