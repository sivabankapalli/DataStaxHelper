using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MiniCrudExample;

internal sealed record Settings(string ScbPath, string Token, string Keyspace)
{
    /// <summary>
    /// Load order: Environment variables → appsettings.json (optional) → hardcoded fallback.
    /// Env: ASTRA_SCB_PATH, ASTRA_TOKEN, ASTRA_KEYSPACE
    /// </summary>
    public static Settings Load()
    {
        // 1) Environment
        var scb = Environment.GetEnvironmentVariable("ASTRA_SCB_PATH");
        var tok = Environment.GetEnvironmentVariable("ASTRA_TOKEN");
        var ksp = Environment.GetEnvironmentVariable("ASTRA_KEYSPACE");

        // 2) appsettings.json (optional), shape:
        // { "Astra": { "ScbPath": "...", "Token": "AstraCS:...", "Keyspace": "dev_ks" } }
        if (string.IsNullOrWhiteSpace(scb) || string.IsNullOrWhiteSpace(tok) || string.IsNullOrWhiteSpace(ksp))
        {
            var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
            if (File.Exists(path))
            {
                try
                {
                    using var s = File.OpenRead(path);
                    using var doc = JsonDocument.Parse(s);
                    var root = doc.RootElement;
                    if (root.TryGetProperty("Astra", out var astra))
                    {
                        scb ??= astra.TryGetProperty("ScbPath", out var v1) ? v1.GetString() : null;
                        tok ??= astra.TryGetProperty("Token", out var v2) ? v2.GetString() : null;
                        ksp ??= astra.TryGetProperty("Keyspace", out var v3) ? v3.GetString() : null;
                    }
                }
                catch { /* ignore config parse errors; fall back */ }
            }
        }

        return new Settings(scb, tok, ksp);
    }
}