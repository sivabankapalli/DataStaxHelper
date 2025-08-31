using System.Reflection;
using Cassandra;
using DataStax.AstraDB.Helper.Abstractions;
using DataStax.AstraDB.Helper.Abstractions.Mapping;

namespace DataStax.AstraDB.Helper.Scb;

/// <summary>
/// Generic repository for a table with one or more partition key columns.
/// Builds prepared statements once via reflection.
/// </summary>
public sealed class CqlRepository<T> : IScbRepository<T> where T : class, new()
{
    private readonly ISession _session;
    private readonly string _keyspace;
    private readonly string _table;
    private readonly (PropertyInfo Prop, string Col)[] _columns;
    private readonly (PropertyInfo Prop, string Col)[] _pks; // partition key(s)

    private PreparedStatement _psInsert = default!;
    private PreparedStatement _psSelectByPk = default!;
    private PreparedStatement _psDeleteByPk = default!;
    private PreparedStatement _psSelectAll = default!;

    public CqlRepository(ISession session)
    {
        _session = session;

        var tAttr = typeof(T).GetCustomAttribute<CqlTableAttribute>()
            ?? throw new InvalidOperationException($"Type {typeof(T).Name} is missing [CqlTable].");

        _keyspace = tAttr.Keyspace ?? _session.Keyspace ?? throw new InvalidOperationException("Keyspace not set.");
        _table = tAttr.Name;

        var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                             .Where(p => p.CanRead && p.CanWrite).ToArray();

        _columns = props
            .Select(p => (p, p.GetCustomAttribute<CqlColumnAttribute>()?.Name ?? p.Name))
            .ToArray();

        _pks = props
            .Where(p => p.GetCustomAttribute<CqlPartitionKeyAttribute>() is not null)
            .OrderBy(p => p.GetCustomAttribute<CqlPartitionKeyAttribute>()!.Order)
            .Select(p => (p, p.GetCustomAttribute<CqlColumnAttribute>()?.Name ?? p.Name))
            .ToArray();

        if (_pks.Length == 0)
            throw new InvalidOperationException($"Type {typeof(T).Name} must mark partition key(s) with [CqlPartitionKey].");
    }

    public async Task InitializeAsync(CancellationToken ct = default)
    {
        // Create table if missing (simple types only)
        var cols = string.Join(", ",
            _columns.Select(c => $"{Id(c.Col)} {ToCqlType(c.Prop.PropertyType)}"));
        var pk = string.Join(", ", _pks.Select(p => Id(p.Col)));

        var create = $@"CREATE TABLE IF NOT EXISTS {Id(_keyspace)}.{Id(_table)} ({cols}, PRIMARY KEY(({pk})));";
        await _session.ExecuteAsync(new SimpleStatement(create)).ConfigureAwait(false);

        // Prepare statements
        var colNames = string.Join(", ", _columns.Select(c => Id(c.Col)));
        var qMarks = string.Join(", ", Enumerable.Repeat("?", _columns.Length));
        _psInsert = await _session.PrepareAsync($@"INSERT INTO {Id(_keyspace)}.{Id(_table)} ({colNames}) VALUES ({qMarks})");

        var wherePk = string.Join(" AND ", _pks.Select(p => $"{Id(p.Col)} = ?"));
        _psSelectByPk = await _session.PrepareAsync($@"SELECT {colNames} FROM {Id(_keyspace)}.{Id(_table)} WHERE {wherePk}");
        _psDeleteByPk = await _session.PrepareAsync($@"DELETE FROM {Id(_keyspace)}.{Id(_table)} WHERE {wherePk}");
        _psSelectAll = await _session.PrepareAsync($@"SELECT {colNames} FROM {Id(_keyspace)}.{Id(_table)}");
    }

    public async Task UpsertAsync(T entity, CancellationToken ct = default)
    {
        var values = _columns.Select(c => c.Prop.GetValue(entity)).ToArray();
        await _session.ExecuteAsync(_psInsert.Bind(values));
    }

    public async Task<T?> GetByIdAsync(object key, CancellationToken ct = default)
    {
        var values = KeyToArray(key);
        var rs = await _session.ExecuteAsync(_psSelectByPk.Bind(values));
        var row = rs.SingleOrDefault();
        return row is null ? null : MapRow(row);
    }

    public async Task<int> DeleteAsync(object key, CancellationToken ct = default)
    {
        var values = KeyToArray(key);
        await _session.ExecuteAsync(_psDeleteByPk.Bind(values));
        return 1; // driver doesn't return counts; treat success as 1
    }

    public async Task<IReadOnlyList<T>> ListAsync(int? limit = null, CancellationToken ct = default)
    {
        IStatement stmt = _psSelectAll.Bind();
        if (limit is int l) stmt = stmt.SetPageSize(l);
        var rs = await _session.ExecuteAsync(stmt);
        return rs.Select(MapRow).ToList();
    }

    // --- helpers ---

    private static string Id(string id) => id.All(char.IsLetterOrDigit) ? id : $"\"{id.Replace("\"", "\"\"")}\"";

    private static string ToCqlType(Type t)
    {
        t = Nullable.GetUnderlyingType(t) ?? t;
        if (t == typeof(string)) return "text";
        if (t == typeof(Guid)) return "uuid";
        if (t == typeof(int)) return "int";
        if (t == typeof(long)) return "bigint";
        if (t == typeof(bool)) return "boolean";
        if (t == typeof(double)) return "double";
        if (t == typeof(float)) return "float";
        if (t == typeof(DateTime) || t == typeof(DateTimeOffset)) return "timestamp";
        // You can add more mappings as needed
        throw new NotSupportedException($"CQL type mapping not defined for {t.FullName}");
    }

    private T MapRow(Row row)
    {
        var obj = new T();
        foreach (var (prop, col) in _columns)
        {
            var type = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
            object? val = type == typeof(Guid) ? row.GetValue<Guid>(col)
                        : type == typeof(string) ? row.GetValue<string>(col)
                        : type == typeof(int) ? row.GetValue<int>(col)
                        : type == typeof(long) ? row.GetValue<long>(col)
                        : type == typeof(bool) ? row.GetValue<bool>(col)
                        : type == typeof(double) ? row.GetValue<double>(col)
                        : type == typeof(float) ? row.GetValue<float>(col)
                        : type == typeof(DateTime) ? row.GetValue<DateTime>(col)
                        : type == typeof(DateTimeOffset) ? new DateTimeOffset(row.GetValue<DateTime>(col))
                        : throw new NotSupportedException($"Mapping not implemented for {type.FullName}");
            prop.SetValue(obj, val);
        }
        return obj;
    }

    private object[] KeyToArray(object key)
    {
        // single pk: pass scalar
        if (_pks.Length == 1 && key is not object[] arr1)
            return new[] { key };

        // composite pk: expect object[]
        if (_pks.Length > 1 && key is object[] arr && arr.Length == _pks.Length)
            return arr;

        // derive from entity instance (if caller passed a T)
        if (key is T entity)
            return _pks.Select(k => k.Prop.GetValue(entity)!).ToArray();

        throw new ArgumentException($"Key for {_pks.Length} partition key columns must be {(_pks.Length == 1 ? "a scalar" : "object[] of same length")}");
    }
}
