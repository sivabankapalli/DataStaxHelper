using Cassandra;

public sealed class ScbSessionHandle : IAsyncDisposable
{
    private readonly ICluster _cluster;
    public ISession Session { get; }

    private ScbSessionHandle(ICluster cluster, ISession session)
    {
        _cluster = cluster;
        Session = session;
    }

    /// <summary>
    /// Connect to Astra with SCB + Token auth. Disposes cluster when disposed.
    /// </summary>
    public static async Task<ScbSessionHandle> ConnectAsync(
        string scbPath,
        string token,
        string keyspace)
    {
        if (string.IsNullOrWhiteSpace(scbPath) || !File.Exists(scbPath))
            throw new FileNotFoundException($"SCB not found: {scbPath}");
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Token is required.", nameof(token));
        if (string.IsNullOrWhiteSpace(keyspace))
            throw new ArgumentException("Keyspace is required.", nameof(keyspace));

        var cluster = Cluster.Builder()
            .WithCloudSecureConnectionBundle(scbPath)
            .WithCredentials("token", token)
            // Optional tuning examples:
            //.WithSocketOptions(new SocketOptions().SetReadTimeoutMillis(120_000))
            //.WithPoolingOptions(new PoolingOptions().SetCoreConnectionsPerHost(HostDistance.Local, 1))
            .Build();

        var session = await cluster.ConnectAsync(keyspace).ConfigureAwait(false);
        return new ScbSessionHandle(cluster, session);
    }

    public async ValueTask DisposeAsync()
    {
        Session.Dispose();
        await Task.Run(_cluster.Dispose);
    }
}
