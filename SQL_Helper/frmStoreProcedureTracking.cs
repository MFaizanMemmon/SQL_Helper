using System.Data;
namespace SQL_Helper;
public partial class frmStoreProcedureTracking : Form
{
    private CancellationTokenSource? pending;
    private readonly CancellationTokenSource lifetime = new();
    private DataTable? procedures;
    public frmStoreProcedureTracking() { InitializeComponent(); AppUi.Apply(this); dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect; dataGridView1.MultiSelect = false; }
    private async void frmStoreProcedureTracking_Load(object sender, EventArgs e)
    {
        try { var databases = await SqlTools.DatabasesAsync(DbConnectionHelper.ConnectionString, lifetime.Token); foreach (DataRow row in databases.Rows) checkedListBoxDatabses.Items.Add(row[0], false); }
        catch (OperationCanceledException) { }
        catch (Exception ex) { SqlTools.Error(this, ex); }
    }
    private async void checkedListBoxDatabses_ItemCheck(object sender, ItemCheckEventArgs e)
    {
        var selected = Enumerable.Range(0, checkedListBoxDatabses.Items.Count).Where(i => i == e.Index ? e.NewValue == CheckState.Checked : checkedListBoxDatabses.GetItemChecked(i)).Select(i => checkedListBoxDatabses.Items[i].ToString()!).ToList();
        pending?.Cancel(); pending = CancellationTokenSource.CreateLinkedTokenSource(lifetime.Token); var token = pending.Token;
        try { await LoadStoredProceduresAsync(selected, token); }
        catch (OperationCanceledException) { }
        catch (Exception ex) { if (!token.IsCancellationRequested) SqlTools.Error(this, ex); }
    }
    public async Task LoadStoredProceduresAsync(List<string> databases, CancellationToken token)
    {
        var table = new DataTable(); table.Columns.Add("DatabaseName"); table.Columns.Add("ProcedureName"); table.Columns.Add("CreateDate", typeof(DateTime)); table.Columns.Add("ModifyDate", typeof(DateTime));
        var failures = new List<string>();
        foreach (string db in databases)
        {
            Text = "Loading procedures • " + db;
            try
            {
                var data = await SqlTools.QueryAsync(SqlTools.DatabaseConnection(DbConnectionHelper.ConnectionString, db), "SELECT QUOTENAME(SCHEMA_NAME(schema_id))+'.'+QUOTENAME(name),create_date,modify_date FROM sys.procedures ORDER BY schema_id,name", token);
                foreach (DataRow row in data.Rows) table.Rows.Add(db, row[0], row[1], row[2]);
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex) { failures.Add(db + ": " + ex.Message); }
        }
        token.ThrowIfCancellationRequested(); if (IsDisposed) return;
        procedures = table; ApplySearch(); Text = $"Track stored procedures • {table.Rows.Count} procedures";
        if (failures.Count > 0) MessageBox.Show(this, "Results are incomplete:\n" + string.Join("\n", failures), "Partial results");
    }
    private void ApplySearch() { if (procedures != null) dataGridView1.DataSource = SqlTools.Filter(procedures, textBox1.Text.Trim()); }
    private void textBox1_TextChanged(object sender, EventArgs e) => ApplySearch();
    private void button1_Click(object sender, EventArgs e)
    {
        if (dataGridView1.CurrentRow?.DataBoundItem is not DataRowView row) { MessageBox.Show(this, "Select a stored procedure first."); return; }
        using var help = new frmSpHelpText { DbName = (string)row["DatabaseName"], SpName = (string)row["ProcedureName"] }; help.ShowDialog(this);
    }
    private void frmStoreProcedureTracking_FormClosed(object sender, FormClosedEventArgs e) { pending?.Cancel(); lifetime.Cancel(); }
    private void checkedListBoxDatabses_SelectedIndexChanged(object sender, EventArgs e) { }
    private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e) { }
}
