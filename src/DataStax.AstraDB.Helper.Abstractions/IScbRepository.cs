namespace DataStax.AstraDB.Helper.Abstractions;

/// <summary>
/// Defines a generic repository abstraction for AstraDB collections.
/// A repository wraps an <see cref="IDataApiClient"/> and binds it to a single collection.
/// </summary>
/// <typeparam name="T">The document type. Must be a class.</typeparam>
public interface IScbRepository<T> where T : class
{
    Task UpsertAsync(T entity, CancellationToken ct = default);
    Task<T?> GetByIdAsync(object key, CancellationToken ct = default);
    Task<int> DeleteAsync(object key, CancellationToken ct = default);
    Task<IReadOnlyList<T>> ListAsync(int? limit = null, CancellationToken ct = default);
}
