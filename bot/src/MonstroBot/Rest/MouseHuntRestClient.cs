using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MonstroBot.Rest;
public class MouseHuntRestClient
{
    private RestRequestHandler _requestHandler;

    public MouseHuntRestClient(
        ILogger<MouseHuntRestClient> logger,
        IOptions<MouseHuntRestClientOptions> options)
    {
        _requestHandler = new RestRequestHandler();
    }

    internal async Task StartAsync(CancellationToken cancellationToken)
    {
        //var response = await _requestHandler.SendAsync(new HttpRequestMessage(HttpMethod.Post, "api/info")
        //{
        //    Version = new Version(2, 0),
        //    VersionPolicy = HttpVersionPolicy.RequestVersionOrHigher,
        //}, cancellationToken);
    }

    internal async Task StopAsync(CancellationToken cancellationToken)
    {

    }
}

internal class RestRequestHandler
{
    private readonly CookieContainer _cookieContainer;
    private readonly HttpClient _httpClient;

    public RestRequestHandler()
    {
        _cookieContainer = new CookieContainer();
        var socketHandler = new SocketsHttpHandler()
        {
            CookieContainer = _cookieContainer,
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli,
        };

        _httpClient = new HttpClient(socketHandler)
        {
            BaseAddress = new Uri("https://www.mousehuntgame.com/"),
        };

        _httpClient.DefaultRequestHeaders.Add("User-Agent", "MonstroBot/1.0 (Discord: Xellis)");
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json, text/javascript, */*; q=0.01");
        _httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
    }

    public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => _httpClient.SendAsync(request, cancellationToken);
}