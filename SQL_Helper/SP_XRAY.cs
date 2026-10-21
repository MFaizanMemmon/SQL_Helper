using Microsoft.Data.SqlClient;
using System.Data;
namespace SQL_Helper;
public partial class frm_Sp_Xray : Form
{
    private readonly CancellationTokenSource lifetime = new();
    public frm_Sp_Xray()
    {
        InitializeComponent(); AppUi.Apply(this); button1.Click += Analyze_Click; AcceptButton = button1;
        foreach (var list in new[] { listView1, listView2, listView3 }) { list.View = View.List; list.FullRowSelect = true; }
        checkBox1.AutoCheck = false; FormClosed += (_, _) => lifetime.Cancel();
    }
    private async void Analyze_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(textBox1.Text)) { MessageBox.Show(this, "Enter a stored procedure name, or paste its SQL definition."); return; }
        button1.Enabled = false;
        try
        {
            string sql = textBox1.Text;
            string database = DbConnectionHelper.SelectedDatabase ?? new SqlConnectionStringBuilder(DbConnectionHelper.ConnectionString).InitialCatalog;
            string cs = SqlTools.DatabaseConnection(DbConnectionHelper.ConnectionString, string.IsNullOrWhiteSpace(database) ? "master" : database);
            if (!sql.Contains('\n') && !sql.TrimStart().StartsWith("CREATE", StringComparison.OrdinalIgnoreCase) && !sql.TrimStart().StartsWith("ALTER", StringComparison.OrdinalIgnoreCase))
            {
                var definition = await SqlTools.QueryAsync(cs, "SELECT definition FROM sys.sql_modules WHERE object_id=OBJECT_ID(@name) AND OBJECTPROPERTY(object_id,'IsProcedure')=1", lifetime.Token, new SqlParameter("@name", sql));
                if (definition.Rows.Count == 0 || definition.Rows[0].IsNull(0)) throw new InvalidOperationException("Procedure not found or its definition is unavailable. Use a schema-qualified name in the selected database.");
                sql = (string)definition.Rows[0][0];
            }
            var analysis = SqlParserHelper.Analyze(sql);
            if (analysis.Errors.Count > 0) throw new InvalidOperationException(string.Join(Environment.NewLine, analysis.Errors));
            listView1.Items.Clear(); listView2.Items.Clear(); listView3.Items.Clear();
            foreach (var variable in analysis.Variables) listView3.Items.Add(variable);
            foreach (var table in analysis.Tables.DistinctBy(t => t.TableName)) listView2.Items.Add(table.TableName);
            checkBox1.Checked = analysis.UsesHttp;
            foreach (var table in analysis.Tables.DistinctBy(t => t.TableName))
            {
                var triggers = await SqlTools.QueryAsync(cs, "SELECT QUOTENAME(OBJECT_SCHEMA_NAME(object_id))+'.'+QUOTENAME(name) AS Name FROM sys.triggers WHERE parent_id=OBJECT_ID(@name)", lifetime.Token, new SqlParameter("@name", table.TableName));
                foreach (DataRow trigger in triggers.Rows) listView1.Items.Add(trigger[0].ToString()!);
            }
            Text = $"Procedure X-Ray • {database} • {analysis.Tables.Count} references";
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) { SqlTools.Error(this, ex); }
        finally { if (!IsDisposed) button1.Enabled = true; }
    }
}
