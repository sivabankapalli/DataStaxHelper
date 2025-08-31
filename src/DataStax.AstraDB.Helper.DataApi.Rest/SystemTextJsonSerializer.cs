using System.Text.Json;
using DataStax.AstraDB.Helper.Abstractions;

namespace DataStax.AstraDB.Helper.DataApi.Rest;

/// <summary>
/// JSON serializer based on System.Text.Json.
/// Default implementation for the REST client.
/// </summary>
public class SystemTextJsonSerializer : IJsonSerializer
{
    public string Serialize<T>(T obj) =>
        JsonSerializer.Serialize(obj);

    public T Deserialize<T>(string json) =>
        JsonSerializer.Deserialize<T>(json)!;
}
