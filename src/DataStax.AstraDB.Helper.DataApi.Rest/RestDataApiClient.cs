namespace DataStax.AstraDB.Helper.DataApi.Rest;

/// <summary>
/// Client for interacting with Astra Data API.
/// Wraps HttpClient and provides access to databases.
/// </summary>
public sealed class RestDataApiClient : IDisposable
{
    private readonly HttpClient _http;
    private readonly string _endpoint;

    public RestDataApiClient(string token, string endpoint)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Token is required", nameof(token));
        if (string.IsNullOrWhiteSpace(endpoint))
            throw new ArgumentException("Endpoint is required", nameof(endpoint));

        _endpoint = endpoint.TrimEnd('/');
        _http = new HttpClient();
        _http.DefaultRequestHeaders.Remove("Token");
        _http.DefaultRequestHeaders.Add("Token", token);
    }

    public Database GetDatabase(string keyspace) => new Database(_http, _endpoint, keyspace);

    public void Dispose() => _http.Dispose();
}
