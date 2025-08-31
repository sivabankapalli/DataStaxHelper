using System.Text.Json;

namespace DataStax.AstraDB.Helper.DataApi.Rest;

internal static class OperationResultParser
{
    public static UpdateOneData ParseUpdateOne(string resp)
    {
        using var doc = JsonDocument.Parse(resp);
        var root = doc.RootElement;

        if (TryGetObject(root, "updateOne", out var obj) ||
            TryGetObject(root, "status", out obj) ||
            TryGetObject(root, "data", out obj))
        {
            var result = new UpdateOneData();
            if (obj.TryGetProperty("matchedCount", out var matched) && matched.TryGetInt32(out var m))
                result.MatchedCount = m;
            if (obj.TryGetProperty("modifiedCount", out var modified) && modified.TryGetInt32(out var md))
                result.ModifiedCount = md;
            if (obj.TryGetProperty("upsertedId", out var up) && up.ValueKind == JsonValueKind.String)
                result.UpsertedId = up.GetString();
            return result;
        }

        return new UpdateOneData();
    }

    public static int ParseDeleteOne(string resp)
    {
        using var doc = JsonDocument.Parse(resp);
        var root = doc.RootElement;

        if (TryGetObject(root, "deleteOne", out var obj) ||
            TryGetObject(root, "status", out obj) ||
            TryGetObject(root, "data", out obj))
        {
            if (obj.TryGetProperty("deletedCount", out var dc) && dc.TryGetInt32(out var val))
                return val;
        }

        return 0;
    }

    private static bool TryGetObject(JsonElement parent, string name, out JsonElement obj)
    {
        if (parent.TryGetProperty(name, out obj) && obj.ValueKind == JsonValueKind.Object)
            return true;

        obj = default;
        return false;
    }
}
