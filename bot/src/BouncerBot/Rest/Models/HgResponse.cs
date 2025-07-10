using System.Text.Json.Serialization;

namespace BouncerBot.Rest.Models;
internal class HgResponse
{
    [JsonPropertyName("messageData")]
    public required Dictionary<string, MessageCategoryData> MessageData { get; set; }

    public int Success { get; set; }
}

internal class MessageCategoryData
{
    public Message[]? Messages { get; set; }
}

internal class Message
{
    [JsonPropertyName("messageData")]
    public required MessageData MessageData { get; set; }
}

internal class MessageData
{
    public required string Title { get; set; }
    public required string Body { get; set; }
}
