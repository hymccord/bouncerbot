using System.Text.Json;
using System.Text.Json.Serialization;

namespace BouncerBot.Rest.Converters;
internal class RankJsonConverter : JsonConverter<Rank>
{
    public override Rank Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException($"Unexpected token parsing Rank. Expected a string, got {reader.TokenType}.");
        }

        var rankString = reader.GetString()?.Replace(" ", "");

        if (Enum.TryParse<Rank>(rankString, ignoreCase: true, out var rank))
        {
            return rank;
        }

        throw new JsonException($"Invalid Rank value: {rankString}");
    }

    public override void Write(Utf8JsonWriter writer, Rank value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}
