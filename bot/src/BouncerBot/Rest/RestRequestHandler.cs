using System.Net;
using Microsoft.Extensions.Options;

namespace BouncerBot.Rest;
internal sealed class RestRequestHandler : IDisposable
{
    private readonly TimeSpan _minTimeBetweenRequests = TimeSpan.FromMilliseconds(250);
    private readonly SemaphoreSlim _rateLimitSemaphore = new(1, 1);
    private readonly CookieContainer _cookieContainer = new();
    private readonly HttpClient _httpClient;

    private DateTime _lastRequestTime = DateTime.MinValue;

    public RestRequestHandler(IOptions<BouncerBotOptions> options)
    {
        var socketHandler = new SocketsHttpHandler()
        {
            CookieContainer = _cookieContainer,
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli,
        };

        _httpClient = new HttpClient(socketHandler)
        {
            BaseAddress = new Uri(options.Value.MouseHuntUrl),
        };

        _httpClient.DefaultRequestHeaders.Add("User-Agent", "BouncerBot/1.0 (Discord: Xellis)");
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json, text/javascript, */*; q=0.01");
        _httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
    }

    public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        await _rateLimitSemaphore.WaitAsync(cancellationToken);
        try
        {
            var timeSinceLastRequest = DateTime.UtcNow - _lastRequestTime;
            var delayNeeded = _minTimeBetweenRequests - timeSinceLastRequest;

            if (delayNeeded > TimeSpan.Zero)
            {
                await Task.Delay(delayNeeded, cancellationToken);
            }

            var response = await _httpClient.SendAsync(request, cancellationToken);
            
            _lastRequestTime = DateTime.UtcNow;
            
            return response;
        }
        finally
        {
            _rateLimitSemaphore.Release();
        }
    }

    public void AddDefaultHeader(string name, params IEnumerable<string> values)
    {
        _httpClient.DefaultRequestHeaders.Add(name, values);
    }

    public void Dispose()
    {
        _rateLimitSemaphore?.Dispose();
        _httpClient?.Dispose();
    }
}
