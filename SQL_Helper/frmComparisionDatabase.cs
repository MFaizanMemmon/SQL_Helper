using System.Data;
namespace SQL_Helper;
public partial class frmComparisionDatabase : Form
{
    private readonly DataGridView dataGridView1 = new() { Dock = DockStyle.Fill };
    private SchemaSnapshot? development, production;
    private List<SchemaDifference> differences = new();
    private DataTable? results;
    private readonly CancellationTokenSource lifetime = new();
    public frmComparisionDatabase()
    {
        InitializeComponent(); AppUi.Apply(this); panel1.Controls.Add(dataGridView1); AppUi.StyleTree(dataGridView1);
        btnGenereateScript.Enabled = btnExportExcel.Enabled = false; FormClosed += (_, _) => lifetime.Cancel();
    }
    private async void button1_Click(object sender, EventArgs e)
    {
        using var connections = new frmCompareDBConnection(); if (connections.ShowDialog(this) != DialogResult.OK) return;
        using var databases = new frmSelectCompareDatabase(); if (databases.ShowDialog(this) != DialogResult.OK) return;
        button1.Enabled = btnGenereateScript.Enabled = btnExportExcel.Enabled = false; Text = "Reading database schemas…";
        try
        {
            // The left connection is development; migrations always target the right production database.
            var first = SchemaService.ReadAsync(DbConnectionHelper.TargetConnectionString!, lifetime.Token);
            var second = SchemaService.ReadAsync(DbConnectionHelper.CompareToConnectionString!, lifetime.Token);
            await Task.WhenAll(first, second); development = first.Result; production = second.Result;
            differences = SchemaService.Compare(development, production);
            results = new DataTable(); foreach (string name in new[] { "Type", "Schema", "Name", "Parent", "Status", "Details" }) results.Columns.Add(name);
            foreach (var difference in differences) { var item = difference.Object; results.Rows.Add(item.Kind, item.Schema, item.Name, item.Parent, difference.Status, item.BlockedReason); }
            ApplySearch(); Text = $"{development.Database} → {production.Database} • {differences.Count} differences";
            btnGenereateScript.Enabled = differences.Count > 0; btnExportExcel.Enabled = true;
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) { development = production = null; results = null; dataGridView1.DataSource = null; Text = "Comparison failed"; SqlTools.Error(this, ex); }
        finally { if (!IsDisposed) button1.Enabled = true; }
    }
    private void ApplySearch() { if (results != null) dataGridView1.DataSource = SqlTools.Filter(results, textBox1.Text.Trim()); }
    private void textBox1_TextChanged(object sender, EventArgs e) => ApplySearch();
    private async void btnExportExcel_Click(object sender, EventArgs e) => await DataExport.SaveAsync(this, dataGridView1.DataSource as DataTable, "xlsx", btnExportExcel);
    private void btnGenereateScript_Click(object sender, EventArgs e)
    {
        if (development == null || production == null) return;
        using var form = new frmGenerateScript(development, production, differences); form.ShowDialog(this);
    }
    private void frmComparisionDatabase_Load(object sender, EventArgs e) { }
}
