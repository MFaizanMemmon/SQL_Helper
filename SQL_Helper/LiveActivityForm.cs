namespace SQL_Helper;

internal sealed class LiveActivityForm : Form
{
    private readonly string connection, database;
    private readonly DataGridView grid = new() { Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false, MultiSelect = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, RowHeadersVisible = false };
    private readonly RichTextBox detail = new() { Dock = DockStyle.Fill, ReadOnly = true, WordWrap = false, Font = new Font("Consolas", 10) };
    private readonly TreeView flow = new() { Dock = DockStyle.Fill };
    private readonly Label status = new() { AutoSize = true, MaximumSize = new Size(850, 0) };
    private readonly Button start = new() { Text = "Start", AutoSize = true }, stop = new() { Text = "Stop", AutoSize = true, Enabled = false };
    private readonly Button pause = new() { Text = "Pause display", AutoSize = true, Enabled = false }, clear = new() { Text = "Clear", AutoSize = true };
    private readonly TextBox search = new() { Width = 220, PlaceholderText = "Filter SP, table or SQL" };
    private readonly System.Windows.Forms.Timer timer = new() { Interval = 1000 };
    private readonly List<Row> history = new();
    private readonly Dictionary<int, List<(int Id, string Name)>> stacks = new();
    private LiveActivitySession? session;
    private bool busy, paused, closing, allowClose;
    private string warning = "";
    private sealed record Row(LiveSqlEvent Event, string Module, IReadOnlyList<LiveTableOperation> Operations);

