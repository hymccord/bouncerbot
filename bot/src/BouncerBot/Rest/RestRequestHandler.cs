using System.Net;

namespace BouncerBot.Rest;
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
            BaseAddress = new Uri("https://bouncerbot.hymccord.workers.dev/"),
        };

        _httpClient.DefaultRequestHeaders.Add("User-Agent", "BouncerBot/1.0 (Discord: Xellis)");
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json, text/javascript, */*; q=0.01");
        _httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
    }

    public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => _httpClient.SendAsync(request, cancellationToken);

    public void AddDefaultHeader(string name, params IEnumerable<string> values)
    {
        _httpClient.DefaultRequestHeaders.Add(name, values);
    }
}
