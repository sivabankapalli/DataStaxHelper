using Cassandra;
using DriverSample;
using Microsoft.Extensions.Configuration;
using ScbSample;

class Program
{
    static async Task Main()
    {
        // 1) Load config
        var cfg = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var scbPath = cfg["Astra:ScbPath"] ?? throw new("Missing Astra:ScbPath");
        var token = cfg["Astra:Token"] ?? throw new("Missing Astra:Token");
        var keyspace = cfg["Astra:Keyspace"] ?? throw new("Missing Astra:Keyspace");
        var table = cfg["Astra:Collection"] ?? "users";

        // 2) Connect (using your ScbSessionHandle)
        await using var handle = await ScbSessionHandle.ConnectAsync(scbPath, token, keyspace);
        var session = handle.Session;
        Console.WriteLine("Connected to Astra via SCB.");

        // 3) Ensure table exists (idempotent)
        var createTableCql = $@"
            CREATE TABLE IF NOT EXISTS {table} (
                email text PRIMARY KEY,
                name text,
                password text,
                user_id uuid
            );";
        await session.ExecuteAsync(new SimpleStatement(createTableCql));

        // Prepare statements once
        var psInsert = await session.PrepareAsync($"INSERT INTO {table} (email, name, password, user_id) VALUES (?, ?, ?, ?)");
        var psSelect = await session.PrepareAsync($"SELECT email, name, password, user_id FROM {table} WHERE email = ?");
        var psUpdate = await session.PrepareAsync($"UPDATE {table} SET name = ? WHERE email = ?");
        var psDelete = await session.PrepareAsync($"DELETE FROM {table} WHERE email = ?");
        var psScan = await session.PrepareAsync($"SELECT email, name, password, user_id FROM {table}");

        // 4) Create (INSERT is upsert in Cassandra)
        var user = new UserDoc
        {
            email = "bob@example.com",
            name = "Bob",
            password = "secret",
            user_id = Guid.NewGuid()
        };
        await session.ExecuteAsync(psInsert.Bind(user.email, user.name, user.password, user.user_id));
        Console.WriteLine("Inserted.");

        // 5) Read (by PK)
        var read = await session.ExecuteAsync(psSelect.Bind(user.email));
        var found = read.SingleOrDefault() is Row r
            ? new UserDoc
            {
                email = r.GetValue<string>("email"),
                name = r.GetValue<string>("name"),
                password = r.GetValue<string>("password"),
                user_id = r.GetValue<Guid>("user_id")
            }
            : null;
        Console.WriteLine("\n=== After insert ===");
        TablePrinter.PrintTable(found is null ? Array.Empty<UserDoc>() : new[] { found! });

        // 6) Update (name)
        await session.ExecuteAsync(psUpdate.Bind("Bob Updated", user.email));
        var afterUpdateRow = (await session.ExecuteAsync(psSelect.Bind(user.email))).SingleOrDefault();
        var afterUpdate = afterUpdateRow is null ? null : new UserDoc
        {
            email = afterUpdateRow.GetValue<string>("email"),
            name = afterUpdateRow.GetValue<string>("name"),
            password = afterUpdateRow.GetValue<string>("password"),
            user_id = afterUpdateRow.GetValue<Guid>("user_id")
        };
        Console.WriteLine("\n=== After update ===");
        TablePrinter.PrintTable(afterUpdate is null ? Array.Empty<UserDoc>() : new[] { afterUpdate! });

        // 7) List (first N)
        var scan = await session.ExecuteAsync(psScan.Bind().SetPageSize(10));
        var list = scan.Select(row => new UserDoc
        {
            email = row.GetValue<string>("email"),
            name = row.GetValue<string>("name"),
            password = row.GetValue<string>("password"),
            user_id = row.GetValue<Guid>("user_id")
        }).ToList();
        Console.WriteLine("\n=== Top rows ===");
        TablePrinter.PrintTable(list);

        // 8) Delete
        await session.ExecuteAsync(psDelete.Bind(user.email));
        var afterDelete = (await session.ExecuteAsync(psSelect.Bind(user.email))).SingleOrDefault();
        Console.WriteLine("\nDeleted. Exists after delete? " + (afterDelete is null ? "No ✅" : "Yes ❌"));
    }
}
