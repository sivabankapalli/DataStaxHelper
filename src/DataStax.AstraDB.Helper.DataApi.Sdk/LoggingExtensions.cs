namespace DataStax.AstraDB.Helper.DataApi.Sdk;

public static class LoggingExtensions
{
    public static void Dump<T>(this IEnumerable<T> items, Func<T, string> projector)
    {
        foreach (var i in items) Console.WriteLine(projector(i));
    }
}
