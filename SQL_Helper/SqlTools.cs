using Microsoft.Data.SqlClient;
using System.Data;

namespace SQL_Helper;

internal static class SqlTools
{
    internal static string Quote(string name) => "[" + name.Replace("]", "]]") + "]";
    internal static string Literal(string text) => "N'" + text.Replace("'", "''") + "'";
    internal static string Qualify(string schema, string name) => Quote(schema) + "." + Quote(name);
    internal static string DatabaseConnection(string? connection, string database)
    {
        if (string.IsNullOrWhiteSpace(connection)) throw new InvalidOperationException("Connect to SQL Server first.");
        return new SqlConnectionStringBuilder(connection) { InitialCatalog = database }.ConnectionString;
    }
    internal static async Task<DataTable> QueryAsync(string connection, string sql, CancellationToken token = default, params SqlParameter[] parameters)
    {
        using var con = new SqlConnection(connection); await con.OpenAsync(token);
        using var cmd = new SqlCommand(sql, con) { CommandTimeout = 60 }; cmd.Parameters.AddRange(parameters);
        using var reader = await cmd.ExecuteReaderAsync(token);
        var table = new DataTable();
        for (int i = 0; i < reader.FieldCount; i++) { string name = reader.GetName(i); if (string.IsNullOrEmpty(name)) name = "Column" + (i + 1); while (table.Columns.Contains(name)) name += "_"; table.Columns.Add(name, reader.GetFieldType(i)); }
        while (await reader.ReadAsync(token)) { var values = new object[reader.FieldCount]; reader.GetValues(values); table.Rows.Add(values); }
        return table;
    }
    internal static Task<DataTable> DatabasesAsync(string? connection, CancellationToken token = default) => QueryAsync(DatabaseConnection(connection, "master"), "SELECT name FROM sys.databases WHERE database_id > 4 AND state=0 AND HAS_DBACCESS(name)=1 ORDER BY name", token);
    internal static DataTable Filter(DataTable source, string term)
    {
        var result = source.Clone();
        foreach (DataRow row in source.Rows)
            if (term.Length == 0 || row.ItemArray.Any(v => Convert.ToString(v, System.Globalization.CultureInfo.InvariantCulture)?.Contains(term, StringComparison.OrdinalIgnoreCase) == true)) result.ImportRow(row);
        return result;
    }
    internal static void Error(IWin32Window owner, Exception ex) => MessageBox.Show(owner, ex.Message, "SQL Helper", MessageBoxButtons.OK, MessageBoxIcon.Error);
}
