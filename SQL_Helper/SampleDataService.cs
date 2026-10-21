using Microsoft.Data.SqlClient;
using System.Data;
using System.Globalization;
using System.Text;
namespace SQL_Helper;
internal sealed record SampleColumn(string Name, string Type, bool Nullable, bool Identity, bool Computed, bool HasDefault, bool PrimaryKey, int Length, byte Precision, byte Scale, int Generated);
internal sealed record SampleOption(SampleColumn Column, string Mode, string Minimum, string Maximum, string Format);
internal sealed record ForeignSample(IReadOnlyList<string> Columns, DataTable Rows);
internal sealed record SampleSchema(string Table, IReadOnlyList<SampleColumn> Columns, IReadOnlyList<ForeignSample> ForeignKeys);
internal static class SampleDataService
{
    internal static async Task<SampleSchema> ReadAsync(string connection, string table, CancellationToken token)
    {
        table = SqlIdentifier.Quote(table);
        var metadata = await SqlTools.QueryAsync(connection, @"
SELECT c.name,TYPE_NAME(c.system_type_id) AS TypeName,c.is_nullable,c.is_identity,c.is_computed,c.default_object_id,c.max_length,c.precision,c.scale,c.generated_always_type,
CASE WHEN EXISTS(SELECT 1 FROM sys.indexes i JOIN sys.index_columns ic ON i.object_id=ic.object_id AND i.index_id=ic.index_id WHERE i.is_primary_key=1 AND ic.object_id=c.object_id AND ic.column_id=c.column_id) THEN 1 ELSE 0 END AS IsPrimaryKey
FROM sys.columns c WHERE c.object_id=OBJECT_ID(@table) ORDER BY c.column_id", token, new SqlParameter("@table", table));
        if (metadata.Rows.Count == 0) throw new InvalidOperationException("Table not found or its columns are inaccessible.");
        var columns = metadata.Rows.Cast<DataRow>().Select(r => new SampleColumn((string)r["name"], (string)r["TypeName"], (bool)r["is_nullable"], (bool)r["is_identity"], (bool)r["is_computed"], (int)r["default_object_id"] != 0, Convert.ToBoolean(r["IsPrimaryKey"]), Convert.ToInt32(r["max_length"]), (byte)r["precision"], (byte)r["scale"], Convert.ToInt32(r["generated_always_type"]))).ToList();
        var references = await SqlTools.QueryAsync(connection, @"
SELECT fk.object_id,pc.name AS ParentColumn,rc.name AS RefColumn,SCHEMA_NAME(rt.schema_id) AS RefSchema,rt.name AS RefTable,fkc.constraint_column_id
FROM sys.foreign_keys fk JOIN sys.foreign_key_columns fkc ON fkc.constraint_object_id=fk.object_id
JOIN sys.columns pc ON pc.object_id=fkc.parent_object_id AND pc.column_id=fkc.parent_column_id
JOIN sys.columns rc ON rc.object_id=fkc.referenced_object_id AND rc.column_id=fkc.referenced_column_id JOIN sys.tables rt ON rt.object_id=fkc.referenced_object_id
WHERE fk.parent_object_id=OBJECT_ID(@table) ORDER BY fk.object_id,fkc.constraint_column_id", token, new SqlParameter("@table", table));
        var foreignKeys = new List<ForeignSample>();
        foreach (var group in references.Rows.Cast<DataRow>().GroupBy(r => (int)r["object_id"]))
        {
            var first = group.First(); string query = "SELECT DISTINCT TOP (10000) " + string.Join(",", group.Select(r => SqlTools.Quote((string)r["RefColumn"]))) + " FROM " + SqlTools.Qualify((string)first["RefSchema"], (string)first["RefTable"]);
            var rows = await SqlTools.QueryAsync(connection, query, token); foreignKeys.Add(new ForeignSample(group.Select(r => (string)r["ParentColumn"]).ToList(), rows));
        }
        return new SampleSchema(table, columns, foreignKeys);
    }
    internal static bool Writable(SampleColumn column) => !column.Identity && !column.Computed && column.Generated == 0 && column.Type is not ("timestamp" or "rowversion");
    internal static SqlDbType DbType(string type) => type switch { "numeric" => SqlDbType.Decimal, "rowversion" => SqlDbType.Timestamp, "sql_variant" => SqlDbType.Variant, _ => Enum.TryParse<SqlDbType>(type, true, out var parsed) ? parsed : throw new NotSupportedException("Sample generation is unavailable for SQL type " + type) };
    internal static object Generate(SampleOption option, long row, decimal primaryKeyBase = 0)
    {
        var c = option.Column; string mode = option.Mode; var culture = CultureInfo.InvariantCulture;
        if (mode == "NULL") { if (!c.Nullable) throw new InvalidOperationException(c.Name + " does not allow NULL."); return DBNull.Value; }
        var parameter = new SqlParameter("@value", DbType(c.Type));
        if (mode == "Constant") return ParameterValues.Parse(parameter, option.Format, false);
        if (mode == "Custom format") return Limit(Format(option.Format), c);
        if (c.PrimaryKey && c.Type is "tinyint" or "smallint" or "int" or "bigint" or "decimal" or "numeric") return primaryKeyBase + row + 1;
        if (c.Type is "tinyint" or "smallint" or "int" or "bigint" or "decimal" or "numeric" or "money" or "smallmoney" or "float" or "real")
        {
            decimal min = string.IsNullOrWhiteSpace(option.Minimum) ? 1 : decimal.Parse(option.Minimum, culture), max = string.IsNullOrWhiteSpace(option.Maximum) ? 100 : decimal.Parse(option.Maximum, culture);
            if (min > max) throw new ArgumentException(c.Name + ": minimum exceeds maximum.");
            if (c.Type is "tinyint" or "smallint" or "int" or "bigint")
            {
                long lower = checked((long)min), upper = checked((long)max); long value = lower == upper ? lower : Random.Shared.NextInt64(lower, upper == long.MaxValue ? upper : upper + 1);
                return ParameterValues.Parse(parameter, value.ToString(culture), false);
            }
            decimal valueDecimal = decimal.Round(min + (decimal)Random.Shared.NextDouble() * (max - min), Math.Min((int)c.Scale, 28));
            return ParameterValues.Parse(parameter, valueDecimal.ToString(culture), false);
        }
        if (c.Type is "date" or "datetime" or "datetime2" or "smalldatetime" or "datetimeoffset")
        {
            DateTime start = string.IsNullOrWhiteSpace(option.Minimum) ? DateTime.Today.AddYears(-1) : DateTime.Parse(option.Minimum, culture), end = string.IsNullOrWhiteSpace(option.Maximum) ? DateTime.Today : DateTime.Parse(option.Maximum, culture);
            if (start > end) throw new ArgumentException(c.Name + ": start date exceeds end date.");
            var date = start.AddTicks((long)((end.Ticks - start.Ticks) * Random.Shared.NextDouble())); return c.Type == "datetimeoffset" ? (object)new DateTimeOffset(date) : date;
        }
        if (c.Type == "bit") { int percentage = string.IsNullOrWhiteSpace(option.Format) ? 50 : int.Parse(option.Format, culture); if (percentage is < 0 or > 100) throw new ArgumentException(c.Name + ": true percentage must be 0–100."); return Random.Shared.Next(100) < percentage; }
        if (c.Type == "uniqueidentifier") return Guid.NewGuid();
        if (c.Type == "time") return TimeSpan.FromSeconds(Random.Shared.Next(86400));
        if (c.Type is "binary" or "varbinary" or "image") { var bytes = new byte[c.Length < 0 ? 16 : Math.Min(c.Length, 64)]; Random.Shared.NextBytes(bytes); return bytes; }
        if (c.Type == "xml") return "<sample id=\"" + (row + 1) + "\" />";
        if (c.Type is "varchar" or "nvarchar" or "char" or "nchar" or "text" or "ntext" or "sysname" or "sql_variant")
        {
            string suffix = Guid.NewGuid().ToString("N");
            string text = mode switch { "Random Name" => "Person " + suffix[..10], "Random Address" => Random.Shared.Next(1, 9999) + " Sample Street", "Random Phone" => "03" + Random.Shared.NextInt64(100000000, 1000000000), "Random CNIC" => Format("#####-#######-#"), "Random Email" => "user" + suffix[..12] + "@example.test", _ => suffix };
            return Limit(text, c);
        }
        throw new NotSupportedException(c.Name + ": configure a database default for unsupported type " + c.Type);
    }
    private static string Limit(string text, SampleColumn column)
    {
        int max = column.Length < 0 || column.Type is "text" or "ntext" ? int.MaxValue : column.Type is "nvarchar" or "nchar" ? column.Length / 2 : column.Length;
        return text.Length <= max ? text : text[..max];
    }
    internal static string Format(string format)
    {
        var builder = new StringBuilder(); foreach (char ch in format) builder.Append(ch switch { '#' => (char)Random.Shared.Next('0', '9' + 1), 'A' => (char)Random.Shared.Next('A', 'Z' + 1), 'a' => (char)Random.Shared.Next('a', 'z' + 1), '*' => "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789"[Random.Shared.Next(62)], _ => ch }); return builder.ToString();
    }
    internal static async Task InsertAsync(string connection, SampleSchema schema, IReadOnlyList<SampleOption> options, int count, IProgress<int>? progress, CancellationToken token)
    {
        if (count <= 0) throw new ArgumentOutOfRangeException(nameof(count));
        var selected = options.Where(o => o.Mode != "Database default").ToList();
        foreach (var option in options.Where(o => o.Mode == "Database default")) if (!option.Column.HasDefault && !option.Column.Nullable) throw new ArgumentException(option.Column.Name + " has no database default and is not nullable.");
        // Validate every configured generator before beginning a transaction.
        foreach (var option in selected.Where(o => o.Mode != "Referenced row")) Generate(option, 0);
        using var con = new SqlConnection(connection); await con.OpenAsync(token);
        using var transaction = con.BeginTransaction(IsolationLevel.Serializable);
        try
        {
            var primaryBases = new Dictionary<string, decimal>();
            foreach (var option in selected.Where(o => o.Column.PrimaryKey && o.Column.Type is "tinyint" or "smallint" or "int" or "bigint" or "decimal" or "numeric"))
            {
                using var maximum = new SqlCommand("SELECT MAX(" + SqlTools.Quote(option.Column.Name) + ") FROM " + schema.Table + " WITH (TABLOCKX,HOLDLOCK)", con, transaction);
                object? value = await maximum.ExecuteScalarAsync(token); primaryBases[option.Column.Name] = value is null or DBNull ? 0 : Convert.ToDecimal(value, CultureInfo.InvariantCulture);
            }
            string sql = selected.Count == 0 ? "INSERT INTO " + schema.Table + " DEFAULT VALUES" : "INSERT INTO " + schema.Table + " (" + string.Join(",", selected.Select(o => SqlTools.Quote(o.Column.Name))) + ") VALUES (" + string.Join(",", selected.Select((_, i) => "@p" + i)) + ")";
            using var command = new SqlCommand(sql, con, transaction);
            for (int i = 0; i < selected.Count; i++) { var c = selected[i].Column; var p = command.Parameters.Add("@p" + i, DbType(c.Type)); p.Size = c.Length < 0 ? -1 : c.Type is "nvarchar" or "nchar" ? c.Length / 2 : c.Length; p.Precision = c.Precision; p.Scale = c.Scale; }
            for (int row = 0; row < count; row++)
            {
                token.ThrowIfCancellationRequested(); var foreignValues = new Dictionary<string, object>();
                foreach (var fk in schema.ForeignKeys)
                {
                    if (!fk.Columns.Any(name => selected.Any(o => o.Column.Name == name && o.Mode == "Referenced row"))) continue;
                    if (fk.Rows.Rows.Count == 0) throw new InvalidOperationException("A referenced table has no rows. Seed the parent table first, or use NULL/default where allowed.");
                    var sample = fk.Rows.Rows[Random.Shared.Next(fk.Rows.Rows.Count)];
                    for (int i = 0; i < fk.Columns.Count; i++) { string name = fk.Columns[i]; if (foreignValues.TryGetValue(name, out var existing) && !Equals(existing, sample[i])) throw new InvalidOperationException("Overlapping foreign keys require compatible constant values."); foreignValues[name] = sample[i]; }
                }
                for (int i = 0; i < selected.Count; i++)
                {
                    var option = selected[i]; object value = option.Mode == "Referenced row" ? foreignValues[option.Column.Name] : Generate(option, row, primaryBases.GetValueOrDefault(option.Column.Name));
                    command.Parameters[i].Value = value;
                }
                await command.ExecuteNonQueryAsync(token); if ((row + 1) % 25 == 0 || row == count - 1) progress?.Report(row + 1);
            }
            transaction.Commit();
        }
        catch { try { transaction.Rollback(); } catch (SqlException) { } throw; }
    }
}
