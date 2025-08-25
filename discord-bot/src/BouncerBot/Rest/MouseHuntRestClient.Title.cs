using BouncerBot.Rest.Models;
using Microsoft.Extensions.Caching.Memory;

namespace BouncerBot.Rest;
public partial class MouseHuntRestClient
{

    public async Task<Title[]> GetTitlesAsync(CancellationToken cancellationToken = default)
    {
        if (_memoryCache.TryGetValue<Title[]>("MouseHuntTitles", out var titles))
        {
            return titles!;
        }


        titles = await SendRequestAsync<Title[]>(HttpMethod.Post, null, "get/title", cancellationToken);

        if (titles.Length > 0)
        {
            _memoryCache.Set("MouseHuntTitles", titles);
        }

        return titles;
    }
}
