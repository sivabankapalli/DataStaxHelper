using System.Text;
using System.Text.Json;

namespace DataStax.AstraDB.Helper.DataApi.Rest;

/// <summary>
/// Represents a collection in Astra Data API (HTTP client version).
/// </summary>
public sealed class Collection<T> where T : class
{
    private readonly HttpClient _http;
    private readonly string _url;

    internal Collection(HttpClient http, string url)
    {
        _http = http;
        _url = url;
    }

    public async Task<string?> InsertOneAsync(T doc, CancellationToken ct = default)
    {
        var payload = new { insertOne = new { document = doc } };
        var resp = await PostAsync(payload, ct);
        return InsertIdExtractor.Extract(resp);
    }

    public async Task<IReadOnlyList<T>> FindAsync(object? filter = null, int? limit = null, CancellationToken ct = default)
    {
        var payload = new { find = new { filter, options = limit is null ? null : new { limit } } };
        var resp = await PostAsync(payload, ct);
        var env = JsonSerializer.Deserialize<FindEnvelope<T>>(resp, Database.JsonOpts);
        return env?.Data?.Documents ?? [];
    }

    public async Task<T?> FindOneAsync(object filter, CancellationToken ct = default)
    {
        var payload = new { findOne = new { filter } };
        var resp = await PostAsync(payload, ct);
        var env = JsonSerializer.Deserialize<FindOneEnvelope<T>>(resp, Database.JsonOpts);
        return env?.Data?.Document;
    }

    public async Task<UpdateOneResult> UpdateOneAsync(object filter, object update, bool upsert = false, CancellationToken ct = default)
    {
        var payload = new { updateOne = new { filter, update, options = new { upsert } } };
        var resp = await PostAsync(payload, ct);

        var parsed = OperationResultParser.ParseUpdateOne(resp);
        return new UpdateOneResult { UpdateOne = parsed };
    }

    public async Task<DeleteOneResult> DeleteOneAsync(object filter, CancellationToken ct = default)
    {
        var findCount = await ExistsAsync(filter, ct);

        var payload = new { deleteOne = new { filter } };
        var resp = await PostAsync(payload, ct);

        // Try to read deletedCount from "deleteOne", then "status", then "data"
        int deletedCount = TryParseDeletedCount(resp);

        // If the API returned -1, do a quick verification round-trip
        if (deletedCount == -1)
        {
            // try to verify: if the doc no longer exists, normalize to 1, else 0
            var exists = await ExistsAsync(filter, ct);
            deletedCount = exists ? 0 : 1;
        }

        return new DeleteOneResult
        {
            DeleteOne = new DeleteOneData { DeletedCount = deletedCount }
        };
    }

    private static int TryParseDeletedCount(string resp)
    {
        using var doc = System.Text.Json.JsonDocument.Parse(resp);
        var root = doc.RootElement;

        // deleteOne.deletedCount
        if (root.TryGetProperty("deleteOne", out var delObj) &&
            delObj.ValueKind == System.Text.Json.JsonValueKind.Object &&
            delObj.TryGetProperty("deletedCount", out var dc) &&
            dc.TryGetInt32(out var val1))
        {
            return val1;
        }

        // status.deletedCount
        if (root.TryGetProperty("status", out var status) &&
            status.ValueKind == System.Text.Json.JsonValueKind.Object &&
            status.TryGetProperty("deletedCount", out var dc2) &&
            dc2.TryGetInt32(out var val2))
        {
            return val2;
        }

        // data.deletedCount (rare)
        if (root.TryGetProperty("data", out var data) &&
            data.ValueKind == System.Text.Json.JsonValueKind.Object &&
            data.TryGetProperty("deletedCount", out var dc3) &&
            dc3.TryGetInt32(out var val3))
        {
            return val3;
        }

        // default when nothing matched
        return 0;
    }

    /// <summary>
    /// Verifies whether a document matching the filter still exists.
    /// </summary>
    private async Task<bool> ExistsAsync(object filter, CancellationToken ct)
    {
        var verifyPayload = new { findOne = new { filter } };
        var verifyResp = await PostAsync(verifyPayload, ct);

        using var doc = System.Text.Json.JsonDocument.Parse(verifyResp);
        var root = doc.RootElement;
        if (root.TryGetProperty("data", out var data) &&
            data.ValueKind == System.Text.Json.JsonValueKind.Object &&
            data.TryGetProperty("document", out var docEl))
        {
            // document present → still exists
            return docEl.ValueKind != System.Text.Json.JsonValueKind.Null &&
                   docEl.ValueKind != System.Text.Json.JsonValueKind.Undefined &&
                   docEl.ValueKind != System.Text.Json.JsonValueKind.Object
                       ? true // defensive: some shapes may inline a scalar, treat as exists
                       : docEl.ValueKind == System.Text.Json.JsonValueKind.Object; // typical object → exists
        }

        // no "data.document" → treat as not found
        return false;
    }



    private async Task<string> PostAsync(object body, CancellationToken ct)
    {
        var json = JsonSerializer.Serialize(body, Database.JsonOpts);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        using var resp = await _http.PostAsync(_url, content, ct);

        var text = await resp.Content.ReadAsStringAsync(ct);
        if (!resp.IsSuccessStatusCode)
            throw new HttpRequestException($"Data API error {(int)resp.StatusCode}: {text}");

        return text;
    }
}
