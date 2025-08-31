using System.Text.Json;
using System.Text.Json.Serialization;

namespace DataStax.AstraDB.Helper.DataApi.Rest;

/// <summary>
/// Represents a database namespace (keyspace).
/// </summary>
public sealed class Database
{
    internal static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly HttpClient _http;
    private readonly string _base;

    internal Database(HttpClient http, string endpoint, string keyspace)
    {
        if (string.IsNullOrWhiteSpace(keyspace))
            throw new ArgumentException("Keyspace is required", nameof(keyspace));

        _http = http;
        _base = $"{endpoint}/api/json/v1/{keyspace}";
    }

    public Collection<T> GetCollection<T>(string name) where T : class
        => new Collection<T>(_http, $"{_base}/{name}");
}
