using System.Reflection;

namespace DriverSample;

public static class TablePrinter
{
    public static void PrintTable<T>(IEnumerable<T> rows)
    {
        var list = rows.ToList();
        if (!list.Any()) { Console.WriteLine("No rows."); return; }

        var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var colNames = props.Select(p => p.Name).ToArray();

        var widths = new Dictionary<string, int>();
        foreach (var col in colNames)
        {
            var p = props.First(x => x.Name == col);
            var maxVal = list.Select(r => p.GetValue(r)?.ToString()?.Length ?? 0)
                             .DefaultIfEmpty(0).Max();
            widths[col] = Math.Max(col.Length, maxVal);
        }

        WriteRow(colNames, widths);
        WriteSep(widths, colNames);

        foreach (var r in list)
        {
            var vals = props.Select(p => p.GetValue(r)?.ToString() ?? "").ToArray();
            WriteRow(vals, widths);
        }

        static void WriteRow(IEnumerable<string> vals, Dictionary<string, int> widths)
        {
            var a = vals.ToArray();
            var i = 0;
            foreach (var (col, w) in widths)
            {
                Console.Write(" " + a[i].PadRight(w) + " ");
                if (i < widths.Count - 1) Console.Write("|");
                i++;
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