namespace DataStax.AstraDB.Helper.Abstractions.Mapping;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class CqlColumnAttribute : Attribute
{
    public string? Name { get; }
    public CqlColumnAttribute(string? name = null) => Name = name;
}
