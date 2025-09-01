using Cassandra;

namespace MiniCrudExample;

public sealed class UserCrud : IAsyncDisposable
{
    private readonly ICluster _cluster;
    private readonly ISession _session;

    private readonly PreparedStatement _ins;
    private readonly PreparedStatement _sel;
    private readonly PreparedStatement _updName;
    private readonly PreparedStatement _del;

    private UserCrud(ICluster cluster, ISession session,
        PreparedStatement ins, PreparedStatement sel, PreparedStatement updName, PreparedStatement del)
    {
        _cluster = cluster;
        _session = session;
        _ins = ins;
        _sel = sel;
        _updName = updName;
        _del = del;
    }

    public static async Task<UserCrud> ConnectAsync(string scbPath, string token, string keyspace, CancellationToken ct = default)
    {
        var cluster = Cluster.Builder()
            .WithCloudSecureConnectionBundle(scbPath)
            .WithCredentials("token", token)
            .Build();

        // Driver doesn't accept CancellationToken here, so we link via cooperative checks
        ct.ThrowIfCancellationRequested();
        var session = await cluster.ConnectAsync(keyspace).ConfigureAwait(false);

        // Prepare once, reuse
        var ins = await session.PrepareAsync(
            "INSERT INTO users (user_id, email, name, password) VALUES (?,?,?,?)").ConfigureAwait(false);

        var sel = await session.PrepareAsync(
            "SELECT user_id, email, name, password FROM users WHERE user_id = ?").ConfigureAwait(false);

        var updName = await session.PrepareAsync(
            "UPDATE users SET name = ? WHERE user_id = ?").ConfigureAwait(false);

        var del = await session.PrepareAsync(
            "DELETE FROM users WHERE user_id = ?").ConfigureAwait(false);

        return new UserCrud(cluster, session, ins, sel, updName, del);
    }

    public async Task InsertAsync(User u, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        await _session.ExecuteAsync(_ins.Bind(u.UserId, u.Email, u.Name, u.Password)).ConfigureAwait(false);
    }

    public async Task<User?> GetAsync(Guid userId, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var rs = await _session.ExecuteAsync(_sel.Bind(userId)).ConfigureAwait(false);
        var row = rs.FirstOrDefault();
        if (row is null) return null;
        return new User
        {
            UserId = row.GetValue<Guid>("user_id"),
            Email = row.GetValue<string>("email"),
            Name = row.GetValue<string>("name"),
            Password = row.GetValue<string>("password")
        };
    }

    public async Task UpdateNameAsync(Guid userId, string newName, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        await _session.ExecuteAsync(_updName.Bind(newName, userId)).ConfigureAwait(false);
    }

    public async Task DeleteAsync(Guid userId, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        await _session.ExecuteAsync(_del.Bind(userId)).ConfigureAwait(false);
    }

    public ValueTask DisposeAsync()
    {
        _session?.Dispose();
        _cluster?.Dispose();
        return ValueTask.CompletedTask;
    }
}