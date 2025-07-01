using System.Text.Json.Serialization;

namespace MonstroBot.Rest.Models;
public class User
{
    [JsonPropertyName("uh")]
    public string UniqueHash { get; set; } = string.Empty;
}
