using System.Text.Json;

using MonstroBot.Rest.Models;

namespace MonstroBot.Rest;
partial class MouseHuntRestClient
{
    public async Task<LoginDetails> LoginAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        var content = new FormUrlEncodedContent([
            new KeyValuePair<string, string>("account_name", username),
            new KeyValuePair<string, string>("password", password),
        ]);

        return await SendRequestAsync<LoginDetails>(HttpMethod.Post, content, "login", cancellationToken);
    }
}
