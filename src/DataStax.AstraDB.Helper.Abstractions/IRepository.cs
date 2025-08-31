namespace DataStax.AstraDB.Helper.Abstractions;

/// <summary>
/// Defines a generic repository abstraction for AstraDB collections.
/// A repository wraps an <see cref="IDataApiClient"/> and binds it to a single collection.
/// </summary>
/// <typeparam name="T">The document type. Must be a class.</typeparam>
public interface IRepository<T> where T : class
{
    /// <summary>
    /// Creates a new document in the collection.
    /// </summary>
    /// <param name="document">The document instance to insert.</param>
    Task CreateAsync(T document);

    /// <summary>
    /// Reads documents from the collection.
    /// </summary>
    /// <param name="filter">
    /// Optional filter. If <c>null</c>, returns all documents in the collection.
    /// </param>
    /// <returns>
    /// A list of documents of type <typeparamref name="T"/>.
    /// </returns>
    Task<IReadOnlyList<T>> ReadAsync(object? filter = null);

    /// <summary>
    /// Updates the first document that matches the filter.
    /// </summary>
    /// <param name="filter">Filter object identifying the target document(s).</param>
    /// <param name="update">Update definition object (e.g., <c>$set</c>).</param>
    /// <returns>Number of modified documents.</returns>
    Task<long> UpdateAsync(object filter, object update);

    /// <summary>
    /// Deletes the first document that matches the filter.
    /// </summary>
    /// <param name="filter">Filter object identifying the target document(s).</param>
    /// <returns>Number of deleted documents.</returns>
    Task<long> DeleteAsync(object filter);
}
