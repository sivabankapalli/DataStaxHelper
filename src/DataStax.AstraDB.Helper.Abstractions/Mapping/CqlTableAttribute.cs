namespace DataStax.AstraDB.Helper.Abstractions.Mapping;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class CqlTableAttribute : Attribute
{
    public string Name { get; }
    public string? Keyspace { get; }
    public CqlTableAttribute(string name, string? keyspace = null)
    {
        Name = name; Keyspace = keyspace;
    }
}
