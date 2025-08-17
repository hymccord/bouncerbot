using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

using BouncerBot.Db;
using BouncerBot.Rest.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BouncerBot.Rest;

public interface IMouseHuntRestClient
{
    Task<Corkboard> GetCorkboardAsync(uint mhId, CancellationToken cancellationToken = default);
    Task<User> GetMeAsync(CancellationToken cancellationToken = default);
    Task<Title[]> GetTitlesAsync(CancellationToken cancellationToken = default);
    Task<UserItemCategoryCompletion> GetUserLocationStatsAsync(uint mhId, CancellationToken cancellationToken = default);
    Task<UserMouseStatistics> GetUserMiceAsync(uint mhId, CancellationToken cancellationToken = default);
    Task<UserItemCategoryCompletion> GetUserProfileItems(uint mhId, CancellationToken cancellationToken = default);
    Task<UserSnuIdInfo> GetUserSnuIdAsync(uint mhId, CancellationToken cancellationToken = default);
    Task<UserTitle> GetUserTitleAsync(uint mhId, CancellationToken cancellationToken = default);
    Task<bool> IsEggMaster(uint mhId, CancellationToken cancellationToken = default);
    Task<LoginDetails> LoginAsync(string username, string password, CancellationToken cancellationToken = default);
    Task<bool> SolvePuzzleAsync(string code, CancellationToken cancellationToken = default);
    Task StartAsync(CancellationToken cancellationToken);
    Task StopAsync(CancellationToken cancellationToken);
}

public partial class MouseHuntRestClient : IMouseHuntRestClient
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
    {
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    };
    private readonly ILogger<MouseHuntRestClient> _logger;
    private readonly IOptions<MouseHuntRestClientOptions> _options;
    private readonly IOptions<BouncerBotOptions> _bouncerBotOptions;
    private readonly RestRequestHandler _requestHandler;
    private readonly IDbContextFactory<BouncerBotDbContext> _dbContextFactory;

    private List<KeyValuePair<string, string>> _defaultFormData = [
        new ("sn", "Hitgrab"),
        new ("hg_is_ajax", "1"),
        ];

    public MouseHuntRestClient(
        ILogger<MouseHuntRestClient> logger,
        IOptions<MouseHuntRestClientOptions> options,
        IOptions<BouncerBotOptions> bouncerBotOptions,
        IDbContextFactory<BouncerBotDbContext> dbContextFactory)
    {
        _logger = logger;
        _options = options;
        _bouncerBotOptions = bouncerBotOptions;
        _dbContextFactory = dbContextFactory;

        _requestHandler = new RestRequestHandler(bouncerBotOptions);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("MouseHuntRestClient Login. Endpoint is {Url}", _bouncerBotOptions.Value.MouseHuntUrl);

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
        catch (RestException ex) when ((int)ex.StatusCode >= 500 && (int)ex.StatusCode < 600)
        {
            _logger.LogWarning(ex, "Failed to login to MouseHunt API. Status code: {StatusCode}. Retrying in 1 minute.", ex.StatusCode);

            // Try to login again in a minute
            _ = Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);
                await StartAsync(cancellationToken);
            }, cancellationToken);

            return;
        }
        catch (RestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized || ex.StatusCode == System.Net.HttpStatusCode.Forbidden)
        {
            // If we get a 401 or 403, we need to re-login
            _logger.LogInformation(ex, "Unauthorized or forbidden access to MouseHunt API. Status code: {StatusCode}. Attempting to re-login.", ex.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "An unexpected error occurred while starting MouseHuntRestClient. {Message}", ex.Message);
            throw;
        }

        var token = await LoginAsync(_options.Value.Username, _options.Value.Password, cancellationToken);

        await SaveSessionToken(token, cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task<T> SendRequestAsync<T>(HttpMethod method, string route, CancellationToken cancellationToken = default)
    {
        return SendRequestAsync<T>(method, null, route, cancellationToken);
    }

    public Task<T> SendRequestAsync<T>(HttpMethod method, HttpContent? content, string route, CancellationToken cancellationToken = default)
    {
        var url = $"api/{route}";
        return SendRequestAsync<T>(CreateMessage, cancellationToken);

        HttpRequestMessage CreateMessage()
        {
            HttpRequestMessage requestMessage = new(method, url)
            {
                Content = content,
            };

            return requestMessage;
        }
    }

    private async Task<T> SendDesktopRequestAsync<T>(HttpMethod method, IList<KeyValuePair<string, string>> formData, string route, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage? response = null;
        for (var tries = 0; tries < 2; tries++)
        {
            response = await _requestHandler.SendAsync(CreateMessage(), cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken).ConfigureAwait(false);

                var hgResponse = data.Deserialize<HgResponse>(_jsonSerializerOptions);

                if (hgResponse.User.HasPuzzle)
                {
                    throw new PuzzleException($"Puzzle encountered for endpoint {route}");
                }

                var popupMessage = hgResponse.MessageData["popup"].MessageCount > 0;
                //if (popupMessage == "Your session has expired.")
                if (popupMessage)
                {
                    Debugger.Break();
                    await RefreshSessionAsync();
                    continue;
                }

                return data.Deserialize<T>(_jsonSerializerOptions);
            }
        }

        _logger.LogError("Desktop request to {Route} failed with status code {StatusCode}. Reason: {Reason}", route, response!.StatusCode, response.ReasonPhrase);

        throw new Exception($"MouseHunt API request unsuccessful. Status: {response.StatusCode}");

        HttpRequestMessage CreateMessage()
        {
            HttpRequestMessage requestMessage = new(method, route)
            {
                Content = new FormUrlEncodedContent([
                    .._defaultFormData,
                    ..formData
                ])
            };

            return requestMessage;
        }

        async Task RefreshSessionAsync()
        {
            await _requestHandler.SendAsync(new HttpRequestMessage(HttpMethod.Get, "camp.php")
            {
                Content = new FormUrlEncodedContent(_defaultFormData)
            }, cancellationToken);
        }
    }

    private async Task<T> SendRequestAsync<T>(Func<HttpRequestMessage> createMessage, CancellationToken cancellationToken = default)
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
            RestError error;
            try
            {
                error = (await JsonSerializer.DeserializeAsync<RestError>(await content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false), cancellationToken: cancellationToken).ConfigureAwait(false))!;
            }
            catch (JsonException)
            {
                throw new RestException(response.StatusCode, response.ReasonPhrase);
            }
            throw new RestException(response.StatusCode, response.ReasonPhrase, error);
        }
        else
        {
            throw new RestException(response.StatusCode, response.ReasonPhrase);
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

    private static async Task SaveSessionToken(LoginDetails loginDetails, CancellationToken cancellationToken = default)
    {
        var tokenFilePath = Path.Combine(Environment.CurrentDirectory, "mousehunt_token.json");

        try
        {
            var tokenJson = JsonSerializer.Serialize(loginDetails);
            await File.WriteAllTextAsync(tokenFilePath, tokenJson, cancellationToken);
        }
        catch
        {
            // Log the exception but don't fail the startup process
            // The token is still valid in memory for this session
        }
    }
}
