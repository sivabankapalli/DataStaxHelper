namespace SdkApiSample;

public sealed class AstraConfig
{
    public string Token { get; set; } = default!;
    public string Endpoint { get; set; } = default!;
    public string Keyspace { get; set; } = default!;
    public string Collection { get; set; } = default!;
}
