using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQL_Helper
{
    public class TableInfo
    {
        public string TableName { get; set; }
        public string Alias { get; set; }
    }

    public static class SqlParserHelper
    {
        public static List<TableInfo> ExtractTablesAndAliases(string sql)
        {
            var result = new List<TableInfo>();
            TSql150Parser parser = new TSql150Parser(false); // For SQL Server 2019+
            IList<ParseError> errors;
            TSqlFragment fragment;

            using (var reader = new StringReader(sql))
            {
                fragment = parser.Parse(reader, out errors);
            }

            if (errors != null && errors.Any())
            {
                foreach (var err in errors)
                    Console.WriteLine($"Parse error: {err.Message}");
                return result;
            }

            var visitor = new TableVisitor();
            fragment.Accept(visitor);
            result = visitor.Tables;
            return result;
        }
    }
    public class TableVisitor : TSqlFragmentVisitor
    {
        public List<TableInfo> Tables { get; private set; } = new();

        public override void Visit(NamedTableReference node)
        {
            var table = new TableInfo
            {
                TableName = node.SchemaObject.BaseIdentifier.Value,
                Alias = node.Alias?.Value
            };

            Tables.Add(table);
        }
    }
}
