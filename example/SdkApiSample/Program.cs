using DataStax.AstraDB.DataApi.Core;
using DataStax.AstraDB.Helper.DataApi.Sdk;
using Microsoft.Extensions.Configuration;
using SdkApiSample;

class Program
{
    static async Task Main()
    {
        // load config
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var endpoint = config["Astra:Endpoint"] ?? throw new("Missing Astra:Endpoint");
        var token = config["Astra:Token"] ?? throw new("Missing Astra:Token");
        var collection = config["Astra:Collection"] ?? "users";

        var client = new SdkDataApiClient(endpoint, token);
        var repo = new SdkRepository<UserDoc>(client.Database, collection);

        Console.WriteLine("=== Astra SDK Sample ===");

        // Create
        var user = new UserDoc
        {
            email = "bob@example.com",
            name = "Bob",
            password = "secret",
            user_id = Guid.NewGuid()
        };
        await repo.InsertOneAsync(user);
        Console.WriteLine($"Inserted {user.email}");

        // Read
        var filter = Builders<UserDoc>.Filter.Eq(x => x.email, "bob@example.com");
        var found = await repo.FindOneAsync(filter);

        if (found is not null)
        {
            Console.WriteLine("=== Table ===");
            TablePrinter.PrintTable([found]);
        }
        else
        {
            Console.WriteLine("User not found.");
        }


        // Update
        var update = Builders<UserDoc>.Update.Set(x => x.name, "Bob Updated");
        var modified = await repo.UpdateOneAsync(filter, update);
        Console.WriteLine($"Modified {modified} doc(s)");
        filter = Builders<UserDoc>.Filter.Eq(x => x.email, "bob@example.com");
        found = await repo.FindOneAsync(filter);

        if (found is not null)
        {
            Console.WriteLine("=== Table ===");
            TablePrinter.PrintTable([found]);
        }
        else
        {
            Console.WriteLine("User not found.");
        }

        // Delete
        var deleted = await repo.DeleteOneAsync(filter);
        Console.WriteLine($"Deleted {deleted} doc(s)");
    }
}