    internal LiveActivityForm(string connection, string database)
    {
        this.connection = connection; this.database = database;
        Text = "Live Activity Monitor | " + database;
        Font = new Font("Segoe UI", 10); Size = new Size(1150, 760); MinimumSize = new Size(650, 480);
        StartPosition = FormStartPosition.CenterParent; BackColor = AppUi.Canvas;
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(14), ColumnCount = 1, RowCount = 4 };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize)); root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.Controls.Add(new Label { Text = "Live Activity Monitor — " + database, Font = new Font("Segoe UI", 18, FontStyle.Bold), AutoSize = true }, 0, 0);
        var bar = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, WrapContents = true };
        bar.Controls.AddRange(new Control[] { start, stop, pause, clear, search }); root.Controls.Add(bar, 0, 1);
        var split = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, Size = new Size(1000, 600), SplitterDistance = 330 };
        split.Panel1.Controls.Add(grid);
        var tabs = new TabControl { Dock = DockStyle.Fill };
        var graphTab = new TabPage("Execution flow"); graphTab.Controls.Add(flow); tabs.TabPages.Add(graphTab);
        var sqlTab = new TabPage("SQL and event details"); sqlTab.Controls.Add(detail); tabs.TabPages.Add(sqlTab);
        split.Panel2.Controls.Add(tabs); root.Controls.Add(split, 0, 2);
        status.Text = "Stopped. SQL text can contain sensitive values. Capture stays in memory.";
        root.Controls.Add(status, 0, 3); Controls.Add(root);
        foreach (string column in new[] { "Time (local)", "SP / Module", "Event", "Session", "Operations / objects", "Duration (ms)", "Rows" }) grid.Columns.Add(column, column);
        grid.Columns[4].FillWeight = 180; grid.Columns[2].FillWeight = 120;
        AppUi.StyleTree(this); Load += (_, _) => AppUi.FitToScreen(this);
        start.Click += async (_, _) => await StartAsync();
        stop.Click += async (_, _) => await StopAsync();
        pause.Click += (_, _) => { paused = !paused; pause.Text = paused ? "Resume display" : "Pause display"; if (!paused) RefreshGrid(); };
        clear.Click += (_, _) => { history.Clear(); RefreshGrid(); };
        search.TextChanged += (_, _) => RefreshGrid();
        grid.SelectionChanged += (_, _) => ShowSelection();
        timer.Tick += async (_, _) => await PollAsync();
        FormClosing += OnClosing;
        FormClosed += (_, _) => timer.Dispose();
    }

    private void Buttons()
    {
        start.Enabled = !busy && !closing && session == null;
        stop.Enabled = !busy && !closing && session != null;
        pause.Enabled = session != null && !closing;
    }

    private async Task StartAsync()
    {
        if (busy || session != null) return;
        if (MessageBox.Show(this, "Start database-filtered tracing? SQL statements may include confidential values. Use short sessions, preferably on development servers.", "Live monitoring", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) != DialogResult.OK) return;
        busy = true; Buttons(); status.Text = "Starting Extended Events...";
        session = new LiveActivitySession(connection, database);
        try
        {
            await session.StartAsync(); history.Clear(); stacks.Clear(); warning = ""; paused = false;
            pause.Text = "Pause display"; RefreshGrid(); timer.Start(); status.Text = "Running: " + session.SessionName;
        }
        catch (Exception ex)
        {
            status.Text = "Start failed: " + ex.Message;
            try { await session.StopAsync(); session = null; }
            catch (Exception cleanup) { status.Text += " | Cleanup failed: " + cleanup.Message; }
        }
        finally { busy = false; Buttons(); }
    }

    private async Task PollAsync()
    {
        if (busy || session == null || closing) return;
        busy = true; Buttons();
        try
        {
            var batch = await session.ReadAsync();
            if (batch.Warning.Length != 0) { warning = batch.Warning; stacks.Clear(); }
            foreach (var e in batch.Events)
            {
                if (!stacks.TryGetValue(e.SessionId, out var stack)) stacks[e.SessionId] = stack = new();
                if (e.Event == "module_start") stack.Add((e.ObjectId, e.Module));
                string module = e.Module.Length > 0 ? e.Module : stack.LastOrDefault().Name ?? "";
                // For ad-hoc statements, do not guess a procedure from an unrelated concurrent request.
                if (e.Event == "sql_statement_completed" && e.ObjectId == 0) module = "";
                var ops = e.Event is "sp_statement_completed" or "sql_statement_completed"
                    ? LiveStatementAnalysis.Analyze(e.Statement) : Array.Empty<LiveTableOperation>();
                history.Add(new Row(e, module, ops));
                if (e.Event == "module_end")
                {
                    int i = stack.FindLastIndex(x => x.Id == e.ObjectId);
                    if (i >= 0) stack.RemoveRange(i, stack.Count - i);
                }
                if (e.Event == "rpc_completed") stack.Clear();
            }
            if (history.Count > 5000) history.RemoveRange(0, history.Count - 5000);
            if (!paused && batch.Events.Count > 0) RefreshGrid();
            status.Text = $"{(paused ? "Display paused (still capturing)" : "Running")} | {history.Count:N0} retained events | {session.SessionName}" + (warning.Length == 0 ? "" : " | " + warning);
        }
        catch (Exception ex) { timer.Stop(); status.Text = "Capture interrupted: " + ex.Message + " | Click Stop to clean up the session."; }
        finally { busy = false; Buttons(); }
    }

    private async Task StopAsync()
    {
        if (busy || session == null) return;
        timer.Stop(); busy = true; Buttons(); status.Text = "Stopping...";
        try { await session.StopAsync(); session = null; status.Text = "Stopped. Captured events remain visible." + (warning.Length > 0 ? " | " + warning : ""); }
        catch (Exception ex) { status.Text = "Cleanup failed: " + session.SessionName + " | " + ex.Message + " | Retry Stop."; }
        finally { busy = false; Buttons(); }
    }

    private void RefreshGrid()
    {
        long? selected = (grid.CurrentRow?.Tag as Row)?.Event.Sequence;
        grid.SuspendLayout();
        try
        {
            grid.Rows.Clear(); string term = search.Text.Trim();
            foreach (var row in history.AsEnumerable().Reverse())
            {
                var e = row.Event; string ops = string.Join(", ", row.Operations.Select(o => o.Operation + " " + o.Table));
                if (term.Length > 0 && !string.Join(" ", row.Module, e.Event, e.Statement, ops, e.SessionId.ToString()).Contains(term, StringComparison.OrdinalIgnoreCase)) continue;
                int index = grid.Rows.Add(e.Timestamp.LocalDateTime.ToString("HH:mm:ss.fff"), row.Module, e.Event, e.SessionId, ops,
                    e.DurationUs.HasValue ? (e.DurationUs.Value / 1000d).ToString("0.###") : "", e.RowCount?.ToString() ?? "");
                grid.Rows[index].Tag = row;
            }
            if (selected.HasValue)
                foreach (DataGridViewRow item in grid.Rows)
                    if ((item.Tag as Row)?.Event.Sequence == selected) { grid.CurrentCell = item.Cells[0]; break; }
        }
        finally { grid.ResumeLayout(); ShowSelection(); }
    }

    private void ShowSelection()
    {
        flow.Nodes.Clear(); detail.Clear();
        if (grid.CurrentRow?.Tag is not Row row) return;
        var e = row.Event;
        detail.Text = $"UTC: {e.Timestamp:O}\nEvent: {e.Event}\nSession: {e.SessionId}\nModule: {row.Module}\nApplication: {e.Application}\nLogin: {e.Login}\nDuration (microseconds): {e.DurationUs}\nRows: {e.RowCount}\n\n{e.Statement}";
        var root = flow.Nodes.Add(row.Module.Length == 0 ? "Ad-hoc / unknown module" : row.Module);
        var statement = root.Nodes.Add(e.Event);
        foreach (var op in row.Operations) statement.Nodes.Add(op.Operation + " → " + op.Table);
        if (row.Operations.Count == 0) statement.Nodes.Add("No direct named table reference identified");
        root.ExpandAll();
    }

    private async void OnClosing(object? sender, FormClosingEventArgs e)
    {
        if (allowClose) return;
        e.Cancel = true; if (closing) return;
        closing = true; timer.Stop(); Buttons();
        while (busy) await Task.Delay(100);
        await StopAsync();
        if (session != null)
        {
            closing = false; Buttons();
            MessageBox.Show(this, "Could not remove " + session.SessionName + ". Retry Stop after restoring connectivity.", "Cleanup required");
            return;
        }
        allowClose = true; Close();
    }
}
