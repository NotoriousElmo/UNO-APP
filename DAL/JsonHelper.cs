using System.Text.Json;

namespace DAL;

public static class JsonHelper
{
    public static readonly JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions()
    {
        WriteIndented = true,
        Converters = { new GameCardConverter() },
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
}