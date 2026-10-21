using System.Data;
namespace SQL_Helper;
public partial class frmTableSearching : Form
{
    private DataTable? originalTable;
    private readonly CancellationTokenSource lifetime = new();
    public frmTableSearching() { InitializeComponent(); AppUi.Apply(this); dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect; dataGridView1.MultiSelect = false; FormClosed += (_, _) => lifetime.Cancel(); }
    private async void frmTableSearching_Load(object sender, EventArgs e) => await LoadAllTablesInAllDatabases();
    public async Task LoadAllTablesInAllDatabases()
    {
        button1.Enabled = false;
        try
        {
            var databases = await SqlTools.DatabasesAsync(DbConnectionHelper.ConnectionString, lifetime.Token);
            var result = new DataTable(); foreach (string name in new[] { "Database", "Schema", "Table" }) result.Columns.Add(name);
            var failures = new List<string>();
            foreach (DataRow row in databases.Rows)
            {
                string database = (string)row[0]; Text = "Finding tables • " + database;
                try
                {
                    var tables = await SqlTools.QueryAsync(SqlTools.DatabaseConnection(DbConnectionHelper.ConnectionString, database), "SELECT SCHEMA_NAME(schema_id),name FROM sys.tables ORDER BY schema_id,name", lifetime.Token);
                    foreach (DataRow table in tables.Rows) result.Rows.Add(database, table[0], table[1]);
                }
                catch (OperationCanceledException) { throw; }
                catch (Exception ex) { failures.Add(database + ": " + ex.Message); }
            }
            if (IsDisposed) return;
            originalTable = result; ApplySearch(); Text = $"Find tables • {result.Rows.Count} tables";
            if (failures.Count > 0) MessageBox.Show(this, "Results are incomplete. These databases could not be read:\n" + string.Join("\n", failures), "Partial results");
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) { SqlTools.Error(this, ex); }
        finally { if (!IsDisposed) button1.Enabled = true; }
    }
    private void ApplySearch() { if (originalTable != null) dataGridView1.DataSource = SqlTools.Filter(originalTable, textBox1.Text.Trim()); }
    private void textBox1_TextChanged(object sender, EventArgs e) => ApplySearch();
    private void button1_Click(object sender, EventArgs e)
    {
        if (dataGridView1.CurrentRow?.DataBoundItem is not DataRowView row) { MessageBox.Show(this, "Select a table first."); return; }
        using var data = new ViewData { DbName = (string)row["Database"], tableName = SqlTools.Qualify((string)row["Schema"], (string)row["Table"]) }; data.ShowDialog(this);
    }
}
