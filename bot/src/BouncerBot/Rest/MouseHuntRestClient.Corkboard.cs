using BouncerBot.Rest.Models;

namespace BouncerBot.Rest;
partial class MouseHuntRestClient
{
    public async Task<Corkboard> GetCorkboardAsync(string userSnuId, CancellationToken cancellationToken = default)
    {
        var content = new FormUrlEncodedContent([
            new ("sn_user_id", userSnuId)
        ]);

        return await SendRequestAsync<Corkboard>(HttpMethod.Post, content, "get/corkboard/profile", cancellationToken);
    }
}
