using DataStax.AstraDB.DataApi;
using DataStax.AstraDB.DataApi.Core;

namespace DataStax.AstraDB.Helper.DataApi.Sdk;

public sealed class SdkDataApiClient
{
    public Database Database { get; }

    public SdkDataApiClient(string endpoint, string token)
    {
        var sdk = new DataApiClient();
        Database = sdk.GetDatabase(endpoint, token);
    }

    public SdkDataApiClient(Database database) => Database = database;
}
