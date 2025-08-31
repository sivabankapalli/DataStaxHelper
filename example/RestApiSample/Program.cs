using DataStax.AstraDB.Helper.DataApi.Rest;
using Microsoft.Extensions.Configuration;
using RestApiSample;


public class UserDoc
{
    public string email { get; set; } = default!;
    public string name { get; set; } = default!;
    public string password { get; set; } = default!;
    public Guid user_id { get; set; }
}

internal static class Program
{
    public static async Task Main()
    {
        Console.WriteLine("=== Astra Data API (HTTP) sample with appsettings.json ===");

        // 1. Load configuration
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        AstraConfig? astra = config.GetSection("Astra").Get<AstraConfig>();

        if (astra is null || string.IsNullOrWhiteSpace(astra.Token))
        {
            Console.WriteLine("Missing Astra configuration in appsettings.json");
            return;
        }

        // 2. Create client and collection
        using var client = new RestDataApiClient(astra.Token, astra.Endpoint);
        var db = client.GetDatabase(astra.Keyspace);
        var coll = db.GetCollection<UserDoc>(astra.Collection);

        // 3. Insert a document
        var bob = new UserDoc
        {
            email = "bob@example.com",
            name = "Bob",
            password = "secret123",
            user_id = Guid.NewGuid()
        };
        var insertedId = await coll.InsertOneAsync(bob);
        Console.WriteLine($"Inserted id={insertedId ?? "(none returned)"}");

        // 4. Find documents
        var found = await coll.FindAsync(new { email = "bob@example.com" }, limit: 5);
        if (found is not null)
        {
            Console.WriteLine("=== Table ===");
            TablePrinter.PrintTable([found]);
        }
        else
        {
            Console.WriteLine("User not found.");
        }

        // 5. Update
        var update = new Dictionary<string, object>
        {
            ["$set"] = new { name = "Bob Updated" }
        };
        var upd = await coll.UpdateOneAsync(new { email = "bob@example.com" }, update);
        Console.WriteLine($"UpdateOne: matched={upd.UpdateOne?.MatchedCount}, modified={upd.UpdateOne?.ModifiedCount}");
        found = await coll.FindAsync(new { email = "bob@example.com" }, limit: 5);

        if (found is not null)
        {
            Console.WriteLine("=== Table ===");
            TablePrinter.PrintTable([found]);
        }
        else
        {
            Console.WriteLine("User not found.");
        }

        // 6. FindOne
        var one = await coll.FindOneAsync(new { email = "bob@example.com" });
        Console.WriteLine($"FindOne: {(one is null ? "null" : $"{one.email} / {one.name}")}");

        // 7. Delete
        var del = await coll.DeleteOneAsync(new { email = "bob@example.com" });
        Console.WriteLine($"DeleteOne: deleted={del.DeleteOne?.DeletedCount}");


        Console.WriteLine("Done.");
    }
}
