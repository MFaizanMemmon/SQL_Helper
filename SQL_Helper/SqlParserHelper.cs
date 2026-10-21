using Microsoft.SqlServer.TransactSql.ScriptDom;
namespace SQL_Helper;

public sealed class TableInfo
{
    public string TableName { get; set; } = "";
    public string Alias { get; set; } = "";
}
public sealed record SqlAnalysis(IReadOnlyList<TableInfo> Tables, IReadOnlyList<string> Variables, bool UsesHttp, IReadOnlyList<string> Errors);
public static class SqlParserHelper
{
    public static SqlAnalysis Analyze(string sql)
    {
        var parser = new TSql170Parser(true);
        var fragment = parser.Parse(new StringReader(sql), out var errors);
        var visitor = new AnalysisVisitor(); fragment.Accept(visitor);
        return new SqlAnalysis(visitor.Tables.Where(t => !visitor.Ctes.Contains(t.TableName)).DistinctBy(t => (t.TableName, t.Alias)).ToList(), visitor.Variables.OrderBy(v => v).ToList(), visitor.UsesHttp, errors.Select(e => $"Line {e.Line}: {e.Message}").ToList());
    }
    public static List<TableInfo> ExtractTablesAndAliases(string sql) => Analyze(sql).Tables.ToList();
    internal static string Normalize(string sql)
    {
        // Preserve literals and identifiers: changing their case can change behavior.
        return string.Join("|", Tokens(ToCreateOrAlter(sql)).Where(t => !Trivia(t)).Select(t => t.TokenType + ":" + t.Text));
    }
    internal static string ToCreateOrAlter(string sql)
    {
        var significant = Tokens(sql).Where(t => !Trivia(t) && t.TokenType != TSqlTokenType.EndOfFile).ToList();
        int first = significant.FindIndex(t => t.Text.Equals("CREATE", StringComparison.OrdinalIgnoreCase) || t.Text.Equals("ALTER", StringComparison.OrdinalIgnoreCase));
        if (first < 0 || first + 1 >= significant.Count) return sql;
        var token = significant[first];
        if (significant[first + 1].Text.Equals("OR", StringComparison.OrdinalIgnoreCase)) return sql;
        string next = significant[first + 1].Text.ToUpperInvariant();
        if (next is not ("PROC" or "PROCEDURE" or "FUNCTION" or "VIEW" or "TRIGGER")) return sql;
        return sql[..token.Offset] + "CREATE OR ALTER" + sql[(token.Offset + token.Text.Length)..];
    }
    private static IList<TSqlParserToken> Tokens(string sql) => new TSql170Parser(true).GetTokenStream(new StringReader(sql), out _);
    private static bool Trivia(TSqlParserToken token) => token.TokenType is TSqlTokenType.WhiteSpace or TSqlTokenType.SingleLineComment or TSqlTokenType.MultilineComment;
    private sealed class AnalysisVisitor : TSqlFragmentVisitor
    {
        internal List<TableInfo> Tables { get; } = new();
        internal HashSet<string> Variables { get; } = new(StringComparer.OrdinalIgnoreCase);
        internal HashSet<string> Ctes { get; } = new(StringComparer.OrdinalIgnoreCase);
        internal bool UsesHttp;
        public override void Visit(NamedTableReference node) => Tables.Add(new TableInfo { TableName = string.Join(".", node.SchemaObject.Identifiers.Select(i => SqlTools.Quote(i.Value))), Alias = node.Alias?.Value ?? "" });
        public override void Visit(CommonTableExpression node) => Ctes.Add(SqlTools.Quote(node.ExpressionName.Value));
        public override void Visit(VariableReference node) => Variables.Add(node.Name);
        public override void Visit(DeclareVariableElement node) => Variables.Add(node.VariableName.Value);
        public override void Visit(ProcedureParameter node) => Variables.Add(node.VariableName.Value);
        public override void Visit(StringLiteral node) { if (node.Value.Contains("http://", StringComparison.OrdinalIgnoreCase) || node.Value.Contains("https://", StringComparison.OrdinalIgnoreCase)) UsesHttp = true; }
    }
}
