using Microsoft.Extensions.Configuration;

namespace AstraSdkSample.Tests;

internal static class AstraConfigTest
{
    private static IConfigurationRoot BuildConfig() =>
        new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.test.json", optional: true) // <-- will look in test project folder
            .Build();

    public static (string Endpoint, string Token, string Collection) Load()
    {
        var cfg = BuildConfig();
        var endpoint = cfg["Astra:Endpoint"];
        var token = cfg["Astra:Token"];
        var collection = cfg["Astra:Collection"] ?? "users_test";
        return (endpoint ?? "", token ?? "", collection);
    }

    public static bool IsConfigured(out string reason)
    {
        var (ep, tok, _) = Load();
        if (string.IsNullOrWhiteSpace(ep))
        {
            reason = "Astra:Endpoint not set in appsettings.test.json.";
            return false;
        }
        if (string.IsNullOrWhiteSpace(tok))
        {
            reason = "Astra:Token not set in appsettings.test.json.";
            return false;
        }
        reason = "";
        return true;
    }
}
