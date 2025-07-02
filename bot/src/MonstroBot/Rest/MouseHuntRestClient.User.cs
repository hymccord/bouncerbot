using System.Text.Json;

using MonstroBot.Rest.Models;

namespace MonstroBot.Rest;
partial class MouseHuntRestClient
{
    public async Task<User> GetMeAsync(CancellationToken cancellationToken = default)
    {
        var response = await SendRequestAsync<User>(HttpMethod.Post, "get/user/me", cancellationToken);

        return response;
    }

    public async Task<UserSnuIdInfo> GetUserSnuId(uint mhId, CancellationToken cancellationToken = default)
    {
        return await SendRequestAsync<UserSnuIdInfo>(HttpMethod.Post, $"get/usersnuid/{mhId}", cancellationToken);
    }
}
