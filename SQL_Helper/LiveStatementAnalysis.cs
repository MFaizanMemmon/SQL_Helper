using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SQL_Helper;

internal sealed record LiveTableOperation(string Operation, string Table);

/// <summary>Extracts statement targets/references, not proof of committed row changes.</summary>
internal static class LiveStatementAnalysis
{
    internal static IReadOnlyList<LiveTableOperation> Analyze(string statement)
    {
        if (string.IsNullOrWhiteSpace(statement)) return Array.Empty<LiveTableOperation>();
        var parser = new TSql170Parser(true);
        var fragment = parser.Parse(new StringReader(statement), out var errors);
        if (errors.Count != 0) return Array.Empty<LiveTableOperation>();
        var visitor = new Visitor();
        fragment.Accept(visitor);
        return visitor.Results;
    }

    private sealed class Visitor : TSqlFragmentVisitor
    {
        internal List<LiveTableOperation> Results { get; } = new();
        private readonly HashSet<string> ctes = new(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<NamedTableReference> targets = new();
        private static string Name(NamedTableReference table) =>
            string.Join(".", table.SchemaObject.Identifiers.Select(i => SqlTools.Quote(i.Value)));

        private void Add(string operation, NamedTableReference table)
        {
            var result = new LiveTableOperation(operation, Name(table));
            if (!Results.Contains(result)) Results.Add(result);
        }

        private void Target(string operation, TableReference? target, FromClause? from = null)
        {
            if (target is not NamedTableReference named) return;
            targets.Add(named);
            // UPDATE/DELETE aliases may refer to a table in FROM.
            if (named.SchemaObject.Identifiers.Count == 1 && from != null)
            {
                var finder = new AliasFinder(named.SchemaObject.BaseIdentifier.Value);
                from.Accept(finder);
                if (finder.Match != null) { targets.Add(finder.Match); Add(operation, finder.Match); return; }
            }
            if (!ctes.Contains(named.SchemaObject.BaseIdentifier.Value) || named.SchemaObject.Identifiers.Count > 1)
                Add(operation, named);
        }

        public override void ExplicitVisit(CommonTableExpression node)
        {
            ctes.Add(node.ExpressionName.Value);
            base.ExplicitVisit(node);
        }
        public override void ExplicitVisit(InsertSpecification node) { Target("INSERT", node.Target); base.ExplicitVisit(node); }
        public override void ExplicitVisit(UpdateSpecification node) { Target("UPDATE", node.Target, node.FromClause); base.ExplicitVisit(node); }
        public override void ExplicitVisit(DeleteSpecification node) { Target("DELETE", node.Target, node.FromClause); base.ExplicitVisit(node); }
        public override void ExplicitVisit(MergeSpecification node) { Target("MERGE", node.Target); base.ExplicitVisit(node); }
        public override void ExplicitVisit(TruncateTableStatement node)
        {
            if (node.TableName != null)
            {
                var result = new LiveTableOperation("TRUNCATE", string.Join(".", node.TableName.Identifiers.Select(i => SqlTools.Quote(i.Value))));
                if (!Results.Contains(result)) Results.Add(result);
            }
            base.ExplicitVisit(node);
        }
        public override void ExplicitVisit(NamedTableReference node)
        {
            if (!targets.Contains(node) && (node.SchemaObject.Identifiers.Count > 1 || !ctes.Contains(node.SchemaObject.BaseIdentifier.Value)))
                Add("READ", node);
            base.ExplicitVisit(node);
        }

        private sealed class AliasFinder(string alias) : TSqlFragmentVisitor
        {
            internal NamedTableReference? Match { get; private set; }
            public override void ExplicitVisit(NamedTableReference node)
            {
                if (string.Equals(node.Alias?.Value, alias, StringComparison.OrdinalIgnoreCase)) Match = node;
                base.ExplicitVisit(node);
            }
        }
    }
}
