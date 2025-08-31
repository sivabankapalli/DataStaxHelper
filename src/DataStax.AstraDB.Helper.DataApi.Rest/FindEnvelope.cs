using System.Text.Json;
using System.Text.Json.Serialization;

namespace DataStax.AstraDB.Helper.DataApi.Rest;

public sealed class FindEnvelope<T> where T : class
{
    [JsonPropertyName("data")] public FindDataNode<T>? Data { get; set; }
    [JsonPropertyName("status")] public object? Status { get; set; }
}
public sealed class FindDataNode<T> where T : class
{
    [JsonPropertyName("documents")] public List<T>? Documents { get; set; }
    [JsonPropertyName("pageState")] public string? PageState { get; set; }
}

public sealed class FindOneEnvelope<T> where T : class
{
    [JsonPropertyName("data")] public FindOneDataNode<T>? Data { get; set; }
    [JsonPropertyName("status")] public object? Status { get; set; }
}
public sealed class FindOneDataNode<T> where T : class
{
    [JsonPropertyName("document")] public T? Document { get; set; }
}

public sealed class UpdateOneResult
{
    [JsonPropertyName("updateOne")] public UpdateOneData? UpdateOne { get; set; }
}
public sealed class UpdateOneData
{
    [JsonPropertyName("matchedCount")] public int MatchedCount { get; set; }
    [JsonPropertyName("modifiedCount")] public int ModifiedCount { get; set; }
    [JsonPropertyName("upsertedId")] public string? UpsertedId { get; set; }
}

public sealed class DeleteOneResult
{
    [JsonPropertyName("deleteOne")] public DeleteOneData? DeleteOne { get; set; }
}
public sealed class DeleteOneData
{
    [JsonPropertyName("deletedCount")] public int DeletedCount { get; set; }
}

/// <summary>
/// Utility for extracting inserted id from multiple possible response formats.
/// </summary>
internal static class InsertIdExtractor
{
    public static string? Extract(string resp)
    {
        using var doc = System.Text.Json.JsonDocument.Parse(resp);
        var root = doc.RootElement;

        if (root.TryGetProperty("status", out var status) &&
            status.TryGetProperty("insertedIds", out var ids) &&
            ids.ValueKind == System.Text.Json.JsonValueKind.Array)
        {
            foreach (var outer in ids.EnumerateArray())
            {
                if (outer.ValueKind == System.Text.Json.JsonValueKind.String)
                {
                    // case: ["id1", "id2"]
                    return outer.GetString();
                }
                else if (outer.ValueKind == System.Text.Json.JsonValueKind.Array)
                {
                    // case: [["bob@example.com"]]
                    foreach (var inner in outer.EnumerateArray())
                    {
                        if (inner.ValueKind == System.Text.Json.JsonValueKind.String)
                            return inner.GetString();
                    }
                }
            }
        }

        if (root.TryGetProperty("data", out var data) &&
            data.TryGetProperty("documentIds", out var did) &&
            did.ValueKind == JsonValueKind.Array)
        {
            foreach (var idEl in did.EnumerateArray())
            {
                if (idEl.ValueKind == JsonValueKind.String)
                    return idEl.GetString();
            }
        }


        return null;
    }
}
