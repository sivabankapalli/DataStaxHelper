using Cassandra;

namespace DataStax.AstraDB.Helper.Abstractions;

public interface ICqlSessionFactory : IAsyncDisposable
{
    ISession Session { get; }
}
