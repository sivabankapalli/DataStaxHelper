using AstraSdkSample.Tests;
using DataStax.AstraDB.DataApi.Core;
using DataStax.AstraDB.Helper.DataApi.Sdk;
using FluentAssertions;
using Xunit;

namespace DataStax.AstraDB.Helper.Tests;

public class RepositoryIntegrationTests
{
    private static async Task EnsureCollectionExistsAsync(Database db, string name)
    {
        var existing = await db.ListCollectionsAsync();
        if (!existing.Any(c => string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase)))
            await db.CreateCollectionAsync(name);
    }

    [SkippableFact]
    public async Task Crud_flow_works_when_configured()
    {
        Skip.IfNot(AstraConfigTest.IsConfigured(out var reason), reason);

        var (endpoint, token, collection) = AstraConfigTest.Load();

        var client = new SdkDataApiClient(endpoint, token);
        var db = client.Database;
        await EnsureCollectionExistsAsync(db, collection);

        var repo = new SdkRepository<UserDoc>(db, collection);

        // unique email per test run
        var email = $"user_{Guid.NewGuid():N}@example.com";

        // ---- CREATE ----
        var user = new UserDoc
        {
            email = email,
            name = "Test User",
            password = "secret",
            user_id = Guid.NewGuid()
        };

        await repo.InsertOneAsync(user);

        // ---- READ ----
        var filter = Builders<UserDoc>.Filter.Eq(x => x.email, email);
        var fetched = await repo.FindOneAsync(filter);
        fetched.Should().NotBeNull();
        fetched!.name.Should().Be("Test User");

        // ---- UPDATE ----
        var update = Builders<UserDoc>.Update.Set(x => x.name, "User Updated");
        var modified = await repo.UpdateOneAsync(filter, update);
        modified.Should().BeGreaterOrEqualTo(0); // some backends may return 0 if no-op
        var afterUpdate = await repo.FindOneAsync(filter);
        afterUpdate!.name.Should().Be("User Updated");

        // ---- DELETE ----
        var deleted = await repo.DeleteOneAsync(filter);
        deleted.Should().BeGreaterOrEqualTo(0); // can be 1 for success, 0 if not found
        var afterDelete = await repo.FindOneAsync(filter);
        afterDelete.Should().BeNull();
    }
}
