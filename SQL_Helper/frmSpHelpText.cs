using Microsoft.Data.SqlClient;
using System.Data;
namespace SQL_Helper;
public partial class frmSpHelpText : Form
{
    public string DbName { get; set; } = "";
    public string SpName { get; set; } = "";
    private readonly CancellationTokenSource lifetime = new();
    public frmSpHelpText() { InitializeComponent(); AppUi.Apply(this); button1.Text = "Run procedure…"; button2.Text = "Referenced tables"; button1.Enabled = false; FormClosed += (_, _) => lifetime.Cancel(); }
    private async void frmSpHelpText_Load(object sender, EventArgs e)
    {
        try
        {
            var data = await SqlTools.QueryAsync(SqlTools.DatabaseConnection(DbConnectionHelper.ConnectionString, DbName), "SELECT definition FROM sys.sql_modules WHERE object_id=OBJECT_ID(@name)", lifetime.Token, new SqlParameter("@name", SpName));
            if (data.Rows.Count == 0 || data.Rows[0].IsNull(0)) throw new InvalidOperationException("The definition is unavailable. Check the object name and VIEW DEFINITION permission.");
            richTextBox1.Text = (string)data.Rows[0][0]; Text = $"{DbName} • {SpName}"; button1.Enabled = true;
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) { SqlTools.Error(this, ex); }
    }
    private async void button1_Click(object sender, EventArgs e)
    {
        button1.Enabled = false;
        try
        {
            var parameters = await ProcedureService.ParametersAsync(SqlTools.DatabaseConnection(DbConnectionHelper.ConnectionString, DbName), SpName, lifetime.Token);
            if (parameters.Any(p => p.Direction != ParameterDirection.Output))
            {
                using var form = new frmParameters(parameters); if (form.ShowDialog(this) != DialogResult.OK) return;
                parameters.RemoveAll(parameter => !form.TypedValues.ContainsKey(parameter.ParameterName));
                foreach (var parameter in parameters) parameter.Value = form.TypedValues[parameter.ParameterName];
            }
            using var data = new ViewData { DbName = DbName, SpName = SpName, parameters = parameters }; data.ShowDialog(this);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) { SqlTools.Error(this, ex); }
        finally { if (!IsDisposed) button1.Enabled = true; }
    }
    private void button2_Click(object sender, EventArgs e)
    {
        var result = SqlParserHelper.Analyze(richTextBox1.Text);
        if (result.Errors.Count > 0) { MessageBox.Show(this, string.Join("\n", result.Errors)); return; }
        MessageBox.Show(this, result.Tables.Count == 0 ? "No static table references found." : string.Join("\n", result.Tables.Select(t => t.TableName + (t.Alias.Length == 0 ? "" : " AS " + t.Alias))), "Referenced tables");
    }
}
