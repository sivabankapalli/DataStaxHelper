namespace DataStax.AstraDB.Helper.Abstractions;

/// <summary>
/// Provides abstraction over JSON serialization and deserialization.
/// Consumers can plug in System.Text.Json, Newtonsoft.Json, or custom implementations.
/// </summary>
public interface IJsonSerializer
{
    /// <summary>
    /// Serializes an object to a JSON string.
    /// </summary>
    string Serialize<T>(T obj);

    /// <summary>
    /// Deserializes JSON text into an object.
    /// </summary>
    T Deserialize<T>(string json);
}
