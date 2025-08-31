
using DataStax.AstraDB.Helper.Abstractions.Mapping;

namespace ScbSample;

[CqlTable("users_scb")]
public sealed class UserDoc
{
    [CqlPartitionKey]
    public string email { get; set; } = default!;

    [CqlColumn] public string name { get; set; } = default!;
    [CqlColumn] public string password { get; set; } = default!;
    [CqlColumn] public Guid user_id { get; set; }
}
