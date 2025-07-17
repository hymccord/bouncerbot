using System.Text.Json.Serialization;

using BouncerBot.Rest.Converters;

namespace BouncerBot.Rest.Models;
public class Title
{
    public uint TitleId { get; set; }

    [JsonPropertyName("name_male")]
    [JsonConverter(typeof(RankJsonConverter))]
    public Rank Name { get; set; }

    public uint DisplayOrder { get; set; }
}

public class UserTitle
{
    public uint TitleId { get; set; }
}
