namespace DataStax.AstraDB.Helper.Abstractions.Mapping;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class CqlPartitionKeyAttribute : Attribute
{
    public int Order { get; }
    public CqlPartitionKeyAttribute(int order = 0) => Order = order;
}
