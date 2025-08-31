using DataStax.AstraDB.DataApi.Collections;
using DataStax.AstraDB.DataApi.Core;
using DataStax.AstraDB.DataApi.Core.Query;

namespace DataStax.AstraDB.Helper.DataApi.Sdk;

/// <summary>Repository over a typed Astra Data API collection.</summary>
public sealed class SdkRepository<TDocument> where TDocument : class
{
    private readonly Collection<TDocument> _collection;

    public SdkRepository(Database database, string collectionName)
        => _collection = database.GetCollection<TDocument>(collectionName);

    public SdkRepository(SdkDataApiClient client, string collectionName)
        : this(client.Database, collectionName) { }

    /// <summary>Insert a single document.</summary>
    public Task InsertOneAsync(TDocument doc) => _collection.InsertOneAsync(doc);

    /// <summary>Find one by filter.</summary>
    public Task<TDocument?> FindOneAsync(Filter<TDocument> filter)
        => _collection.FindOneAsync(filter);

    /// <summary>Find many by filter (optionally limited).</summary>
    public async Task<IReadOnlyList<TDocument>> FindAsync(Filter<TDocument> filter, int? limit = null)
    {
        var list = new List<TDocument>();
        var q = _collection.Find(filter);
        if (limit is int l) q = q.Limit(l);
        await foreach (var d in q) list.Add(d);
        return list;
    }

    /// <summary>
    /// Update one and return the modified count.
    /// Note: DataApi 2.0.1-beta does not expose a concrete UpdateOptions you can instantiate.
    /// Use the 2-arg overload (filter, update).
    /// </summary>
    public async Task<long> UpdateOneAsync(Filter<TDocument> filter, UpdateBuilder<TDocument> update)
    {
        var res = await _collection.UpdateOneAsync(filter, update);
        return res.ModifiedCount;
    }

    /// <summary>Delete one and return the deleted count.</summary>
    public async Task<long> DeleteOneAsync(Filter<TDocument> filter)
    {
        var res = await _collection.DeleteOneAsync(filter);
        return res.DeletedCount;
    }
}
