using System.Text.Json;
using System.Text.Json.Serialization;
using DataStax.AstraDB.Helper.Abstractions;

namespace DataStax.AstraDB.Helper.Scb;

public sealed class SystemTextJsonSerializer : IJsonSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false
    };

    public string Serialize<T>(T value) => JsonSerializer.Serialize(value, Options);
    public T? Deserialize<T>(string json) => JsonSerializer.Deserialize<T>(json, Options);
}
