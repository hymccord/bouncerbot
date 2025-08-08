using System.Text.Json.Serialization;

namespace BouncerBot.Rest.Models;
internal class HgResponse
{
    public User User { get; set; } = null!;

    [JsonPropertyName("messageData")]
    public required Dictionary<string, MessageCategoryData> MessageData { get; set; }

    public int Success { get; set; }
}

internal class MessageCategoryData
{
    [JsonPropertyName("messageCount")]
    public required int MessageCount { get; set; }
    public Message[]? Messages { get; set; }
}

internal class Message
{
    //[JsonPropertyName("messageData")]
    //public required MessageData MessageData { get; set; }
}

internal class MessageData
{
}
