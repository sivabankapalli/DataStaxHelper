using System.Reflection;

namespace SdkApiSample;
/// <summary>Utility to print a collection of objects as a table using their public properties.</summary>
public static class TablePrinter
{
    /// <summary>
    /// Prints a collection of objects as a table using their public properties.
    /// </summary>
    public static void PrintTable<T>(IEnumerable<T> rows)
    {
        var list = rows.ToList();
        if (!list.Any())
        {
            Console.WriteLine("No rows.");
            return;
        }

        var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var colNames = props.Select(p => p.Name).ToArray();

        // calculate widths
        var widths = new Dictionary<string, int>();
        foreach (var col in colNames)
        {
            var maxVal = list.Select(r => props.First(p => p.Name == col).GetValue(r)?.ToString()?.Length ?? 0)
                             .DefaultIfEmpty(0).Max();
            widths[col] = Math.Max(col.Length, maxVal);
        }

        // print header
        WriteRow(colNames, widths);
        WriteSep(widths, colNames);

        // print rows
        foreach (var r in list)
        {
            var vals = props.Select(p => p.GetValue(r)?.ToString() ?? "").ToArray();
            WriteRow(vals, widths);
        }

        static void WriteRow(IEnumerable<string> vals, Dictionary<string, int> widths)
        {
            var a = vals.ToArray();
            for (int i = 0; i < a.Length; i++)
            {
                var col = widths.Keys.ElementAt(i);
                Console.Write(" " + a[i].PadRight(widths[col]) + " ");
                if (i < a.Length - 1) Console.Write("|");
            }
            Console.WriteLine();
        }

        static void WriteSep(Dictionary<string, int> widths, string[] cols)
        {
            for (int i = 0; i < cols.Length; i++)
            {
                Console.Write(" " + new string('-', widths[cols[i]]) + " ");
                if (i < cols.Length - 1) Console.Write("+");
            }
            Console.WriteLine();
        }
    }
}
