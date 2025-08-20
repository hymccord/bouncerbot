using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;

namespace BouncerBot.Services;

public interface IMouseRipService
{
    Task<MouseRipMouse[]?> GetAllMiceAsync();
}

public class MouseRipService : IMouseRipService
{
    private static readonly JsonSerializerOptions s_jsonSerializerOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    private const string CacheKey = "all_mice";
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _memoryCache;

    public MouseRipService(HttpClient httpClient, IMemoryCache memoryCache)
    {
        _httpClient = httpClient;
        _memoryCache = memoryCache;

        httpClient.BaseAddress = new Uri("https://api.mouse.rip/");
        httpClient.DefaultRequestHeaders.Add("User-Agent", "BouncerBot/1.0 (Discord: Xellis)");
    }

    public async Task<MouseRipMouse[]?> GetAllMiceAsync()
    {
        if (_memoryCache.TryGetValue(CacheKey, out MouseRipMouse[]? cachedMice))
        {
            return cachedMice;
        }

        var mice = await _httpClient.GetFromJsonAsync<MouseRipMouse[]>("mice", s_jsonSerializerOptions);
        
        if (mice != null)
        {
            _memoryCache.Set(CacheKey, mice, TimeSpan.FromMinutes(5));
        }

        return mice;
    }
}

public class MouseRipMouse
{
    public uint Id { get; set; }
    public string Name { get; set; } = null!;
    public string Group { get; set; } = null!;
    public Dictionary<MouseRipEffectivenesses, double> Effectivenesses { get; set; } = null!;
}

public enum MouseRipEffectivenesses
{
    Power, // ignore this
    Arcane,
    Draconic,
    Forgotten,
    Hydro,
    Physical,
    Shadow,
    Tactical,
    Law,
    Rift,
}
