using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;

namespace SQL_Helper
{
    public partial class frmGenerateScript : Form
    {
        public DataGridView SourceGrid { get; set; }

        public string DevelopmentDatabase { get; set; }
        public string ProductionDatabase { get; set; }

        public string DevelopmentConnectionString { get; set; }
        public string ProductionConnectionString { get; set; }

        private readonly Dictionary<string, string> _tableSchemaCache = new();
        private readonly Dictionary<string, string> _tableTypeCache = new();
        private readonly Dictionary<string, string> _storedProcCache = new();
        private readonly Dictionary<string, string> _triggerCache = new();

        public frmGenerateScript()
        {
            InitializeComponent();
        }

        private void frmGenerateScript_Load(object sender, EventArgs e)
        {
            GenerateScriptFromGrid();
        }

        private void GenerateScriptFromGrid()
        {
            if (SourceGrid == null)
            {
                richTextBox1.Text = "-- Grid not set.";
                return;
            }

            var sb = new StringBuilder();

            sb.AppendLine("-- ===============================================");
            sb.AppendLine("-- DATABASE SYNC SCRIPT");
            sb.AppendLine($"-- Development DB : {DevelopmentDatabase}");
            sb.AppendLine($"-- Production DB  : {ProductionDatabase}");
            sb.AppendLine($"-- Generated      : {DateTime.Now}");
            sb.AppendLine("-- ===============================================");
            sb.AppendLine();

            using var devConn = new SqlConnection(DevelopmentConnectionString);
            using var prodConn = new SqlConnection(ProductionConnectionString);

            devConn.Open();
            prodConn.Open();

            ProcessRows(sb, devConn, prodConn, "Table");
            ProcessRows(sb, devConn, prodConn, "Table Type");
            ProcessRows(sb, devConn, prodConn, "Stored Procedure", "Procedure");
            ProcessRows(sb, devConn, prodConn, "Trigger");

            richTextBox1.Text = sb.ToString();
        }

        private void ProcessRows(
            StringBuilder sb,
            SqlConnection devConn,
            SqlConnection prodConn,
            params string[] objectTypes)
        {
            foreach (DataGridViewRow row in SourceGrid.Rows)
            {
                if (row.IsNewRow) continue;

                string server = GetCellValue(row, "Server");
                string database = GetCellValue(row, "Database");
                string type = GetCellValue(row, "Type");
                string name = GetCellValue(row, "Name");

                if (!objectTypes.Any(t => t.Equals(type, StringComparison.OrdinalIgnoreCase)))
                    continue;

                SqlConnection sourceConn =
                    server.Equals("Development", StringComparison.OrdinalIgnoreCase)
                        ? prodConn
                        : devConn;

                sb.AppendLine($"USE [{database}]");
                sb.AppendLine("GO");

                if (type.Equals("Table", StringComparison.OrdinalIgnoreCase))
                {
                    if (!_tableSchemaCache.TryGetValue(name, out var script))
                    {
                        script = GetTableSchema(sourceConn, name);
                        _tableSchemaCache[name] = script;
                    }
                    sb.AppendLine(script);
                }
                else if (type.Equals("Table Type", StringComparison.OrdinalIgnoreCase))
                {
                    if (!_tableTypeCache.TryGetValue(name, out var script))
                    {
                        script = GetTableTypeSchema(sourceConn, name);
                        _tableTypeCache[name] = script;
                    }
                    sb.AppendLine(script);
                }
                else if (type.Equals("Stored Procedure", StringComparison.OrdinalIgnoreCase) ||
                         type.Equals("Procedure", StringComparison.OrdinalIgnoreCase))
                {
                    string procName = name.Replace("[dbo].", "").Replace("dbo.", "");

                    if (!_storedProcCache.TryGetValue(procName, out var script))
                    {
                        script = GetStoredProcedureScript(sourceConn, procName);
                        _storedProcCache[procName] = script;
                    }
                    sb.AppendLine(script);
                }
                else if (type.Equals("Trigger", StringComparison.OrdinalIgnoreCase))
                {
                    if (!_triggerCache.TryGetValue(name, out var script))
                    {
                        script = GetTriggerScript(sourceConn, name);
                        _triggerCache[name] = script;
                    }
                    sb.AppendLine(script);
                }

                sb.AppendLine("GO");
                sb.AppendLine();
            }
        }

