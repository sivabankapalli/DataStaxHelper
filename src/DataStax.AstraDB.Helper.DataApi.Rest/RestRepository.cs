using DataStax.AstraDB.Helper.Abstractions;

namespace DataStax.AstraDB.Helper.DataApi.Rest;

/// <summary>
/// A repository implementation backed by the REST Data API client.
/// </summary>
/// <typeparam name="T">The document type.</typeparam>
public class RestRepository<T> : IRepository<T> where T : class
{
    private readonly IDataApiClient _client;
    private readonly string _collection;

    public RestRepository(IDataApiClient client, string collection)
    {
        _client = client;
        _collection = collection;
    }

    public Task CreateAsync(T document) =>
        _client.InsertOneAsync(_collection, document);

    public Task<IReadOnlyList<T>> ReadAsync(object? filter = null) =>
        _client.FindManyAsync<T>(_collection, filter);

    public Task<long> UpdateAsync(object filter, object update) =>
        _client.UpdateOneAsync(_collection, filter, update);

    public Task<long> DeleteAsync(object filter) =>
        _client.DeleteOneAsync(_collection, filter);
}
