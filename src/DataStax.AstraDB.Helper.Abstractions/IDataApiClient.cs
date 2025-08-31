namespace DataStax.AstraDB.Helper.Abstractions;

/// <summary>
/// Defines the contract for interacting with AstraDB's Data API (REST or SDK).
/// 
/// This interface abstracts common CRUD operations:
/// - Consumers provide filters and update definitions as simple <c>object</c>
///   values (usually anonymous objects like <c>new { email = "user@test.com" }</c>).
/// - Implementations translate these into the required form:
///   - REST: serialized to JSON payloads.
///   - SDK: wrapped in <c>CommandOptions</c>.
/// </summary>
public interface IDataApiClient
{
    /// <summary>
    /// Finds a single document in the specified collection that matches the filter.
    /// </summary>
    /// <typeparam name="T">
    /// The document type. Must be a class that maps to the collection schema.
    /// </typeparam>
    /// <param name="collection">The name of the target collection.</param>
    /// <param name="filter">
    /// Filter criteria expressed as an anonymous object, e.g.
    /// <c>new { email = "user@test.com" }</c>.
    /// </param>
    /// <returns>
    /// The first matching document, or <c>null</c> if none found.
    /// </returns>
    Task<T?> FindOneAsync<T>(string collection, object filter) where T : class;

    /// <summary>
    /// Finds multiple documents in the specified collection.
    /// </summary>
    /// <typeparam name="T">
    /// The document type. Must be a class.
    /// </typeparam>
    /// <param name="collection">The name of the target collection.</param>
    /// <param name="filter">
    /// Optional filter object. If <c>null</c>, returns all documents.
    /// </param>
    /// <returns>
    /// A read-only list of documents that match the filter. Empty list if no matches.
    /// </returns>
    Task<IReadOnlyList<T>> FindManyAsync<T>(string collection, object? filter = null) where T : class;

    /// <summary>
    /// Inserts a new document into the specified collection.
    /// </summary>
    /// <typeparam name="T">
    /// The document type. Must be a class.
    /// </typeparam>
    /// <param name="collection">The name of the target collection.</param>
    /// <param name="document">The document instance to insert.</param>
    Task InsertOneAsync<T>(string collection, T document) where T : class;

    /// <summary>
    /// Updates the first document matching the filter in the specified collection.
    /// </summary>
    /// <param name="collection">The name of the target collection.</param>
    /// <param name="filter">
    /// The filter object identifying the document(s) to update.
    /// Example: <c>new { email = "user@test.com" }</c>.
    /// </param>
    /// <param name="update">
    /// The update definition object, e.g.
    /// <c>new { $set = new { name = "New Name" } }</c>.
    /// </param>
    /// <returns>
    /// The number of modified documents (0 if none matched).
    /// </returns>
    Task<long> UpdateOneAsync(string collection, object filter, object update);

    /// <summary>
    /// Deletes the first document matching the filter in the specified collection.
    /// </summary>
    /// <param name="collection">The name of the target collection.</param>
    /// <param name="filter">
    /// The filter object identifying the document(s) to delete.
    /// Example: <c>new { email = "user@test.com" }</c>.
    /// </param>
    /// <returns>
    /// The number of documents deleted (0 if none matched).
    /// </returns>
    Task<long> DeleteOneAsync(string collection, object filter);
}
