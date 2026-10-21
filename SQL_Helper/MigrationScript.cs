using System.Text;
namespace SQL_Helper;
internal static class MigrationScript
{
    private static bool Constraint(SchemaObject o) => o.Kind is "Primary Key" or "Unique Constraint" or "Foreign Key" or "Check Constraint" or "Index";
    private static bool Module(SchemaObject o) => o.Kind is "View" or "Function" or "Stored Procedure" or "Trigger";
    internal static string Build(SchemaSnapshot source, SchemaSnapshot target, IReadOnlyList<SchemaDifference> differences, bool destructive)
    {
        var changes = differences.Where(d => d.Source != null || destructive).ToList();
        var blockers = new List<string>(); var before = new List<string>(); var body = new List<string>(); var after = new List<string>();
        var drop = new Dictionary<string, SchemaObject>(StringComparer.Ordinal); var recreate = new Dictionary<string, SchemaObject>(StringComparer.Ordinal);
        foreach (var difference in changes)
        {
            var item = difference.Object;
            if (item.BlockedReason.Length > 0) blockers.Add(item.Qualified + ": " + item.BlockedReason);
            if (difference.Target?.BlockedReason.Length > 0) blockers.Add(difference.Target.Qualified + ": " + difference.Target.BlockedReason);
            if (item.Parent.Length > 0 && source.Objects.TryGetValue("Table|" + item.Schema + "||" + item.Parent, out var parent) && parent.BlockedReason.Length > 0) blockers.Add(parent.Qualified + ": " + parent.BlockedReason);
            if (Constraint(item))
            {
                if (difference.Target != null) drop[difference.Target.Key] = difference.Target;
                if (difference.Source != null) recreate[difference.Source.Key] = difference.Source;
            }
            if (item.Kind == "Column")
            {
                foreach (var dependency in target.Objects.Values.Where(o => Constraint(o) && ((o.Schema == item.Schema && o.Parent == item.Parent) || (o.Kind == "Foreign Key" && o.Definition.Contains(SqlTools.Qualify(item.Schema, item.Parent), StringComparison.Ordinal)))))
                { drop[dependency.Key] = dependency; if (source.Objects.TryGetValue(dependency.Key, out var desired)) recreate[desired.Key] = desired; }
                if (difference.Source?.Column is ColumnDefinition a && difference.Target?.Column is ColumnDefinition b && (a.IdentitySeed != b.IdentitySeed || a.IdentityIncrement != b.IdentityIncrement || a.Computed != b.Computed || a.Persisted != b.Persisted || a.RowGuid != b.RowGuid))
                    blockers.Add(item.Qualified + ": changing identity, ROWGUIDCOL, or computed expressions requires a table rebuild that preserves existing data.");
            }
        }
        // Drop foreign keys first, then indexes and keys, before column changes.
        foreach (var item in drop.Values.OrderBy(o => o.Kind == "Foreign Key" ? 0 : o.Kind == "Index" ? 1 : 2)) before.Add(Drop(item));
        foreach (var difference in changes.Where(d => !Constraint(d.Object)).OrderBy(d => d.Source == null ? 100 - Rank(d.Object.Kind) : Rank(d.Object.Kind)))
        {
            var item = difference.Object;
            if (difference.Source == null)
            {
                if (item.Kind == "Column" && item.Column?.DefaultSql.Length > 0) body.Add("ALTER TABLE " + SqlTools.Qualify(item.Schema, item.Parent) + " DROP CONSTRAINT " + SqlTools.Quote(item.Column.DefaultName) + ";");
                if (item.Kind == "Table")
                {
                    foreach (var fk in target.Objects.Values.Where(o => o.Kind == "Foreign Key" && o.Definition.Contains(item.Qualified, StringComparison.Ordinal))) if (!drop.ContainsKey(fk.Key)) { before.Insert(0, Drop(fk)); drop[fk.Key] = fk; }
                }
                body.Add(Drop(item)); continue;
            }
            var desired = difference.Source;
            if (desired.Kind == "Column")
            {
                string table = SqlTools.Qualify(desired.Schema, desired.Parent);
                if (difference.Target?.Column is ColumnDefinition current)
                {
                    if (current.DefaultSql.Length > 0) body.Add("ALTER TABLE " + table + " DROP CONSTRAINT " + SqlTools.Quote(current.DefaultName) + ";");
                    body.Add("ALTER TABLE " + table + " ALTER COLUMN " + desired.Column!.AlterSql + ";");
                    if (desired.Column.DefaultSql.Length > 0) body.Add("ALTER TABLE " + table + " ADD CONSTRAINT " + SqlTools.Quote(desired.Column.DefaultName) + " DEFAULT " + desired.Column.DefaultSql + " FOR " + SqlTools.Quote(desired.Name) + ";");
                }
                else body.Add("ALTER TABLE " + table + " ADD " + desired.Definition + ";");
            }
            else if (Module(desired)) { /* Ordered by SQL dependency graph below. */ }
            else if (difference.Target != null && desired.Kind is "Table Type" or "Alias Type")
            {
                blockers.Add(desired.Qualified + ": a type definition changed. Its dependent parameters/columns must be migrated explicitly before replacing the type.");
            }
            else body.Add(desired.Definition);
        }
        foreach (var item in recreate.Values.OrderBy(o => o.Kind == "Foreign Key" ? 2 : o.Kind == "Index" ? 1 : 0)) after.Add(item.Definition);
        var modules = changes.Where(d => d.Source != null && Module(d.Source)).Select(d => d.Source!).ToDictionary(o => o.Qualified, StringComparer.Ordinal);
        var done = new HashSet<string>(StringComparer.Ordinal); var visiting = new HashSet<string>(StringComparer.Ordinal);
        var moduleSql = new List<string>();
        void Visit(SchemaObject item)
        {
            if (done.Contains(item.Qualified)) return;
            if (!visiting.Add(item.Qualified)) { blockers.Add("Circular SQL module dependency involving " + item.Qualified); return; }
            foreach (string name in item.Dependencies ?? Array.Empty<string>()) if (name != item.Qualified && modules.TryGetValue(name, out var dependency)) Visit(dependency);
            visiting.Remove(item.Qualified); done.Add(item.Qualified);
            moduleSql.Add($"SET ANSI_NULLS {(item.AnsiNulls ? "ON" : "OFF")};\nSET QUOTED_IDENTIFIER {(item.QuotedIdentifiers ? "ON" : "OFF")};\nGO\n" + SqlParserHelper.ToCreateOrAlter(item.Definition));
        }
        foreach (var item in modules.Values) Visit(item);
        var result = new StringBuilder(); result.AppendLine("-- SQL Helper: development → production migration");
        result.AppendLine("-- Source: " + source.Database.Replace("\r", " ").Replace("\n", " ") + "; destination: " + target.Database.Replace("\r", " ").Replace("\n", " "));
        result.AppendLine("-- Review before execution. SQL Helper never executes this script automatically.");
        result.AppendLine("USE " + SqlTools.Quote(target.Database) + ";\nGO\nSET XACT_ABORT ON;\nGO");
        if (blockers.Count > 0)
        {
            // Fail closed: do not return a partially executable migration.
            result.AppendLine("-- Migration needs manual resolution before SQL can be generated:");
            foreach (string message in blockers.Distinct()) result.AppendLine("-- " + message.Replace("\n", " ").Replace("\r", " "));
            result.AppendLine("THROW 51000, 'Resolve the migration blockers listed above, then regenerate.', 1;\nGO");
            return result.ToString();
        }
        result.AppendLine("BEGIN TRANSACTION;\nGO");
        foreach (string sql in before.Concat(body).Concat(moduleSql).Concat(after)) result.AppendLine(sql + "\nGO");
        result.AppendLine("COMMIT TRANSACTION;\nGO");
        if (!destructive)
        {
            var extras = differences.Where(d => d.Source == null).ToList();
            if (extras.Count > 0) { result.AppendLine("-- Production-only objects retained (enable destructive changes to generate removals):"); foreach (var extra in extras) result.AppendLine("-- " + extra.Object.Kind + " " + extra.Object.Qualified.Replace("\n", " ").Replace("\r", " ")); }
        }
        return result.ToString();
    }
    private static int Rank(string kind) => kind switch { "Schema" => 0, "Alias Type" => 1, "Table Type" => 2, "Table" => 3, "Column" => 4, _ => 5 };
    private static string Drop(SchemaObject item) => item.Kind switch
    {
        "Column" => "ALTER TABLE " + SqlTools.Qualify(item.Schema, item.Parent) + " DROP COLUMN " + SqlTools.Quote(item.Name) + ";",
        "Index" => "DROP INDEX " + SqlTools.Quote(item.Name) + " ON " + SqlTools.Qualify(item.Schema, item.Parent) + ";",
        "Primary Key" or "Unique Constraint" or "Foreign Key" or "Check Constraint" => "ALTER TABLE " + SqlTools.Qualify(item.Schema, item.Parent) + " DROP CONSTRAINT " + SqlTools.Quote(item.Name) + ";",
        "Table Type" or "Alias Type" => "DROP TYPE " + item.Qualified + ";",
        "Stored Procedure" => "DROP PROCEDURE " + item.Qualified + ";",
        "Schema" => "DROP SCHEMA " + SqlTools.Quote(item.Name) + ";",
        _ => "DROP " + item.Kind.ToUpperInvariant() + " " + item.Qualified + ";"
    };
}
