using BouncerBot.Rest.Models;

namespace BouncerBot.Rest;
partial class MouseHuntRestClient
{
    public async Task<Corkboard> GetCorkboardAsync(uint mhId, CancellationToken cancellationToken = default)
    {
        var userSnuId = await GetUserSnuIdAsync(mhId, cancellationToken);

        var content = new FormUrlEncodedContent([
            new ("sn_user_id", userSnuId.SnUserId)
        ]);

        return await SendRequestAsync<Corkboard>(HttpMethod.Post, content, "get/corkboard/profile", cancellationToken);
    }
}