        // ===================== TABLE =====================
        private string GetTableSchema(SqlConnection conn, string tableName)
        {
            var columns = new List<string>();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
SELECT 
    c.name,
    t.name AS DataType,
    c.max_length,
    c.precision,
    c.scale,
    c.is_nullable,
    c.is_identity
FROM sys.columns c
JOIN sys.types t ON c.user_type_id = t.user_type_id
JOIN sys.tables tb ON c.object_id = tb.object_id
WHERE tb.name = @Table
ORDER BY c.column_id";

            cmd.Parameters.AddWithValue("@Table", tableName);

            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                var col = $"[{r["name"]}] {BuildType(r)}";
                if ((bool)r["is_identity"]) col += " IDENTITY(1,1)";
                col += (bool)r["is_nullable"] ? " NULL" : " NOT NULL";
                columns.Add(col);
            }

            return $@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = N'{tableName}')
BEGIN
    CREATE TABLE [dbo].[{tableName}]
    (
        {string.Join(",\n        ", columns)}
    )
END";
        }

        // ===================== TABLE TYPE =====================
        private string GetTableTypeSchema(SqlConnection conn, string typeName)
        {
            var columns = new List<string>();

            string schemaName = "dbo";
            string pureTypeName = typeName;

            if (typeName.Contains("."))
            {
                var parts = typeName.Split('.');
                schemaName = parts[0].Replace("[", "").Replace("]", "");
                pureTypeName = parts[1].Replace("[", "").Replace("]", "");
            }

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
SELECT 
    c.name,
    t.name AS DataType,
    c.max_length,
    c.precision,
    c.scale,
    c.is_nullable
FROM sys.table_types tt
JOIN sys.columns c ON tt.type_table_object_id = c.object_id
JOIN sys.types t ON c.user_type_id = t.user_type_id
JOIN sys.schemas s ON tt.schema_id = s.schema_id
WHERE tt.name = @TypeName
  AND s.name = @SchemaName
ORDER BY c.column_id";

            cmd.Parameters.AddWithValue("@TypeName", pureTypeName);
            cmd.Parameters.AddWithValue("@SchemaName", schemaName);

            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                var col = $"[{r["name"]}] {BuildType(r)}";
                col += (bool)r["is_nullable"] ? " NULL" : " NOT NULL";
                columns.Add(col);
            }

            return $@"
IF NOT EXISTS (
    SELECT 1 
    FROM sys.table_types tt
    JOIN sys.schemas s ON tt.schema_id = s.schema_id
    WHERE tt.name = N'{pureTypeName}'
      AND s.name = N'{schemaName}'
)
BEGIN
    CREATE TYPE [{schemaName}].[{pureTypeName}] AS TABLE
    (
        {string.Join(",\n        ", columns)}
    )
END";
        }

        // ===================== STORED PROCEDURE =====================
        private string GetStoredProcedureScript(SqlConnection conn, string name)
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
            SELECT definition 
            FROM sys.sql_modules 
            WHERE object_id = OBJECT_ID(@Name)";
                        cmd.Parameters.AddWithValue("@Name", $"dbo.{name}");

            var def = cmd.ExecuteScalar()?.ToString();
            if (string.IsNullOrWhiteSpace(def))
                return string.Empty;

            // Normalize line endings
            def = def.Replace("\r\n", "\n");

            // Force CREATE OR ALTER (case-insensitive, safe)
            def = System.Text.RegularExpressions.Regex.Replace(
                def,
                @"\bCREATE\s+PROC(EDURE)?\b|\bALTER\s+PROC(EDURE)?\b",
                "CREATE OR ALTER PROCEDURE",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            return def;
        }


        // ===================== TRIGGER =====================
        private string GetTriggerScript(SqlConnection conn, string name)
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT definition FROM sys.sql_modules WHERE object_id = OBJECT_ID(@Name)";
            cmd.Parameters.AddWithValue("@Name", name);

            return cmd.ExecuteScalar()?.ToString() ?? "";
        }

        // ===================== HELPERS =====================
        private static string BuildType(SqlDataReader r)
        {
            string type = r["DataType"].ToString();
            int maxLength = Convert.ToInt32(r["max_length"]);
            int precision = Convert.ToInt32(r["precision"]);
            int scale = Convert.ToInt32(r["scale"]);

            if (type is "varchar" or "nvarchar" or "char" or "nchar")
            {
                string len = maxLength == -1
                    ? "MAX"
                    : (type.StartsWith("n") ? (maxLength / 2).ToString() : maxLength.ToString());

                return $"{type}({len})";
            }

            if (type is "decimal" or "numeric")
                return $"{type}({precision},{scale})";

            return type;
        }

        private string GetCellValue(DataGridViewRow row, string col)
        {
            if (!row.DataGridView.Columns.Contains(col)) return "";
            return row.Cells[col]?.Value?.ToString() ?? "";
        }
    }
}
