using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using MonstroBot.Rest.Models;

namespace MonstroBot.Rest;
public partial class MouseHuntRestClient
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    private readonly IOptions<MouseHuntRestClientOptions> _options;
    private readonly RestRequestHandler _requestHandler;

    private List<KeyValuePair<string, string>> _defaultFormData = [
        new ("sn", "Hitgrab"),
        new ("hg_is_ajax", "1"),
        ];

    public MouseHuntRestClient(
        ILogger<MouseHuntRestClient> logger,
        IOptions<MouseHuntRestClientOptions> options)
    {
        _requestHandler = new RestRequestHandler();
        _options = options;
    }

    internal async Task StartAsync(CancellationToken cancellationToken)
    {
        await TryLoadSessionToken(cancellationToken);

        try
        {
            var me = await GetMeAsync(cancellationToken);

            if (me is not null)
            {
                _defaultFormData.Add(new KeyValuePair<string, string>("uh", me.UniqueHash));
            }

            return;
        }
        catch
        { }

        LoginDetails token = await LoginAsync(_options.Value.Username, _options.Value.Password, cancellationToken);

        await SaveSessionToken(token, cancellationToken);
    }

    internal async Task StopAsync(CancellationToken cancellationToken)
    {

    }

    public Task<T> SendRequestAsync<T>(HttpMethod method, string route, CancellationToken cancellationToken = default)
    {
        return SendRequestAsync<T>(method, null, route, cancellationToken);
    }

    public Task<T> SendRequestAsync<T>(HttpMethod method, HttpContent? content, string route, CancellationToken cancellationToken = default)
    {
        var url = $"api/{route}";
        return SendRequestAsync<T>(url, CreateMessage, cancellationToken);

        HttpRequestMessage CreateMessage()
        {
            HttpRequestMessage requestMessage = new(method, url)
            {
                Content = content,
            };

            return requestMessage;
        }
    }

    private async Task<T> SendRequestAsync<T>(string url, Func<HttpRequestMessage> createMessage, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response;
        try
        {
            response = await _requestHandler.SendAsync(createMessage(), cancellationToken);
        }
        catch
        {
            throw;
        }

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<T>(_jsonSerializerOptions, cancellationToken).ConfigureAwait(false);
        }

        var content = response.Content;

        if (content.Headers.ContentType is { MediaType: "application/json" })
        {
            throw new Exception($"Request to {url} failed with status code {response.StatusCode}: {await content.ReadAsStringAsync(cancellationToken)}");
        }
        else
        {
            throw new Exception($"Request to {url} failed with status code {response.StatusCode}: {response.ReasonPhrase}");
        }

    }

    private async Task TryLoadSessionToken(CancellationToken cancellationToken = default)
    {
        var tokenFilePath = Path.Combine(Environment.CurrentDirectory, "mousehunt_token.json");
        if (File.Exists(tokenFilePath))
        {
            try
            {
                var loginDetailsJson = await File.ReadAllTextAsync(tokenFilePath, cancellationToken);
                var token = JsonSerializer.Deserialize<LoginDetails>(loginDetailsJson);

                if (!string.IsNullOrEmpty(token?.LoginToken))
                {
                    _requestHandler.AddDefaultHeader("Cookie", $"HG_TOKEN={token.LoginToken}");
                }
            }
            catch
            {
                // If we can't read the token file, we just ignore it and proceed with the login flow
            }
        }
    }

    private async Task SaveSessionToken(LoginDetails loginDetails, CancellationToken cancellationToken = default)
    {
        var tokenFilePath = Path.Combine(Environment.CurrentDirectory, "mousehunt_token.json");

        try
        {
            var tokenJson = JsonSerializer.Serialize(loginDetails.LoginToken);
            await File.WriteAllTextAsync(tokenFilePath, tokenJson, cancellationToken);
        }
        catch
        {
            // Log the exception but don't fail the startup process
            // The token is still valid in memory for this session
        }
    }
}
