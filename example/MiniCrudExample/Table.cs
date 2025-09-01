namespace MiniCrudExample;

internal static class Table
{
    public static void Print<T>(IEnumerable<T> rows, params (string Header, Func<T, object?> Selector)[] cols)
    {
        var data = rows?.ToList() ?? new List<T>();
        if (cols is null || cols.Length == 0) { Console.WriteLine("(no columns)"); return; }

        var widths = GetWidths(data, cols);
        PrintSep(widths);
        Console.WriteLine("| " + string.Join(" | ", cols.Select((c, i) => c.Header.PadRight(widths[i]))) + " |");
        PrintSep(widths);

        if (data.Count == 0)
        {
            Console.WriteLine("(no rows)");
            PrintSep(widths);
            return;
        }

        foreach (var r in data)
        {
            var cells = cols.Select((c, i) =>
            {
                var s = c.Selector(r)?.ToString() ?? "";
                return s.PadRight(widths[i]);
            });
            Console.WriteLine("| " + string.Join(" | ", cells) + " |");
        }
        PrintSep(widths);

        static int[] GetWidths(System.Collections.Generic.IReadOnlyList<T> rowsLocal, (string Header, Func<T, object?> Selector)[] columns)
        {
            var w = new int[columns.Length];
            for (int i = 0; i < columns.Length; i++) w[i] = columns[i].Header.Length;
            foreach (var r in rowsLocal)
                for (int i = 0; i < columns.Length; i++)
                {
                    var cell = columns[i].Selector(r)?.ToString() ?? "";
                    if (cell.Length > w[i]) w[i] = cell.Length;
                }
            return w;
        }

        static void PrintSep(int[] widths) =>
            Console.WriteLine("+-" + string.Join("-+-", widths.Select(w => new string('-', w))) + "-+");
    }
}
