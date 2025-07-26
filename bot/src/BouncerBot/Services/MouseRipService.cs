using System.Net.Http.Json;
using System.Text.Json;

namespace BouncerBot.Services;

public interface IMouseRipService
{
    Task<MouseRipMouse[]?> GetAllMiceAsync();
}

public class MouseRipService(HttpClient httpClient) : IMouseRipService
{
    private static readonly JsonSerializerOptions s_jsonSerializerOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public async Task<MouseRipMouse[]?> GetAllMiceAsync()
    {
        var mice = await httpClient.GetFromJsonAsync<MouseRipMouse[]>("mice", s_jsonSerializerOptions);

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
