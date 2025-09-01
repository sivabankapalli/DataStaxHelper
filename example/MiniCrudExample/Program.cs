using Cassandra;

namespace MiniCrudExample;

internal static class Program
{
    static async Task<int> Main()
    {
        using var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };

        try
        {
            var settings = Settings.Load();
            ValidateSettings(settings);

            await using var crud = await UserCrud.ConnectAsync(
                settings.ScbPath, settings.Token, settings.Keyspace, cts.Token);

            // 1) Create
            var user = NewUser("ada@example.com", "Ada Lovelace", "secret");
            await crud.InsertAsync(user, cts.Token);
            Info("Inserted:");
            Table.Print(new[] { user }, ("user_id", u => u.UserId), ("email", u => u.Email), ("name", u => u.Name));

            // 2) Read (by PK)
            var fetched = await crud.GetAsync(user.UserId, cts.Token);
            Info("\nFetched:");
            Table.Print(AsArrayOrEmpty(fetched), ("user_id", u => u.UserId), ("email", u => u.Email), ("name", u => u.Name));

            // 3) Update
            await crud.UpdateNameAsync(user.UserId, "Ada L.", cts.Token);
            var after = await crud.GetAsync(user.UserId, cts.Token);
            Info("\nAfter Update:");
            Table.Print(AsArrayOrEmpty(after), ("user_id", u => u.UserId), ("email", u => u.Email), ("name", u => u.Name));

            // 4) Delete
            await crud.DeleteAsync(user.UserId, cts.Token);
            var gone = await crud.GetAsync(user.UserId, cts.Token);
            Info("\nAfter Delete:");
            Table.Print(AsArrayOrEmpty(gone), ("user_id", u => u.UserId), ("email", u => u.Email), ("name", u => u.Name));

            return 0;
        }
        catch (OperationCanceledException)
        {
            Warn("Cancelled.");
            return 1;
        }
        catch (FileNotFoundException ex)
        {
            Error(ex.Message);
            return 2;
        }
        catch (NoHostAvailableException ex)
        {
            Error("Cluster connection failed (NoHostAvailable). Check SCB path, token, keyspace, and network.");
            Error(ex.Message);
            return 3;
        }
        catch (AuthenticationException ex)
        {
            Error("Authentication failed. Verify your Astra token (should start with 'AstraCS:...').");
            Error(ex.Message);
            return 4;
        }
        catch (Exception ex)
        {
            Error(ex.ToString());
            return 5;
        }
    }

    // --- Helpers -------------------------------------------------------------

    private static void ValidateSettings(Settings s)
    {
        if (string.IsNullOrWhiteSpace(s.ScbPath)) throw new ArgumentException("SCB path is empty.");
        if (!File.Exists(s.ScbPath)) throw new FileNotFoundException($"SCB not found: {s.ScbPath}");
        if (string.IsNullOrWhiteSpace(s.Token)) throw new ArgumentException("Token is empty.");
        if (string.IsNullOrWhiteSpace(s.Keyspace)) throw new ArgumentException("Keyspace is empty.");
    }

    private static User NewUser(string email, string name, string password) => new()
    {
        UserId = Guid.NewGuid(),
        Email = email,
        Name = name,
        Password = password
    };

    private static User[] AsArrayOrEmpty(User? u) => u is null ? Array.Empty<User>() : new[] { u };

    private static void Info(string msg) => Console.WriteLine(msg);
    private static void Warn(string msg) { var c = Console.ForegroundColor; Console.ForegroundColor = ConsoleColor.Yellow; Console.WriteLine(msg); Console.ForegroundColor = c; }
    private static void Error(string msg) { var c = Console.ForegroundColor; Console.ForegroundColor = ConsoleColor.Red; Console.WriteLine(msg); Console.ForegroundColor = c; }
}
