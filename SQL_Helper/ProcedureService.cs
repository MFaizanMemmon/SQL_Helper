using Microsoft.Data.SqlClient;
using System.Data;
using System.Globalization;
using System.Text.Json;
namespace SQL_Helper;
internal static class ParameterValues
{
    internal static object Parse(SqlParameter parameter, string text, bool isNull)
    {
        if (isNull) return DBNull.Value;
        var culture = CultureInfo.InvariantCulture;
        try
        {
            return parameter.SqlDbType switch
            {
                SqlDbType.BigInt => long.Parse(text, culture), SqlDbType.Int => int.Parse(text, culture), SqlDbType.SmallInt => short.Parse(text, culture), SqlDbType.TinyInt => byte.Parse(text, culture),
                SqlDbType.Bit => text.Trim().ToLowerInvariant() switch { "true" or "1" => true, "false" or "0" => false, _ => throw new FormatException("Use true/false or 1/0.") },
                SqlDbType.Decimal or SqlDbType.Money or SqlDbType.SmallMoney => decimal.Parse(text, culture), SqlDbType.Float => double.Parse(text, culture), SqlDbType.Real => float.Parse(text, culture),
                SqlDbType.Date or SqlDbType.DateTime or SqlDbType.DateTime2 or SqlDbType.SmallDateTime => DateTime.Parse(text, culture, DateTimeStyles.RoundtripKind),
                SqlDbType.DateTimeOffset => DateTimeOffset.Parse(text, culture, DateTimeStyles.RoundtripKind), SqlDbType.Time => TimeSpan.Parse(text, culture), SqlDbType.UniqueIdentifier => Guid.Parse(text),
                SqlDbType.Binary or SqlDbType.VarBinary or SqlDbType.Image or SqlDbType.Timestamp => Convert.FromHexString(text.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? text[2..] : text),
                SqlDbType.Structured => ParseRows(parameter, text),
                SqlDbType.Udt => throw new NotSupportedException("CLR user-defined parameters require a provider-specific value and cannot be entered as text."),
                _ => text
            };
        }
        catch (Exception ex) when (ex is FormatException or OverflowException or JsonException) { throw new FormatException($"{parameter.ParameterName} ({parameter.SqlDbType}): {ex.Message}", ex); }
    }
    private static DataTable ParseRows(SqlParameter parameter, string text)
    {
        if (parameter.Value is not DataTable schema) throw new InvalidOperationException("Table parameter schema is unavailable.");
        var table = schema.Clone(); using var document = JsonDocument.Parse(text);
        if (document.RootElement.ValueKind != JsonValueKind.Array) throw new FormatException("Enter a JSON array of row objects.");
        foreach (var element in document.RootElement.EnumerateArray())
        {
            var row = table.NewRow();
            foreach (DataColumn column in table.Columns)
            {
                if (!element.TryGetProperty(column.ColumnName, out var value) || value.ValueKind == JsonValueKind.Null) row[column] = DBNull.Value;
                else row[column] = column.DataType == typeof(Guid) ? Guid.Parse(value.GetString()!) : column.DataType == typeof(byte[]) ? Convert.FromHexString(value.GetString()!) : Convert.ChangeType(value.ToString(), column.DataType, CultureInfo.InvariantCulture);
            }
            table.Rows.Add(row);
        }
        return table;
    }
}
internal sealed record ProcedureResults(IReadOnlyList<DataTable> Tables, DataTable Outputs, int RecordsAffected);
internal static class ProcedureService
{
    internal static async Task<List<SqlParameter>> ParametersAsync(string connection, string name, CancellationToken token)
    {
        using var con = new SqlConnection(connection); await con.OpenAsync(token);
        using var command = new SqlCommand(name, con) { CommandType = CommandType.StoredProcedure };
        await Task.Run(() => SqlCommandBuilder.DeriveParameters(command), token);
        var parameters = command.Parameters.Cast<SqlParameter>().Where(p => p.Direction != ParameterDirection.ReturnValue).Select(p => (SqlParameter)((ICloneable)p).Clone()).ToList();
        foreach (var parameter in parameters.Where(p => p.SqlDbType == SqlDbType.Structured))
        {
            // SqlClient's derivation can include a database component; SQL Server expects schema.type.
            var type = await SqlTools.QueryAsync(connection, "SELECT QUOTENAME(SCHEMA_NAME(schema_id))+'.'+QUOTENAME(name) FROM sys.table_types WHERE user_type_id=(SELECT user_type_id FROM sys.parameters WHERE object_id=OBJECT_ID(@proc) AND name=@name)", token, new SqlParameter("@proc", name), new SqlParameter("@name", parameter.ParameterName));
            if (type.Rows.Count == 0) throw new InvalidOperationException("Cannot determine the table parameter type.");
            parameter.TypeName = (string)type.Rows[0][0];
            var columns = await SqlTools.QueryAsync(connection, "SELECT c.name,TYPE_NAME(c.system_type_id) AS TypeName FROM sys.table_types t JOIN sys.columns c ON t.type_table_object_id=c.object_id WHERE t.user_type_id=TYPE_ID(@type) ORDER BY c.column_id", token, new SqlParameter("@type", parameter.TypeName));
            var schema = new DataTable(); foreach (DataRow column in columns.Rows) schema.Columns.Add((string)column[0], ClrType((string)column[1])); parameter.Value = schema;
        }
        return parameters;
    }
    internal static Type ClrType(string type) => type switch { "bigint" => typeof(long), "int" => typeof(int), "smallint" => typeof(short), "tinyint" => typeof(byte), "bit" => typeof(bool), "decimal" or "numeric" or "money" or "smallmoney" => typeof(decimal), "float" => typeof(double), "real" => typeof(float), "date" or "datetime" or "datetime2" or "smalldatetime" => typeof(DateTime), "datetimeoffset" => typeof(DateTimeOffset), "time" => typeof(TimeSpan), "uniqueidentifier" => typeof(Guid), "binary" or "varbinary" or "image" or "timestamp" => typeof(byte[]), _ => typeof(string) };
    internal static async Task<ProcedureResults> ExecuteAsync(string connection, string name, IEnumerable<SqlParameter> parameters, CancellationToken token)
    {
        using var con = new SqlConnection(connection); await con.OpenAsync(token);
        using var command = new SqlCommand(name, con) { CommandType = CommandType.StoredProcedure, CommandTimeout = 60 };
        foreach (var parameter in parameters) { var clone = (SqlParameter)((ICloneable)parameter).Clone(); clone.Value ??= DBNull.Value; command.Parameters.Add(clone); }
        var returnValue = command.Parameters.Add("@RETURN_VALUE", SqlDbType.Int); returnValue.Direction = ParameterDirection.ReturnValue;
        var tables = new List<DataTable>(); int affected;
        using (var reader = await command.ExecuteReaderAsync(token))
        {
            do
            {
                if (reader.FieldCount == 0) continue;
                var table = new DataTable("Result " + (tables.Count + 1));
                for (int i = 0; i < reader.FieldCount; i++) { string column = reader.GetName(i); if (string.IsNullOrWhiteSpace(column)) column = "Column" + (i + 1); while (table.Columns.Contains(column)) column += "_"; table.Columns.Add(column, reader.GetFieldType(i)); }
                while (await reader.ReadAsync(token)) { var row = new object[reader.FieldCount]; reader.GetValues(row); table.Rows.Add(row); }
                tables.Add(table);
            } while (await reader.NextResultAsync(token));
            affected = reader.RecordsAffected;
        }
        var outputs = new DataTable("Output parameters"); outputs.Columns.Add("Parameter"); outputs.Columns.Add("Value");
        foreach (SqlParameter parameter in command.Parameters) if (parameter.Direction != ParameterDirection.Input) outputs.Rows.Add(parameter.ParameterName, parameter.Value == DBNull.Value ? "NULL" : Convert.ToString(parameter.Value, CultureInfo.InvariantCulture));
        return new ProcedureResults(tables, outputs, affected);
    }
}
