using Microsoft.Data.SqlClient;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Data;
using System.Text;
namespace SQL_Helper;

/// <summary>Opt-in regression checks. SQL checks create and remove only uniquely named verification databases.</summary>
internal static class ProjectVerification
{
    internal static async Task<int> RunAsync(string reportPath, bool withSql)
    {
        using var log = new StreamWriter(reportPath, false, Encoding.UTF8) { AutoFlush = true };
        int passed = 0;
        void Check(bool condition, string name) { if (!condition) throw new InvalidOperationException(name); log.WriteLine("PASS " + name); passed++; }
        try
        {
            Check(SqlIdentifier.Quote("[odd.schema].[a]]b]") == "[odd.schema].[a]]b]", "SQL identifier escaping");
            var analysis = SqlParserHelper.Analyze("CREATE PROC [sales].[p] @Id int AS BEGIN DECLARE @Text nvarchar(50)='https://example.test'; WITH cte AS (SELECT * FROM [sales].[Parent]) SELECT * FROM cte c JOIN [sales].[Child] a ON c.Id=a.ParentId WHERE a.Id=@Id; END");
            Check(analysis.Errors.Count == 0 && analysis.Tables.Count == 2 && analysis.Variables.Contains("@Id") && analysis.Variables.Contains("@Text") && analysis.UsesHttp, "AST variables, CTE exclusion, schema references, and HTTP detection");
            Check(SqlParserHelper.Normalize("CREATE PROC dbo.p AS SELECT 'ABC'") != SqlParserHelper.Normalize("ALTER PROC dbo.p AS SELECT 'abc'"), "SQL comparison preserves string literal case");
            Check(SqlParserHelper.Normalize("CREATE PROC dbo.p AS SELECT 1") == SqlParserHelper.Normalize("ALTER PROC dbo.p AS -- comment\n SELECT 1"), "SQL comparison ignores comments and whitespace");
            Check(SqlParserHelper.ToCreateOrAlter("-- CREATE in comment\nCREATE PROC dbo.p AS SELECT 'CREATE PROC';").Count(c => c == '\'') == 2, "Module conversion preserves SQL literals");
            var profile = new ConnectionProfile("localhost", "SQL SERVER AUTHENTICATION", "a;b", "  x|y;z  ", true);
            var decoded = ConnectionStore.Decode(ConnectionStore.Encode(profile));
            Check(decoded == profile && new SqlConnectionStringBuilder(decoded.ConnectionString).Password == "  x|y;z  ", "Encrypted credential and delimiter round trip");
            Check(ConnectionStore.Decode("local|Windows Authentication||").Windows, "Legacy authentication compatibility");
            Check(ConnectionStore.Decode("local|SQL SERVER AUTHENTICATION|name|a|b").Password == "a|b", "Legacy passwords containing delimiters");
            Check((int)ParameterValues.Parse(new SqlParameter("@i", SqlDbType.Int), "-23", false) == -23, "Typed integer input");
            Check((string)ParameterValues.Parse(new SqlParameter("@s", SqlDbType.NVarChar), "", false) == "" && ParameterValues.Parse(new SqlParameter("@s", SqlDbType.NVarChar), "", true) == DBNull.Value, "Empty string and SQL NULL remain distinct");
            Check((bool)ParameterValues.Parse(new SqlParameter("@b", SqlDbType.Bit), "1", false), "Typed boolean input");
            Check(((byte[])ParameterValues.Parse(new SqlParameter("@b", SqlDbType.VarBinary), "0x01FF", false)).SequenceEqual(new byte[] { 1, 255 }), "Typed binary input");
            Check(SchemaService.FormatType("nvarchar", "sys", false, -1, 0, 0) == "nvarchar(MAX)" && SchemaService.FormatType("datetime2", "sys", false, 8, 27, 3) == "datetime2(3)", "SQL type lengths and temporal precision");
            var data = new DataTable(); data.Columns.Add("A]"); data.Rows.Add("a[b]*%'");
            Check(SqlTools.Filter(data, "[b]*%'").Rows.Count == 1 && SqlTools.Filter(data, "none").Rows.Count == 0, "Literal search with brackets, wildcards, and apostrophes");
            data.Rows[0][0] = "a,\"b\"\r\nc";
            string csv = Path.Combine(Path.GetDirectoryName(reportPath)!, "verification-export.csv"); DataExport.Write(data, csv, "csv");
            Check(File.ReadAllText(csv).Contains("\"a,\"\"b\"\"\r\nc\""), "CSV commas, quotes, and newlines"); File.Delete(csv);
            string xlsx = Path.ChangeExtension(csv, ".xlsx"); DataExport.Write(data, xlsx, "xlsx");
            using (var book = new ClosedXML.Excel.XLWorkbook(xlsx)) Check(book.Worksheet(1).Cell(2, 1).GetString().Replace("\r\n", "\n") == "a,\"b\"\nc", "Excel export round trip"); File.Delete(xlsx);
            Check(System.Text.RegularExpressions.Regex.IsMatch(SampleDataService.Format("AAA-###-aaa"), "^[A-Z]{3}-[0-9]{3}-[a-z]{3}$"), "Custom sample-data format");
            var numeric = new SampleColumn("n", "int", false, false, false, false, false, 4, 10, 0, 0);
            Check((int)SampleDataService.Generate(new(numeric, "Automatic", "-5", "-5", ""), 0) == -5, "Negative and equal numeric range");
            var noObjects = new SchemaSnapshot("Empty", new Dictionary<string, SchemaObject>());
            var blocked = new SchemaObject("Table", "dbo", "t", "", "", BlockedReason: "Unsupported test shape"); var blockedSnapshot = new SchemaSnapshot("Source", new Dictionary<string, SchemaObject> { [blocked.Key] = blocked });
            string blockedSql = MigrationScript.Build(blockedSnapshot, noObjects, SchemaService.Compare(blockedSnapshot, noObjects), false);
            Check(blockedSql.Contains("THROW 51000") && !blockedSql.Contains("BEGIN TRANSACTION"), "Unsupported migrations fail closed");
            Check(SchemaService.Compare(blockedSnapshot, blockedSnapshot).Count == 1, "Unreadable or unsupported definitions always require review");
            if (withSql) passed += await VerifySqlAsync(log);
            log.WriteLine($"SUCCESS: {passed} checks passed."); return 0;
        }
        catch (Exception ex) { log.WriteLine("FAIL " + ex); return 1; }
    }
    private sealed class InlineProgress<T>(Action<T> callback) : IProgress<T> { public void Report(T value) => callback(value); }
    private static async Task<int> VerifySqlAsync(StreamWriter log)
    {
        int passed = 0; void Check(bool condition, string name) { if (!condition) throw new InvalidOperationException(name); log.WriteLine("PASS " + name); passed++; }
        string suffix = Guid.NewGuid().ToString("N"); string devName = "SqlHelper_Verify_" + suffix + "_Dev", prodName = "SqlHelper_Verify_" + suffix + "_Prod";
        const string server = "Server=localhost;Integrated Security=True;TrustServerCertificate=True;Encrypt=False;Initial Catalog=master;Connect Timeout=10";
        string dev = SqlTools.DatabaseConnection(server, devName), prod = SqlTools.DatabaseConnection(server, prodName);
        async Task Execute(string cs, string sql)
        {
            using var con = new SqlConnection(cs); await con.OpenAsync();
            var fragment = new TSql170Parser(true).Parse(new StringReader(sql), out var errors);
            if (errors.Count > 0) throw new InvalidOperationException("Generated SQL parse errors: " + string.Join("; ", errors.Select(e => $"{e.Line}: {e.Message}")));
            foreach (var batch in ((TSqlScript)fragment).Batches)
            {
                using var command = new SqlCommand(sql.Substring(batch.StartOffset, batch.FragmentLength), con) { CommandTimeout = 60 }; await command.ExecuteNonQueryAsync();
            }
        }
        try
        {
            await Execute(server, "CREATE DATABASE " + SqlTools.Quote(devName) + ";\nGO\nCREATE DATABASE " + SqlTools.Quote(prodName) + ";");
            const string common = "CREATE SCHEMA sales;\nGO\nCREATE TABLE sales.Parent(Id int IDENTITY(10,2) CONSTRAINT PK_Parent PRIMARY KEY,Name nvarchar(80) NOT NULL CONSTRAINT UQ_Parent_Name UNIQUE); INSERT INTO sales.Parent(Name) VALUES(N'One'),(N'Two');";
            await Execute(dev, common); await Execute(prod, common);
            await Execute(dev, @"
CREATE TYPE dbo.Code FROM nvarchar(20) NOT NULL;
GO
CREATE TYPE dbo.Items AS TABLE(Id int NOT NULL PRIMARY KEY, Amount decimal(10,2) NULL);
GO
CREATE TABLE sales.Child(Id int CONSTRAINT PK_Child PRIMARY KEY,ParentId int NOT NULL,Amount decimal(12,3) NOT NULL,Flag bit NOT NULL CONSTRAINT DF_Child_Flag DEFAULT(1),Name nvarchar(80) NULL,Created datetime2(3) NULL,Token uniqueidentifier NOT NULL,CONSTRAINT FK_Child_Parent FOREIGN KEY(ParentId) REFERENCES sales.Parent(Id),CONSTRAINT CK_Child_Amount CHECK(Amount>0));
CREATE INDEX IX_Child_Amount ON sales.Child(Amount DESC) INCLUDE(Name) WHERE Flag=1;
CREATE TABLE sales.NewTable(Id int IDENTITY(5,3) CONSTRAINT PK_New PRIMARY KEY,Code dbo.Code NOT NULL,Value varbinary(MAX) NULL,DoubleId AS Id*2 PERSISTED);
CREATE TABLE dbo.ParentPair(A int NOT NULL,B int NOT NULL,CONSTRAINT PK_ParentPair PRIMARY KEY(A,B)); INSERT dbo.ParentPair VALUES(1,5),(2,9);
CREATE TABLE dbo.ChildPair(Id int CONSTRAINT PK_ChildPair PRIMARY KEY,A int NOT NULL,B int NOT NULL,CONSTRAINT FK_Pair FOREIGN KEY(A,B) REFERENCES dbo.ParentPair(A,B));
GO
CREATE FUNCTION dbo.fnLabel() RETURNS nvarchar(30) AS BEGIN RETURN N'AbC'; END;
GO
CREATE VIEW sales.vChild AS SELECT Id,Name,dbo.fnLabel() AS Label FROM sales.Child;
GO
CREATE PROCEDURE sales.pResults @Amount decimal(10,2),@Items dbo.Items READONLY,@Total decimal(12,2) OUTPUT AS BEGIN SET NOCOUNT ON; SELECT @Total=COALESCE(SUM(Amount),0)+@Amount FROM @Items; SELECT Id,Amount FROM @Items; SELECT @Total AS Total; RETURN 7; END;
GO
CREATE TRIGGER sales.trChild ON sales.Child AFTER INSERT AS BEGIN SET NOCOUNT ON; DECLARE @Url nvarchar(100)=N'https://example.test'; END;
GO
CREATE PROCEDURE dbo.pDefaults @Value int=42 AS SELECT @Value AS Value;");
            await Execute(prod, @"
CREATE TABLE sales.Child(Id int CONSTRAINT PK_Child PRIMARY KEY,ParentId int NOT NULL,Amount decimal(10,2) NOT NULL,Flag bit NOT NULL CONSTRAINT DF_Child_Flag DEFAULT(1),Created datetime2(3) NULL,Token uniqueidentifier NOT NULL,CONSTRAINT FK_Child_Parent FOREIGN KEY(ParentId) REFERENCES sales.Parent(Id),CONSTRAINT CK_Child_Amount CHECK(Amount>0));
CREATE TABLE dbo.Extra(Id int NULL);
GO
CREATE FUNCTION dbo.fnLabel() RETURNS nvarchar(30) AS BEGIN RETURN N'abc'; END;");
            var source = await SchemaService.ReadAsync(dev, default); var target = await SchemaService.ReadAsync(prod, default); var differences = SchemaService.Compare(source, target);
            Check(differences.Any(d => d.Object.Kind == "Column" && d.Object.Name == "Amount") && differences.Any(d => d.Object.Kind == "Function") && differences.Any(d => d.Object.Kind == "View") && differences.Any(d => d.Object.Kind == "Trigger"), "Live schema comparison detects columns, literal changes, views, and triggers");
            string script = MigrationScript.Build(source, target, differences, false); await File.WriteAllTextAsync(Path.Combine(Path.GetDirectoryName(log.BaseStream is FileStream fs ? fs.Name : "")!, "verification-migration.sql"), script);
            Check(!script.Contains("THROW 51000"), "Standard migration has no unsupported blockers");
            await Execute(prod, script); var migrated = await SchemaService.ReadAsync(prod, default); var remaining = SchemaService.Compare(source, migrated);
            Check(remaining.All(d => d.Source == null), "Live migration converges to development schema and retains production-only objects");
            var identity = await SqlTools.QueryAsync(prod, "SELECT seed_value,increment_value FROM sys.identity_columns WHERE object_id=OBJECT_ID('sales.NewTable')");
            Check(Convert.ToInt32(identity.Rows[0][0]) == 5 && Convert.ToInt32(identity.Rows[0][1]) == 3, "Migration preserves identity seed and increment");
            var parameters = await ProcedureService.ParametersAsync(dev, "sales.pResults", default); var items = parameters.Single(p => p.SqlDbType == SqlDbType.Structured);
            items.Value = ParameterValues.Parse(items, "[{\"Id\":1,\"Amount\":3.25},{\"Id\":2,\"Amount\":4.50}]", false);
            parameters.Single(p => p.ParameterName == "@Amount").Value = 1.25m;
            var execution = await ProcedureService.ExecuteAsync(dev, "sales.pResults", parameters, default);
            Check(execution.Tables.Count == 2 && execution.Tables[0].Rows.Count == 2 && Convert.ToDecimal(execution.Tables[1].Rows[0][0]) == 9m, "TVP, decimal metadata, and multiple procedure result sets");
            Check(execution.Outputs.Rows.Cast<DataRow>().Any(r => (string)r[0] == "@Total" && (string)r[1] == "9.00") && execution.Outputs.Rows.Cast<DataRow>().Any(r => (string)r[0] == "@RETURN_VALUE" && (string)r[1] == "7"), "Procedure output parameters and return value");
            Check((int)(await ProcedureService.ExecuteAsync(dev, "dbo.pDefaults", Array.Empty<SqlParameter>(), default)).Tables[0].Rows[0][0] == 42, "Omitted parameters use the stored procedure default");
            var schema = await SampleDataService.ReadAsync(dev, "sales.Child", default);
            var options = schema.Columns.Where(SampleDataService.Writable).Select(c => new SampleOption(c, schema.ForeignKeys.Any(fk => fk.Columns.Contains(c.Name)) ? "Referenced row" : c.HasDefault ? "Database default" : "Automatic", "", "", "")).ToList();
            await SampleDataService.InsertAsync(dev, schema, options, 20, null, default);
            var count = await SqlTools.QueryAsync(dev, "SELECT COUNT(*) FROM sales.Child WHERE Amount>0 AND ParentId IN(SELECT Id FROM sales.Parent)"); Check((int)count.Rows[0][0] == 20, "Atomic sample insertion with primary keys, foreign keys, typed values, and defaults");
            bool failed = false;
            try { await SampleDataService.InsertAsync(dev, schema, options.Select(o => o.Column.Name == "Amount" ? o with { Mode = "Constant", Format = "-1" } : o).ToList(), 5, null, default); } catch (SqlException) { failed = true; }
            count = await SqlTools.QueryAsync(dev, "SELECT COUNT(*) FROM sales.Child"); Check(failed && (int)count.Rows[0][0] == 20, "Failed sample generation rolls back all rows");
            using var cancelled = new CancellationTokenSource(); bool wasCancelled = false;
            try { await SampleDataService.InsertAsync(dev, schema, options, 60, new InlineProgress<int>(_ => cancelled.Cancel()), cancelled.Token); } catch (OperationCanceledException) { wasCancelled = true; }
            Check(wasCancelled && (int)(await SqlTools.QueryAsync(dev, "SELECT COUNT(*) FROM sales.Child")).Rows[0][0] == 20, "Mid-insertion cancellation rolls back rows already written");
            var pair = await SampleDataService.ReadAsync(dev, "dbo.ChildPair", default); var pairOptions = pair.Columns.Where(SampleDataService.Writable).Select(c => new SampleOption(c, c.Name == "Id" ? "Automatic" : "Referenced row", "", "", "")).ToList();
            await SampleDataService.InsertAsync(dev, pair, pairOptions, 10, null, default); Check((int)(await SqlTools.QueryAsync(dev, "SELECT COUNT(*) FROM dbo.ChildPair" )).Rows[0][0] == 10, "Composite foreign keys use a consistent referenced tuple");
            string destructive = MigrationScript.Build(source, migrated, remaining, true); await Execute(prod, destructive);
            Check(SchemaService.Compare(source, await SchemaService.ReadAsync(prod, default)).Count == 0, "Explicit destructive migration removes production-only objects");
        }
        finally
        {
            foreach (string name in new[] { devName, prodName })
            {
                if (!name.StartsWith("SqlHelper_Verify_" + suffix + "_", StringComparison.Ordinal)) throw new InvalidOperationException("Verification cleanup name mismatch.");
                await Execute(server, "IF DB_ID(" + SqlTools.Literal(name) + ") IS NOT NULL BEGIN ALTER DATABASE " + SqlTools.Quote(name) + " SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE " + SqlTools.Quote(name) + "; END;");
            }
            log.WriteLine("CLEANUP verification databases removed.");
        }
        return passed;
    }
}
