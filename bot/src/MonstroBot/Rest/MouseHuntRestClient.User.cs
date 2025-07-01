using System.Text.Json;

using MonstroBot.Rest.Models;

namespace MonstroBot.Rest;
partial class MouseHuntRestClient
{
    public async Task<User> GetMeAsync(CancellationToken cancellationToken = default)
    {
        var response = await SendRequestAsync(HttpMethod.Post, "api/get/user/me", cancellationToken);

        return response.RootElement.Deserialize<User>(_jsonSerializerOptions) ?? throw new JsonException();
    }
}
