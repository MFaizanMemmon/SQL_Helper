using Microsoft.Data.SqlClient;
using System.Data;
using System.Globalization;
using System.Text;
namespace SQL_Helper;

internal sealed record ColumnDefinition(string Name, string TypeSql, bool Nullable, string DefaultSql, string DefaultName, string Computed, bool Persisted, string IdentitySeed, string IdentityIncrement, bool RowGuid, string Collation, bool Sparse)
{
    internal string Sql => SqlTools.Quote(Name) + (Computed.Length > 0 ? " AS " + Computed + (Persisted ? " PERSISTED" : "") : " " + TypeSql + (Collation.Length > 0 ? " COLLATE " + SafeCollation(Collation) : "") + (IdentitySeed.Length > 0 ? $" IDENTITY({IdentitySeed},{IdentityIncrement})" : "") + (RowGuid ? " ROWGUIDCOL" : "") + (Sparse ? " SPARSE" : "") + (Nullable ? " NULL" : " NOT NULL") + (DefaultSql.Length > 0 ? " CONSTRAINT " + SqlTools.Quote(DefaultName) + " DEFAULT " + DefaultSql : ""));
    private static string SafeCollation(string value) => System.Text.RegularExpressions.Regex.IsMatch(value, "^[A-Za-z0-9_]+$") ? value : throw new InvalidOperationException("Invalid collation metadata.");
    internal string AlterSql => SqlTools.Quote(Name) + " " + TypeSql + (Collation.Length > 0 ? " COLLATE " + SafeCollation(Collation) : "") + (Sparse ? " SPARSE" : "") + (Nullable ? " NULL" : " NOT NULL");
}
internal sealed record SchemaObject(string Kind, string Schema, string Name, string Definition, string Fingerprint, string Parent = "", ColumnDefinition? Column = null, string BlockedReason = "", bool AnsiNulls = true, bool QuotedIdentifiers = true, IReadOnlyList<string>? Dependencies = null)
{
    internal string Qualified => SqlTools.Qualify(Schema, Name);
    internal string Key => Kind + "|" + Schema + "|" + Parent + "|" + Name;
}
internal sealed record SchemaSnapshot(string Database, IReadOnlyDictionary<string, SchemaObject> Objects);
internal sealed record SchemaDifference(SchemaObject? Source, SchemaObject? Target)
{
    internal SchemaObject Object => Source ?? Target!;
    internal string Status => Object.BlockedReason.Length > 0 ? "Manual review required" : Source == null ? "Only in production" : Target == null ? "Missing in production" : "Definition differs";
}
internal static class SchemaService
{
    private static string Text(DataRow row, string name) => row.IsNull(name) ? "" : Convert.ToString(row[name], CultureInfo.InvariantCulture)!;
    private static bool Flag(DataRow row, string name) => !row.IsNull(name) && Convert.ToBoolean(row[name], CultureInfo.InvariantCulture);
    internal static string FormatType(string type, string schema, bool userDefined, int length, byte precision, byte scale)
    {
        if (userDefined) return SqlTools.Qualify(schema, type);
        return type switch
        {
            "nvarchar" or "nchar" => $"{type}({(length == -1 ? "MAX" : (length / 2).ToString(CultureInfo.InvariantCulture))})",
            "varchar" or "char" or "varbinary" or "binary" => $"{type}({(length == -1 ? "MAX" : length.ToString(CultureInfo.InvariantCulture))})",
            "decimal" or "numeric" => $"{type}({precision},{scale})",
            "datetime2" or "datetimeoffset" or "time" => $"{type}({scale})",
            "float" => $"float({precision})", _ => type
        };
    }
    internal static async Task<SchemaSnapshot> ReadAsync(string connection, CancellationToken token)
    {
        string database = new SqlConnectionStringBuilder(connection).InitialCatalog;
        var objects = new Dictionary<string, SchemaObject>(StringComparer.Ordinal);
        void Add(SchemaObject item) => objects.Add(item.Key, item);
        var schemas = await SqlTools.QueryAsync(connection, "SELECT name FROM sys.schemas WHERE principal_id NOT IN(SELECT principal_id FROM sys.database_principals WHERE is_fixed_role=1) AND name NOT IN ('dbo','guest','sys','INFORMATION_SCHEMA')", token);
        foreach (DataRow row in schemas.Rows) { string name = (string)row[0]; Add(new("Schema", name, name, "CREATE SCHEMA " + SqlTools.Quote(name) + ";", name)); }
        var aliases = await SqlTools.QueryAsync(connection, "SELECT SCHEMA_NAME(schema_id) AS SchemaName,name,TYPE_NAME(system_type_id) AS BaseType,max_length,precision,scale,is_nullable,is_assembly_type FROM sys.types WHERE is_user_defined=1 AND is_table_type=0", token);
        foreach (DataRow row in aliases.Rows)
        {
            string schema = Text(row, "SchemaName"), name = Text(row, "name"); bool assembly = Flag(row, "is_assembly_type");
            string sql = "CREATE TYPE " + SqlTools.Qualify(schema, name) + " FROM " + FormatType(Text(row, "BaseType"), "sys", false, Convert.ToInt32(row["max_length"]), (byte)row["precision"], (byte)row["scale"]) + (Flag(row, "is_nullable") ? " NULL;" : " NOT NULL;");
            Add(new("Alias Type", schema, name, sql, sql, BlockedReason: assembly ? "CLR types require their assembly deployment." : ""));
        }
        var parents = await SqlTools.QueryAsync(connection, @"
SELECT t.object_id,SCHEMA_NAME(t.schema_id) AS SchemaName,t.name,'Table' AS Kind,
CASE WHEN t.temporal_type<>0 OR t.is_memory_optimized=1 OR t.is_filetable=1 THEN 1 ELSE 0 END AS Unsupported
FROM sys.tables t WHERE t.is_ms_shipped=0
UNION ALL SELECT type_table_object_id,SCHEMA_NAME(schema_id),name,'Table Type',is_memory_optimized FROM sys.table_types", token);
        var columnData = await SqlTools.QueryAsync(connection, @"
SELECT c.object_id,c.column_id,c.name,ty.name AS TypeName,SCHEMA_NAME(ty.schema_id) AS TypeSchema,ty.is_user_defined,c.max_length,c.precision,c.scale,c.is_nullable,
d.definition AS DefaultSql,d.name AS DefaultName,cc.definition AS Computed,cc.is_persisted,
CONVERT(nvarchar(100),ic.seed_value) AS IdentitySeed,CONVERT(nvarchar(100),ic.increment_value) AS IdentityIncrement,
c.is_rowguidcol,c.collation_name,c.is_sparse,c.generated_always_type,c.is_hidden,c.is_filestream
FROM sys.columns c JOIN sys.types ty ON ty.user_type_id=c.user_type_id
LEFT JOIN sys.default_constraints d ON d.object_id=c.default_object_id
LEFT JOIN sys.computed_columns cc ON cc.object_id=c.object_id AND cc.column_id=c.column_id
LEFT JOIN sys.identity_columns ic ON ic.object_id=c.object_id AND ic.column_id=c.column_id
ORDER BY c.object_id,c.column_id", token);
        var keyData = await SqlTools.QueryAsync(connection, @"
SELECT k.parent_object_id,k.name,k.type,i.type_desc,i.is_disabled,ic.key_ordinal,ic.is_descending_key,c.name AS ColumnName
FROM sys.key_constraints k JOIN sys.indexes i ON i.object_id=k.parent_object_id AND i.index_id=k.unique_index_id
JOIN sys.index_columns ic ON ic.object_id=i.object_id AND ic.index_id=i.index_id AND ic.key_ordinal>0
JOIN sys.columns c ON c.object_id=ic.object_id AND c.column_id=ic.column_id
ORDER BY k.parent_object_id,k.name,ic.key_ordinal", token);
        var checkData = await SqlTools.QueryAsync(connection, "SELECT parent_object_id,name,definition,is_disabled,is_not_trusted,is_not_for_replication FROM sys.check_constraints", token);
        foreach (DataRow parent in parents.Rows)
        {
            int id = (int)parent["object_id"]; string schema = Text(parent, "SchemaName"), name = Text(parent, "name"), kind = Text(parent, "Kind");
            string qualified = SqlTools.Qualify(schema, name);
            var columns = new List<ColumnDefinition>(); bool unsupported = Flag(parent, "Unsupported");
            foreach (DataRow row in columnData.Rows.Cast<DataRow>().Where(r => (int)r["object_id"] == id))
            {
                var column = new ColumnDefinition(Text(row, "name"), FormatType(Text(row, "TypeName"), Text(row, "TypeSchema"), Flag(row, "is_user_defined"), Convert.ToInt32(row["max_length"]), (byte)row["precision"], (byte)row["scale"]), Flag(row, "is_nullable"), Text(row, "DefaultSql"), Text(row, "DefaultName"), Text(row, "Computed"), Flag(row, "is_persisted"), Text(row, "IdentitySeed"), Text(row, "IdentityIncrement"), Flag(row, "is_rowguidcol"), Flag(row, "is_user_defined") ? "" : Text(row, "collation_name"), Flag(row, "is_sparse"));
                columns.Add(column); unsupported |= Convert.ToInt32(row["generated_always_type"]) != 0 || Flag(row, "is_hidden") || Flag(row, "is_filestream");
                if (kind == "Table") Add(new("Column", schema, column.Name, column.Sql, column.Sql, name, column));
            }
            var constraints = new List<string>();
            foreach (var key in keyData.Rows.Cast<DataRow>().Where(r => (int)r["parent_object_id"] == id).GroupBy(r => Text(r, "name")))
            {
                var row = key.First(); string keyName = key.Key; string keyKind = Text(row, "type").Trim() == "PK" ? "Primary Key" : "Unique Constraint";
                string definition = (kind == "Table Type" ? "" : "CONSTRAINT " + SqlTools.Quote(keyName)) + (keyKind == "Primary Key" ? " PRIMARY KEY " : " UNIQUE ") + Text(row, "type_desc").Replace('_', ' ') + " (" + string.Join(", ", key.Select(r => SqlTools.Quote(Text(r, "ColumnName")) + (Flag(r, "is_descending_key") ? " DESC" : " ASC"))) + ")";
                if (kind == "Table Type") constraints.Add(definition);
                else Add(new(keyKind, schema, keyName, "ALTER TABLE " + qualified + " ADD " + definition + ";", definition, name));
            }
            foreach (DataRow row in checkData.Rows.Cast<DataRow>().Where(r => (int)r["parent_object_id"] == id))
            {
                string definition = (kind == "Table Type" ? "" : "CONSTRAINT " + SqlTools.Quote(Text(row, "name"))) + " CHECK " + (Flag(row, "is_not_for_replication") ? "NOT FOR REPLICATION " : "") + Text(row, "definition");
                if (kind == "Table Type") constraints.Add(definition);
                else
                {
                    string sql = "ALTER TABLE " + qualified + (Flag(row, "is_not_trusted") ? " WITH NOCHECK" : " WITH CHECK") + " ADD " + definition + ";" + (Flag(row, "is_disabled") ? "\nALTER TABLE " + qualified + " NOCHECK CONSTRAINT " + SqlTools.Quote(Text(row, "name")) + ";" : "");
                    Add(new("Check Constraint", schema, Text(row, "name"), sql, sql, name));
                }
            }
            string create = (kind == "Table" ? "CREATE TABLE " : "CREATE TYPE ") + qualified + (kind == "Table Type" ? " AS TABLE" : "") + " (\n    " + string.Join(",\n    ", columns.Select(c => c.Sql).Concat(constraints)) + "\n);";
            Add(new(kind, schema, name, create, kind == "Table" ? "Table" : create, BlockedReason: unsupported ? "Temporal, memory-optimized, FILETABLE/FILESTREAM, or generated columns need a specialized migration." : ""));
        }
        var modules = await SqlTools.QueryAsync(connection, @"
SELECT SCHEMA_NAME(o.schema_id) AS SchemaName,o.name,o.type,m.definition,m.uses_ansi_nulls,m.uses_quoted_identifier,tr.is_disabled,OBJECT_SCHEMA_NAME(tr.parent_id) AS TriggerSchema,OBJECT_NAME(tr.parent_id) AS TriggerTable
FROM sys.objects o LEFT JOIN sys.sql_modules m ON m.object_id=o.object_id LEFT JOIN sys.triggers tr ON tr.object_id=o.object_id
WHERE o.is_ms_shipped=0 AND o.type IN ('P','V','FN','IF','TF','TR')", token);
        foreach (DataRow row in modules.Rows)
        {
            string kind = Text(row, "type").Trim() switch { "P" => "Stored Procedure", "V" => "View", "TR" => "Trigger", _ => "Function" };
            string definition = Text(row, "definition");
            if (kind == "Trigger" && Flag(row, "is_disabled")) definition += "\nGO\nDISABLE TRIGGER " + SqlTools.Qualify(Text(row, "SchemaName"), Text(row, "name")) + " ON " + SqlTools.Qualify(Text(row, "TriggerSchema"), Text(row, "TriggerTable")) + ";";
            // Trigger state is applied separately below, using its owning table.
            string fingerprint = SqlParserHelper.Normalize(definition) + "|ANSI=" + Flag(row, "uses_ansi_nulls") + "|QUOTED=" + Flag(row, "uses_quoted_identifier");
            Add(new(kind, Text(row, "SchemaName"), Text(row, "name"), definition, fingerprint, BlockedReason: definition.Length == 0 ? "Definition is encrypted or VIEW DEFINITION permission is missing." : "", AnsiNulls: Flag(row, "uses_ansi_nulls"), QuotedIdentifiers: Flag(row, "uses_quoted_identifier")));
        }
        var fkData = await SqlTools.QueryAsync(connection, @"
SELECT fk.parent_object_id,fk.name,SCHEMA_NAME(pt.schema_id) AS SchemaName,pt.name AS ParentName,
SCHEMA_NAME(rt.schema_id) AS RefSchema,rt.name AS RefName,pc.name AS ParentColumn,rc.name AS RefColumn,
fk.delete_referential_action_desc,fk.update_referential_action_desc,fk.is_disabled,fk.is_not_trusted,fk.is_not_for_replication,fkc.constraint_column_id
FROM sys.foreign_keys fk JOIN sys.tables pt ON pt.object_id=fk.parent_object_id JOIN sys.tables rt ON rt.object_id=fk.referenced_object_id
JOIN sys.foreign_key_columns fkc ON fkc.constraint_object_id=fk.object_id JOIN sys.columns pc ON pc.object_id=fkc.parent_object_id AND pc.column_id=fkc.parent_column_id
JOIN sys.columns rc ON rc.object_id=fkc.referenced_object_id AND rc.column_id=fkc.referenced_column_id ORDER BY fk.object_id,fkc.constraint_column_id", token);
        foreach (var group in fkData.Rows.Cast<DataRow>().GroupBy(r => Text(r, "SchemaName") + "|" + Text(r, "ParentName") + "|" + Text(r, "name")))
        {
            var row = group.First(); string table = SqlTools.Qualify(Text(row, "SchemaName"), Text(row, "ParentName")), name = Text(row, "name");
            string sql = "ALTER TABLE " + table + (Flag(row, "is_not_trusted") ? " WITH NOCHECK" : " WITH CHECK") + " ADD CONSTRAINT " + SqlTools.Quote(name) + " FOREIGN KEY (" + string.Join(",", group.Select(r => SqlTools.Quote(Text(r, "ParentColumn")))) + ") REFERENCES " + SqlTools.Qualify(Text(row, "RefSchema"), Text(row, "RefName")) + " (" + string.Join(",", group.Select(r => SqlTools.Quote(Text(r, "RefColumn")))) + ") ON DELETE " + Text(row, "delete_referential_action_desc").Replace('_', ' ') + " ON UPDATE " + Text(row, "update_referential_action_desc").Replace('_', ' ') + (Flag(row, "is_not_for_replication") ? " NOT FOR REPLICATION" : "") + ";";
            if (Flag(row, "is_disabled")) sql += "\nALTER TABLE " + table + " NOCHECK CONSTRAINT " + SqlTools.Quote(name) + ";";
            Add(new("Foreign Key", Text(row, "SchemaName"), name, sql, sql, Text(row, "ParentName")));
        }
        var indexes = await SqlTools.QueryAsync(connection, @"
SELECT SCHEMA_NAME(t.schema_id) AS SchemaName,t.name AS ParentName,i.name,i.type,i.type_desc,i.is_unique,i.filter_definition,i.is_disabled,ic.key_ordinal,ic.is_descending_key,ic.is_included_column,ic.index_column_id,c.name AS ColumnName
FROM sys.indexes i JOIN sys.tables t ON t.object_id=i.object_id LEFT JOIN sys.index_columns ic ON ic.object_id=i.object_id AND ic.index_id=i.index_id LEFT JOIN sys.columns c ON c.object_id=ic.object_id AND c.column_id=ic.column_id
WHERE i.index_id>0 AND i.is_primary_key=0 AND i.is_unique_constraint=0 AND i.is_hypothetical=0 ORDER BY t.object_id,i.index_id,ic.index_column_id", token);
        foreach (var group in indexes.Rows.Cast<DataRow>().GroupBy(r => Text(r, "SchemaName") + "|" + Text(r, "ParentName") + "|" + Text(r, "name")))
        {
            var row = group.First(); string schema = Text(row, "SchemaName"), parent = Text(row, "ParentName"), name = Text(row, "name");
            bool supported = Convert.ToInt32(row["type"]) is 1 or 2;
            string sql = "CREATE " + (Flag(row, "is_unique") ? "UNIQUE " : "") + Text(row, "type_desc").Replace('_', ' ') + " INDEX " + SqlTools.Quote(name) + " ON " + SqlTools.Qualify(schema, parent) + " (" + string.Join(",", group.Where(r => !Flag(r, "is_included_column") && Convert.ToInt32(r["key_ordinal"]) > 0).OrderBy(r => Convert.ToInt32(r["key_ordinal"])).Select(r => SqlTools.Quote(Text(r, "ColumnName")) + (Flag(r, "is_descending_key") ? " DESC" : " ASC"))) + ")";
            var included = group.Where(r => Flag(r, "is_included_column")).Select(r => SqlTools.Quote(Text(r, "ColumnName"))).ToList(); if (included.Count > 0) sql += " INCLUDE (" + string.Join(",", included) + ")";
            if (Text(row, "filter_definition").Length > 0) sql += " WHERE " + Text(row, "filter_definition"); sql += ";";
            if (Flag(row, "is_disabled")) sql += "\nALTER INDEX " + SqlTools.Quote(name) + " ON " + SqlTools.Qualify(schema, parent) + " DISABLE;";
            Add(new("Index", schema, name, sql, sql, parent, BlockedReason: supported ? "" : "XML/spatial/columnstore indexes need a specialized migration."));
        }
        var dependencies = await SqlTools.QueryAsync(connection, "SELECT DISTINCT OBJECT_SCHEMA_NAME(referencing_id) AS OwnerSchema,OBJECT_NAME(referencing_id) AS OwnerName,OBJECT_SCHEMA_NAME(referenced_id) AS RefSchema,OBJECT_NAME(referenced_id) AS RefName FROM sys.sql_expression_dependencies WHERE referenced_id IS NOT NULL", token);
        foreach (var item in objects.Values.Where(o => o.Kind is "View" or "Function" or "Stored Procedure" or "Trigger").ToList())
        {
            var names = dependencies.Rows.Cast<DataRow>().Where(r => Text(r, "OwnerSchema") == item.Schema && Text(r, "OwnerName") == item.Name).Select(r => SqlTools.Qualify(Text(r, "RefSchema"), Text(r, "RefName"))).ToList();
            objects[item.Key] = item with { Dependencies = names };
        }
        return new SchemaSnapshot(database, objects);
    }
    internal static List<SchemaDifference> Compare(SchemaSnapshot source, SchemaSnapshot target)
    {
        var result = new List<SchemaDifference>();
        foreach (string key in source.Objects.Keys.Union(target.Objects.Keys, StringComparer.Ordinal).OrderBy(k => k, StringComparer.Ordinal))
        {
            source.Objects.TryGetValue(key, out var first); target.Objects.TryGetValue(key, out var second);
            if (first?.Kind == "Column" || second?.Kind == "Column")
            {
                var column = first ?? second!; string parent = "Table|" + column.Schema + "||" + column.Parent;
                if (!source.Objects.ContainsKey(parent) || !target.Objects.ContainsKey(parent)) continue;
            }
            if (first == null || second == null || first.Fingerprint != second.Fingerprint || first.BlockedReason != second.BlockedReason || first.BlockedReason.Length > 0) result.Add(new(first, second));
        }
        return result;
    }
}
