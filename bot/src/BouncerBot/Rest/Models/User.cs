using System.Text.Json.Serialization;

namespace BouncerBot.Rest.Models;
public class User
{
    [JsonPropertyName("uh")]
    public string UniqueHash { get; set; } = string.Empty;
}
