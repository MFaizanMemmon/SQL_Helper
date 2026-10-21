using Microsoft.Data.SqlClient;
using System.Data;
namespace SQL_Helper;
public partial class ViewData : Form
{
    public string DbName { get; set; } = "";
    public string tableName { get; set; } = "";
    public string SpName { get; set; } = "";
    public List<SqlParameter> parameters = new();
    private readonly CancellationTokenSource lifetime = new();
    private readonly TabControl results = new() { Dock = DockStyle.Fill };
    private readonly Dictionary<TabPage, (DataGridView Grid, DataTable Source)> tables = new();
    public ViewData()
    {
        InitializeComponent(); AppUi.Apply(this);
        var parent = (TableLayoutPanel)dataGridView1.Parent!; var position = parent.GetPositionFromControl(dataGridView1); parent.Controls.Remove(dataGridView1); parent.Controls.Add(results, position.Column, position.Row);
        results.SelectedIndexChanged += (_, _) => ApplySearch(); FormClosed += (_, _) => lifetime.Cancel();
    }
    private async void ViewData_Load(object sender, EventArgs e)
    {
        BtnExportCSV.Enabled = btnExportExcel.Enabled = false; Text = "Loading data…";
        try
        {
            string connection = SqlTools.DatabaseConnection(DbConnectionHelper.ConnectionString, DbName);
            if (!string.IsNullOrWhiteSpace(tableName))
            {
                var table = await SqlTools.QueryAsync(connection, "SELECT TOP (100) * FROM " + SqlIdentifier.Quote(tableName), lifetime.Token); AddTable(table, "First 100 rows");
            }
            else if (!string.IsNullOrWhiteSpace(SpName))
            {
                var data = await ProcedureService.ExecuteAsync(connection, SpName, parameters, lifetime.Token);
                foreach (var table in data.Tables) AddTable(table, table.TableName);
                AddTable(data.Outputs, "Output / return values");
                Text = $"{DbName} • {SpName} • {data.RecordsAffected} rows affected";
            }
            else throw new InvalidOperationException("No table or procedure selected.");
            if (tableName.Length > 0) Text = $"{DbName} • {tableName} • first 100 rows";
            ApplySearch();
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) { if (!IsDisposed) SqlTools.Error(this, ex); }
        finally { if (!IsDisposed) BtnExportCSV.Enabled = btnExportExcel.Enabled = tables.Count > 0; }
    }
    private void AddTable(DataTable table, string title)
    {
        var page = new TabPage($"{title} ({table.Rows.Count})"); var grid = tables.Count == 0 ? dataGridView1 : new DataGridView();
        grid.Dock = DockStyle.Fill; AppUi.StyleTree(grid); page.Controls.Add(grid); tables[page] = (grid, table); results.TabPages.Add(page);
    }
    private DataTable? CurrentData => results.SelectedTab is TabPage page && tables.TryGetValue(page, out var value) ? value.Grid.DataSource as DataTable : null;
    private void ApplySearch()
    {
        if (results.SelectedTab is not TabPage page || !tables.TryGetValue(page, out var value)) return;
        value.Grid.DataSource = SqlTools.Filter(value.Source, textBox1.Text.Trim());
    }
    private void textBox1_TextChanged(object sender, EventArgs e) => ApplySearch();
    private async void BtnExportCSV_Click(object sender, EventArgs e) => await DataExport.SaveAsync(this, CurrentData, "csv", BtnExportCSV);
    private async void btnExportExcel_Click(object sender, EventArgs e) => await DataExport.SaveAsync(this, CurrentData, "xlsx", btnExportExcel);
}
