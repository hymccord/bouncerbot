using BouncerBot.Rest.Models;

namespace BouncerBot.Rest;
public partial class MouseHuntRestClient
{

    public async Task<Title[]> GetTitlesAsync(CancellationToken cancellationToken = default)
    {
        return await SendRequestAsync<Title[]>(HttpMethod.Post, null, "get/title", cancellationToken);
    }
}
