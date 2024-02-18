using System.Text.Json;
using System.Text.Json.Serialization;
using UNOEngine;

namespace DAL;

public class GameCardConverter : JsonConverter<GameCard>
{
    public override GameCard Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;
        
        var suite = root.GetProperty("CardSuite").GetInt16();
        var value = root.GetProperty("CardValue").GetInt16();
        
        return new GameCard(suite, value);
    }

    public override void Write(Utf8JsonWriter writer, GameCard value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("CardSuite", (int)value.CardSuite);
        writer.WriteNumber("CardValue", (int)value.CardValue);
        writer.WriteEndObject();
    }
}