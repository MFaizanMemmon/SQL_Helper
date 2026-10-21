using System.Data;
namespace SQL_Helper;
public partial class frmDummyData : Form
{
    private readonly DataGridView settings = new() { Dock = DockStyle.Fill };
    private readonly CancellationTokenSource lifetime = new();
    private CancellationTokenSource? operation;
    private SampleSchema? schema;
    private readonly Button cancel = new() { Text = "Cancel insertion", Dock = DockStyle.Bottom, Enabled = false, Height = 36 };
    public frmDummyData()
    {
        InitializeComponent(); AppUi.Apply(this); pnlSchema.Controls.Add(settings); AppUi.StyleTree(settings); settings.ReadOnly = false; settings.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
        settings.Columns.Add(new DataGridViewTextBoxColumn { Name = "Column", HeaderText = "Column", ReadOnly = true }); settings.Columns.Add(new DataGridViewTextBoxColumn { Name = "Type", HeaderText = "SQL type", ReadOnly = true });
        settings.Columns.Add(new DataGridViewComboBoxColumn { Name = "Mode", HeaderText = "Generator", FlatStyle = FlatStyle.Flat });
        foreach (string name in new[] { "Minimum", "Maximum", "Format" }) settings.Columns.Add(new DataGridViewTextBoxColumn { Name = name, HeaderText = name == "Format" ? "Constant / custom format / true %" : name });
        settings.CurrentCellDirtyStateChanged += (_, _) => { if (settings.IsCurrentCellDirty) settings.CommitEdit(DataGridViewDataErrorContexts.Commit); };
        settings.DataError += (_, e) => { e.ThrowException = false; };
        cmbTable.SelectedIndexChanged += (_, _) => { schema = null; settings.Rows.Clear(); };
        txtTotalRows.Text = "100"; Controls.Add(cancel); AppUi.StyleTree(cancel); cancel.Click += (_, _) => operation?.Cancel();
        FormClosed += (_, _) => { operation?.Cancel(); lifetime.Cancel(); };
    }
    private async void frmDummyData_Load(object sender, EventArgs e)
    {
        try { var databases = await SqlTools.DatabasesAsync(DbConnectionHelper.ConnectionString, lifetime.Token); foreach (DataRow row in databases.Rows) cmbDatabaes.Items.Add(row[0]); if (DbConnectionHelper.SelectedDatabase != null) cmbDatabaes.SelectedItem = DbConnectionHelper.SelectedDatabase; }
        catch (OperationCanceledException) { }
        catch (Exception ex) { SqlTools.Error(this, ex); }
    }
    private async void cmbDatabaes_SelectedIndexChanged(object sender, EventArgs e)
    {
        schema = null; settings.Rows.Clear(); cmbTable.Items.Clear();
        if (cmbDatabaes.SelectedItem is not string database) return;
        cmbDatabaes.Enabled = false;
        try
        {
            var tables = await SqlTools.QueryAsync(SqlTools.DatabaseConnection(DbConnectionHelper.ConnectionString, database), "SELECT QUOTENAME(SCHEMA_NAME(schema_id))+'.'+QUOTENAME(name) FROM sys.tables ORDER BY schema_id,name", lifetime.Token);
            foreach (DataRow row in tables.Rows) cmbTable.Items.Add(row[0]);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) { SqlTools.Error(this, ex); }
        finally { if (!IsDisposed) cmbDatabaes.Enabled = true; }
    }
    private async void btnViewSchema_Click(object sender, EventArgs e)
    {
        if (cmbDatabaes.SelectedItem is not string database || cmbTable.SelectedItem is not string table) { MessageBox.Show(this, "Select a database and table."); return; }
        btnViewSchema.Enabled = false;
        try
        {
            schema = await SampleDataService.ReadAsync(SqlTools.DatabaseConnection(DbConnectionHelper.ConnectionString, database), table, lifetime.Token); settings.Rows.Clear();
            foreach (var column in schema.Columns.Where(SampleDataService.Writable))
            {
                int index = settings.Rows.Add(); var row = settings.Rows[index]; row.Tag = column;
                row.Cells["Column"].Value = column.Name; row.Cells["Type"].Value = column.Type;
                var mode = (DataGridViewComboBoxCell)row.Cells["Mode"];
                mode.Items.AddRange("Automatic", "Constant");
                if (column.Type is "varchar" or "nvarchar" or "char" or "nchar" or "text" or "ntext") mode.Items.AddRange("Custom format", "Random Name", "Random Email", "Random Phone", "Random Address", "Random CNIC");
                if (column.Nullable) mode.Items.Add("NULL"); if (column.HasDefault || column.Nullable) mode.Items.Add("Database default");
                bool foreign = schema.ForeignKeys.Any(fk => fk.Columns.Contains(column.Name)); if (foreign) mode.Items.Add("Referenced row");
                mode.Value = foreign ? "Referenced row" : column.HasDefault ? "Database default" : "Automatic";
            }
            Text = $"Generate sample data • {table} • {settings.Rows.Count} writable columns";
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) { schema = null; SqlTools.Error(this, ex); }
        finally { if (!IsDisposed) btnViewSchema.Enabled = true; }
    }
    private async void btnFeedData_Click(object sender, EventArgs e)
    {
        if (schema == null) { MessageBox.Show(this, "Load the selected table schema first."); return; }
        if (!int.TryParse(txtTotalRows.Text, out int count) || count is <= 0 or > 100000) { MessageBox.Show(this, "Enter 1–100,000 rows per insertion."); return; }
        settings.EndEdit();
        var options = settings.Rows.Cast<DataGridViewRow>().Select(row => new SampleOption((SampleColumn)row.Tag!, Convert.ToString(row.Cells["Mode"].Value) ?? "Automatic", Convert.ToString(row.Cells["Minimum"].Value) ?? "", Convert.ToString(row.Cells["Maximum"].Value) ?? "", Convert.ToString(row.Cells["Format"].Value) ?? "")).ToList();
        operation?.Dispose(); operation = CancellationTokenSource.CreateLinkedTokenSource(lifetime.Token);
        btnFeedData.Enabled = btnViewSchema.Enabled = cmbDatabaes.Enabled = cmbTable.Enabled = settings.Enabled = false; cancel.Enabled = true;
        var progress = new Progress<int>(done => { if (!IsDisposed) Text = $"Inserting sample data • {done:N0} / {count:N0}"; });
        try
        {
            await SampleDataService.InsertAsync(SqlTools.DatabaseConnection(DbConnectionHelper.ConnectionString, cmbDatabaes.Text), schema, options, count, progress, operation.Token);
            if (!IsDisposed) MessageBox.Show(this, $"Inserted {count:N0} rows into {schema.Table}.");
        }
        catch (OperationCanceledException) { if (!IsDisposed) MessageBox.Show(this, "Insertion cancelled. The transaction was rolled back."); }
        catch (Exception ex) { if (!IsDisposed) SqlTools.Error(this, new InvalidOperationException("Insertion failed and was rolled back. " + ex.Message, ex)); }
        finally { if (!IsDisposed) { btnFeedData.Enabled = btnViewSchema.Enabled = cmbDatabaes.Enabled = cmbTable.Enabled = settings.Enabled = true; cancel.Enabled = false; Text = "Generate sample data | SQL Helper"; } }
    }
    public int GetRandomNumber(int start, int end) { if (start > end) throw new ArgumentException("Start must not exceed end."); return checked((int)Random.Shared.NextInt64(start, (long)end + 1)); }
    public string GenerateRandomByFormat(string format) => SampleDataService.Format(format);
}
