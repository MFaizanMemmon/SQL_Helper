using Microsoft.Data.SqlClient;
using System.Data;

namespace SQL_Helper;

public partial class MainDashboard : Form
{
    private readonly ToolStripStatusLabel status = new() { Spring = true, TextAlign = ContentAlignment.MiddleLeft };
    private readonly Label empty = new() { Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, Text = "Connect to SQL Server to explore your databases.", ForeColor = Color.SlateGray };
    private string objectKind = "Tables";
    private CancellationTokenSource? pending;
    private readonly CancellationTokenSource lifetime = new();
    private int databaseLoadVersion;
    private readonly Dictionary<string, Button> tabs = new();

    public MainDashboard()
    {
        InitializeComponent();
        BuildLayout();
    }

    private void BuildLayout()
    {
        SuspendLayout();
        Controls.Clear();
        Text = "SQL Helper | Database Explorer";
        MinimumSize = new Size(380, 480);
        Size = new Size(1250, 800);
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 10);
        BackColor = Color.FromArgb(244, 247, 251);
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4, Padding = new Padding(20) };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        root.Controls.Add(new Label { Text = "SQL Helper", Font = new Font("Segoe UI", 24, FontStyle.Bold), AutoSize = true, ForeColor = Color.FromArgb(24, 42, 70) }, 0, 0);
        var toolbar = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = true, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Margin = new Padding(0, 0, 0, 12) };
        void Action(string title, EventHandler click) { var b = new Button { Text = title, AutoSize = true, Height = 34, FlatStyle = FlatStyle.Flat, BackColor = Color.White, Margin = new Padding(0, 0, 10, 0) }; b.FlatAppearance.BorderColor = Color.LightSteelBlue; b.Click += click; toolbar.Controls.Add(b); }
        Action("Connect server", toolStripButton5_Click);
        Action("Refresh", async (_, _) => await LoadDatabasesAsync());
        Action("Find tables", toolStripButton2_Click);
        Action("Track procedures", toolStripButton1_Click);
        Action("Compare databases", toolStripButton6_Click);
        Action("Generate data", toolStripButton7_Click);
        Action("Procedure X-Ray", toolStripTextBox1_Click);
        root.Controls.Add(toolbar, 0, 1);
        var split = new SplitContainer { Size = new Size(1100, 600), Dock = DockStyle.Fill, FixedPanel = FixedPanel.Panel1, SplitterDistance = 225, Panel1MinSize = 160 };
        split.Panel1MinSize = 70;
        var sidebar = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Padding = new Padding(0, 0, 14, 0) };
        sidebar.RowStyles.Add(new RowStyle(SizeType.Absolute, 36)); sidebar.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        sidebar.Controls.Add(new Label { Text = "DATABASES", AutoSize = true, ForeColor = Color.SlateGray }, 0, 0);
        listBoxDB.Dock = DockStyle.Fill; listBoxDB.BorderStyle = BorderStyle.None; listBoxDB.ItemHeight = 28; listBoxDB.IntegralHeight = false;
        sidebar.Controls.Add(listBoxDB, 0, 1); split.Panel1.Controls.Add(sidebar);
        var content = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
        content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        content.RowStyles.Add(new RowStyle(SizeType.AutoSize)); content.RowStyles.Add(new RowStyle(SizeType.Absolute, 44)); content.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        var nav = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = true, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink };
        foreach (string kind in new[] { "Tables", "Views", "Procedures", "Triggers", "Types" })
        {
            var button = new Button { Text = kind, AutoSize = true, Height = 34, FlatStyle = FlatStyle.Flat, BackColor = Color.White };
            button.Click += async (_, _) => { objectKind = kind; await LoadObjectsAsync(); };
            tabs.Add(kind, button); nav.Controls.Add(button);
        }
        content.Controls.Add(nav, 0, 0);
        textBox1.Dock = DockStyle.Fill; textBox1.PlaceholderText = "Search objects in this database…"; textBox1.Margin = new Padding(0, 3, 0, 10);
        content.Controls.Add(textBox1, 0, 1);
        var gridPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
        dataGridViewDetail.Dock = DockStyle.Fill; dataGridViewDetail.ReadOnly = true; dataGridViewDetail.AllowUserToAddRows = false; dataGridViewDetail.AllowUserToDeleteRows = false;
        dataGridViewDetail.MultiSelect = false; dataGridViewDetail.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dataGridViewDetail.BackgroundColor = Color.White; dataGridViewDetail.BorderStyle = BorderStyle.None; dataGridViewDetail.RowHeadersVisible = false;
        dataGridViewDetail.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dataGridViewDetail.EnableHeadersVisualStyles = false; dataGridViewDetail.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(231, 237, 245);
        dataGridViewDetail.ColumnHeadersHeight = 38; dataGridViewDetail.RowTemplate.Height = 30;
        dataGridViewDetail.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(247, 249, 252);
        dataGridViewDetail.DefaultCellStyle.SelectionBackColor = Color.FromArgb(218, 233, 255); dataGridViewDetail.DefaultCellStyle.SelectionForeColor = Color.Black;
        gridPanel.Controls.Add(dataGridViewDetail); gridPanel.Controls.Add(empty); content.Controls.Add(gridPanel, 0, 2);
        split.Panel2.Controls.Add(content); root.Controls.Add(split, 0, 2);
        var strip = new StatusStrip { Dock = DockStyle.Fill, SizingGrip = false }; strip.Items.Add(status); root.Controls.Add(strip, 0, 3);
        root.Dock = DockStyle.None;
        var viewport = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
        viewport.Controls.Add(root); Controls.Add(viewport);
        void FitLayout()
        {
            int width = Math.Max(320, viewport.ClientSize.Width - SystemInformation.VerticalScrollBarWidth);
            root.Size = new Size(width, Math.Max(viewport.ClientSize.Height, width < 640 ? 780 : 540));
        }
        viewport.SizeChanged += (_, _) => FitLayout();
        Shown += (_, _) => FitLayout();
        FitLayout();
        split.SizeChanged += (_, _) =>
        {
            bool narrow = split.Width < 640;
            var direction = narrow ? Orientation.Horizontal : Orientation.Vertical;
            if (split.Orientation != direction) split.Orientation = direction;
            int length = narrow ? split.Height : split.Width;
            split.Panel1MinSize = narrow ? 70 : 140;
            if (length > split.Panel1MinSize + 60) split.SplitterDistance = Math.Max(split.Panel1MinSize, Math.Min(narrow ? 110 : 225, length / 3));
        };
        Load += (_, _) => AppUi.FitToScreen(this);
        AppUi.StyleTree(this);
        viewOrChangeSchemaToolStripMenuItem.Text = "View definition / columns / run procedure";
        viewOrChangeSchemaToolStripMenuItem.Click += async (_, _) => await ShowDefinitionAsync();
        dataGridViewDetail.CellDoubleClick += async (_, e) => { if (e.RowIndex >= 0) await ShowDefinitionAsync(); };
        FormClosed += (_, _) => { pending?.Cancel(); lifetime.Cancel(); };
        ResumeLayout(true);
    }

    private async void MainDashboard_Load(object? sender, EventArgs e) => await LoadDatabasesAsync();
    private Task<DataTable> QueryAsync(string sql, string database, CancellationToken token) => SqlTools.QueryAsync(SqlTools.DatabaseConnection(DbConnectionHelper.ConnectionString, database), sql, token);
    private async Task LoadDatabasesAsync()
    {
        if (string.IsNullOrWhiteSpace(DbConnectionHelper.ConnectionString)) { status.Text = "Disconnected • Connect a server to get started"; return; }
        pending?.Cancel();
        int version = ++databaseLoadVersion;
        try
        {
            status.Text = "Loading databases…";
            var table = await QueryAsync("SELECT name FROM sys.databases WHERE state = 0 AND HAS_DBACCESS(name) = 1 AND database_id > 4 ORDER BY name", "master", lifetime.Token);
            if (IsDisposed || version != databaseLoadVersion) return;
            listBoxDB.Items.Clear();
            foreach (DataRow row in table.Rows) listBoxDB.Items.Add(row[0].ToString()!);
            if (listBoxDB.Items.Count > 0) listBoxDB.SelectedIndex = 0;
            else { dataGridViewDetail.DataSource = null; empty.Text = "No accessible user databases on this server."; empty.Visible = true; status.Text = "Connected • No user databases"; }
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) { if (!IsDisposed) ShowError(ex); }
    }
    private async void listBoxDB_SelectedIndexChanged(object? sender, EventArgs e) => await LoadObjectsAsync();
    private async Task LoadObjectsAsync()
    {
        if (listBoxDB.SelectedItem is not string database) return;
        pending?.Cancel(); pending?.Dispose(); pending = CancellationTokenSource.CreateLinkedTokenSource(lifetime.Token); var token = pending.Token;
        DbConnectionHelper.SelectedDatabase = database;
        string kind = objectKind;
        foreach (var pair in tabs) { pair.Value.BackColor = pair.Key == kind ? Color.FromArgb(32, 92, 180) : Color.White; pair.Value.ForeColor = pair.Key == kind ? Color.White : Color.FromArgb(24, 42, 70); }
        string query = kind switch
        {
            "Tables" => "SELECT s.name AS SchemaName, t.name AS TableName, (SELECT COUNT(*) FROM sys.columns c WHERE c.object_id=t.object_id) AS Columns, t.create_date AS Created, t.modify_date AS Modified FROM sys.tables t JOIN sys.schemas s ON s.schema_id=t.schema_id ORDER BY s.name,t.name",
            "Views" => "SELECT SCHEMA_NAME(schema_id) AS SchemaName,name AS ViewName,create_date AS Created,modify_date AS Modified FROM sys.views ORDER BY SchemaName,name",
            "Procedures" => "SELECT SCHEMA_NAME(schema_id) AS SchemaName,name AS ProcedureName,create_date AS Created,modify_date AS Modified FROM sys.procedures ORDER BY SchemaName,name",
            "Triggers" => "SELECT OBJECT_SCHEMA_NAME(object_id) AS SchemaName,name AS TriggerName,OBJECT_NAME(parent_id) AS ParentObject,is_disabled AS Disabled,create_date AS Created FROM sys.triggers WHERE parent_class=1 ORDER BY SchemaName,name",
            _ => "SELECT SCHEMA_NAME(schema_id) AS SchemaName,name AS TypeName,is_table_type AS TableType,max_length AS MaxLength,precision AS Precision,scale AS Scale FROM sys.types WHERE is_user_defined=1 ORDER BY SchemaName,name"
        };
        status.Text = $"Loading {kind.ToLowerInvariant()} from {database}…"; empty.Text = "Loading objects…"; empty.Visible = true;
        try
        {
            var table = await QueryAsync(query, database, token);
            if (token.IsCancellationRequested || IsDisposed) return;
            dataGridViewDetail.DataSource = table.DefaultView; ApplyFilter();
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) { if (!token.IsCancellationRequested) { dataGridViewDetail.DataSource = null; empty.Text = "Unable to load objects. Refresh to try again."; ShowError(ex); } }
    }
    private void ApplyFilter()
    {
        if (dataGridViewDetail.DataSource is not DataView view) return;
        string term = textBox1.Text.Trim();
        // Match literal text, including SQL wildcard characters and unusual column names.
        // A DataView expression is escaped character by character so wildcard text stays literal.
        string escaped = string.Concat(term.Select(c => c switch { '\'' => "''", '[' => "[[]", ']' => "[]]", '%' => "[%]", '*' => "[*]", _ => c.ToString() }));
        view.RowFilter = term.Length == 0 ? "" : string.Join(" OR ", view.Table!.Columns.Cast<DataColumn>().Where(c => c.DataType == typeof(string)).Select(c => $"[{c.ColumnName.Replace("\\", "\\\\").Replace("]", "\\]")}] LIKE '%{escaped}%'"));
        empty.Text = term.Length == 0 ? "No objects found in this category." : "No matching objects. Try another search.";
        empty.Visible = view.Count == 0;
        status.Text = $"{listBoxDB.SelectedItem}  •  {objectKind}  •  {view.Count:N0} of {view.Table!.Rows.Count:N0} objects";
    }
    private void textBox1_TextChanged(object? sender, EventArgs e) => ApplyFilter();
    private string? SelectedName() => dataGridViewDetail.CurrentRow?.DataBoundItem is DataRowView row ? Quote(Convert.ToString(row[0]) ?? "dbo") + "." + Quote(Convert.ToString(row[1])!) : null;
    private static string Quote(string name) => "[" + name.Replace("]", "]]") + "]";
    private void viewDataToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        if (objectKind is not ("Tables" or "Views") || SelectedName() is not string name) return;
        using var form = new ViewData { DbName = listBoxDB.Text, tableName = name }; form.ShowDialog(this);
    }
    private async Task ShowDefinitionAsync()
    {
        if (SelectedName() is not string name) return;
        if (objectKind == "Procedures") { using var help = new frmSpHelpText { DbName = listBoxDB.Text, SpName = name }; help.ShowDialog(this); return; }
        try
        {
            string literal = name.Replace("'", "''");
            string sql = objectKind is "Tables" or "Types"
                ? $"SELECT c.name AS ColumnName,TYPE_NAME(c.user_type_id) AS DataType,c.max_length AS MaxLength,c.is_nullable AS Nullable FROM sys.columns c WHERE c.object_id=COALESCE(OBJECT_ID(N'{literal}'),(SELECT type_table_object_id FROM sys.table_types WHERE user_type_id=TYPE_ID(N'{literal}'))) ORDER BY c.column_id"
                : $"SELECT OBJECT_DEFINITION(OBJECT_ID(N'{literal}')) AS Definition";
            var table = await QueryAsync(sql, listBoxDB.Text, lifetime.Token);
            if (objectKind == "Types" && table.Rows.Count == 0)
                table = await QueryAsync($"SELECT SCHEMA_NAME(schema_id) AS SchemaName,name AS TypeName,TYPE_NAME(system_type_id) AS BaseType,max_length AS MaxLength,precision,scale,is_nullable AS Nullable FROM sys.types WHERE user_type_id=TYPE_ID(N'{literal}')", listBoxDB.Text, lifetime.Token);
            if (IsDisposed) return;
            using var dialog = new Form { Text = name, Size = new Size(900, 650), StartPosition = FormStartPosition.CenterParent };
            if (objectKind is "Tables" or "Types") dialog.Controls.Add(new DataGridView { Dock = DockStyle.Fill, DataSource = table, ReadOnly = true, AllowUserToAddRows = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill });
            else dialog.Controls.Add(new RichTextBox { Dock = DockStyle.Fill, ReadOnly = true, Font = new Font("Consolas", 11), Text = table.Rows.Count == 0 || table.Rows[0].IsNull(0) ? "Definition unavailable. The object may be encrypted or require VIEW DEFINITION permission." : table.Rows[0][0].ToString(), WordWrap = false });
            dialog.MinimumSize = new Size(360, 360);
            AppUi.StyleTree(dialog); dialog.Load += (_, _) => AppUi.FitToScreen(dialog);
            dialog.ShowDialog(this);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) { if (!IsDisposed) ShowError(ex); }
    }
    private void ShowError(Exception ex) { status.Text = "Operation failed"; MessageBox.Show(this, ex.Message, "SQL Helper", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    private bool Connected() { if (!string.IsNullOrWhiteSpace(DbConnectionHelper.ConnectionString)) return true; MessageBox.Show(this, "Connect to a SQL Server first.", "Connection required"); return false; }
    private async void toolStripButton5_Click(object? sender, EventArgs e) { using var form = new frmConnectSQL(); if (form.ShowDialog(this) == DialogResult.OK) await LoadDatabasesAsync(); }
    private void toolStripButton1_Click(object? sender, EventArgs e) { if (Connected()) { using var form = new frmStoreProcedureTracking(); form.ShowDialog(this); } }
    private void toolStripButton2_Click(object? sender, EventArgs e) { if (Connected()) { using var form = new frmTableSearching(); form.ShowDialog(this); } }
    private void toolStripButton6_Click(object? sender, EventArgs e) { using var form = new frmComparisionDatabase(); form.ShowDialog(this); }
    private void toolStripButton7_Click(object? sender, EventArgs e) { if (Connected()) { using var form = new frmDummyData(); form.ShowDialog(this); } }
    private void toolStripTextBox1_Click(object? sender, EventArgs e) { if (Connected()) { using var form = new frm_Sp_Xray(); form.ShowDialog(this); } }
    private void dataGridViewDetail_MouseDown(object? sender, MouseEventArgs e) { if (e.Button != MouseButtons.Right) return; var hit = dataGridViewDetail.HitTest(e.X, e.Y); if (hit.RowIndex < 0) return; dataGridViewDetail.CurrentCell = dataGridViewDetail.Rows[hit.RowIndex].Cells[0]; viewDataToolStripMenuItem.Enabled = objectKind is "Tables" or "Views"; contextMenuStrip1.Show(dataGridViewDetail, e.Location); }
    private async void btnTable_Click(object? s, EventArgs e) { objectKind = "Tables"; await LoadObjectsAsync(); }
    private async void btnShowProcedures_Click(object? s, EventArgs e) { objectKind = "Procedures"; await LoadObjectsAsync(); }
    private async void btnShowView_Click(object? s, EventArgs e) { objectKind = "Views"; await LoadObjectsAsync(); }
    private async void btnShowTriggers_Click(object? s, EventArgs e) { objectKind = "Triggers"; await LoadObjectsAsync(); }
    private async void btnShowTypes_Click(object? s, EventArgs e) { objectKind = "Types"; await LoadObjectsAsync(); }
    private void dataGridViewTableDetail_CellClick(object? s, DataGridViewCellEventArgs e) { }
    private void dataGridViewTableDetail_CellPainting(object? s, DataGridViewCellPaintingEventArgs e) { }
    private void panel1_Paint(object? s, PaintEventArgs e) { }
    private void panel2_Paint(object? s, PaintEventArgs e) { }
    private void panel4_Paint(object? s, PaintEventArgs e) { }
    private void panel2_Click(object? s, EventArgs e) { }
    private void panel3_Click(object? s, EventArgs e) { }
    private void panel4_Click(object? s, EventArgs e) { }
}
