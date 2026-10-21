namespace SQL_Helper;

/// <summary>Shared desktop design system. Existing controls retain their data and event handlers.</summary>
internal static class AppUi
{
    internal static readonly Color Ink = Color.FromArgb(25, 42, 66);
    internal static readonly Color Accent = Color.FromArgb(35, 91, 181);
    internal static readonly Color Canvas = Color.FromArgb(244, 247, 251);

    internal static void Apply(Form form)
    {
        var controls = Descendants(form).Where(c => !string.IsNullOrEmpty(c.Name)).GroupBy(c => c.Name).ToDictionary(g => g.Key, g => g.First());
        Control Get(string name) => controls.TryGetValue(name, out var c) ? c : throw new InvalidOperationException($"Missing UI control: {name}");
        Control Field(string caption, string name) => MakeField(caption, Get(name));
        Control Actions(params string[] names) => Wrap(names.Select(Get), 130);
        Control Fields(params (string Label, string Name)[] fields) => Wrap(fields.Select(f => Field(f.Label, f.Name)), 260);
        string title; string subtitle; Control body; Control? filters = null; Control? footer = null;
        switch (form)
        {
            case frmParameters:
                title = "Procedure parameters"; subtitle = "Enter values for the stored procedure parameters.";
                body = Get("flowLayoutPanel1"); footer = Actions("confirmParameters"); break;
            case frmConnectSQL:
                title = "Connect to SQL Server"; subtitle = "Choose a server and sign in to start exploring.";
                body = Fields(("Server", "cmbServer"), ("Authentication", "cmbAuthentication"), ("Username", "txtUserName"), ("Password", "txtPassword"));
                footer = Wrap(new[] { Get("chkTrustedCertificate"), Get("lblLinkConnections"), Get("btnConnect") }, 170);
                form.AcceptButton = (Button)Get("btnConnect"); break;
            case frmCompareDBConnection:
                title = "Connect comparison servers"; subtitle = "Set up development and production connections.";
                Control Server(string heading, bool target)
                {
                    string prefix = target ? "Target" : "Compare";
                    var fields = Fields(("Server", $"cmb{prefix}Server"), ("Authentication", target ? "cmbTargetAuthentication" : "txtCompareAuthType"), ("Username", $"txt{prefix}UserName"), ("Password", $"txt{prefix}Password"));
                    return new ServerCard(heading, fields, Get(target ? "chkTrustedCertificate" : "checkBox1"));
                }
                body = Wrap(new[] { Server("Development", true), Server("Production", false) }, 310); footer = Actions("btnConnect"); break;
            case frmManageConnections:
                title = "Saved connections"; subtitle = "Select a connection to edit its details or create a new one.";
                filters = Fields(("Server", "txtServer"), ("Authentication", "cmbAuthentication"), ("Username", "txtUserName"), ("Password", "txtPassword"));
                body = Get("dataGridViewDetail"); footer = Wrap(new[] { Get("chkTrustedCertificate"), Get("btnConnect"), Get("btnDelete") }, 160); break;
            case frmSelectCompareDatabase:
                title = "Choose databases"; subtitle = "Select the databases you want to compare.";
                body = Fields(("Development database", "comboBox1"), ("Production database", "comboBox2")); footer = Actions("button1"); break;
            case frmComparisionDatabase:
                title = "Database comparison"; subtitle = "Inspect schema differences and export the results.";
                filters = Wrap(new[] { Field("Search differences", "textBox1"), Get("button1"), Get("btnGenereateScript"), Get("btnExportExcel") }, 180);
                body = Get("panel1"); break;
            case frmTableSearching:
                title = "Find tables"; subtitle = "Search table names across your connected databases.";
                filters = Wrap(new[] { Field("Search tables", "textBox1"), Get("button1") }, 230); body = Get("dataGridView1"); break;
            case frmStoreProcedureTracking:
                title = "Track stored procedures"; subtitle = "Choose databases and find the procedure you need.";
                filters = Wrap(new[] { Field("Search procedures", "textBox1"), Get("button1") }, 230);
                body = AdaptiveSplit(Section("Databases", Get("checkedListBoxDatabses")), Get("dataGridView1")); break;
            case ViewData:
                title = "Data preview"; subtitle = "Browse the first 100 table rows or procedure results. Export the filtered data.";
                filters = Wrap(new[] { Field("Search all columns", "textBox1"), Get("BtnExportCSV"), Get("btnExportExcel") }, 200); body = Get("dataGridView1"); break;
            case frmSpHelpText:
                title = "Procedure definition"; subtitle = "Read SQL and inspect the related data and tables.";
                filters = Actions("button1", "button2"); body = Get("richTextBox1"); break;
            case frmGenerateScript:
                title = "Generated SQL script"; subtitle = "Review the script before using it in your database.";
                body = Get("richTextBox1"); break;
            case frmDummyData:
                title = "Generate sample data"; subtitle = "Choose a table, configure its columns, and set the number of rows.";
                filters = Wrap(new[] { Field("Database", "cmbDatabaes"), Field("Table", "cmbTable"), Get("btnViewSchema") }, 220);
                body = Get("pnlSchema"); ((Panel)body).AutoScroll = true; body.ControlAdded += (_, e) => { if (e.Control != null) StyleTree(e.Control); };
                footer = Wrap(new[] { Field("Rows to generate", "txtTotalRows"), Get("btnFeedData") }, 220);
                Get("btnFeedData").Text = "Insert sample data"; break;
            case frm_Sp_Xray:
                title = "Procedure X-Ray"; subtitle = "Inspect variables, triggers, and referenced tables.";
                filters = Wrap(new[] { Field("Stored procedure", "textBox1"), Get("button1") }, 260);
                body = Wrap(new[] { Section("Variables", Get("listView3")), Section("Triggers", Get("listView1")), Section("Referenced tables", Get("listView2")) }, 230, true);
                footer = Get("checkBox1"); footer.Text = "Uses an HTTP API"; break;
            default: return;
        }
        form.SuspendLayout();
        // Detach reused controls before removing the old designer containers.
        var host = new TableLayoutPanel { Dock = DockStyle.None, ColumnCount = 1, RowCount = 4, Padding = new Padding(20), MinimumSize = new Size(340, 420), BackColor = Canvas };
        host.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        host.RowStyles.Add(new RowStyle(SizeType.AutoSize)); host.RowStyles.Add(new RowStyle(SizeType.AutoSize)); host.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); host.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        var heading = new TableLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, ColumnCount = 1, RowCount = 2, Margin = new Padding(0, 0, 0, 16) };
        heading.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        heading.RowStyles.Add(new RowStyle(SizeType.AutoSize)); heading.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        heading.Controls.Add(new Label { Text = title, AutoSize = true, Font = new Font("Segoe UI", 20, FontStyle.Bold), ForeColor = Ink, Margin = new Padding(0, 0, 0, 6) });
        heading.Controls.Add(new Label { Text = subtitle, AutoSize = true, ForeColor = Color.FromArgb(88, 105, 126), Margin = Padding.Empty });
        heading.SizeChanged += (_, _) => { foreach (Control label in heading.Controls) label.MaximumSize = new Size(Math.Max(200, heading.ClientSize.Width - 10), 0); };
        host.Controls.Add(heading, 0, 0);
        if (filters != null) host.Controls.Add(filters, 0, 1);
        body.Dock = DockStyle.Fill; body.Margin = new Padding(0, 0, 0, 12); host.Controls.Add(body, 0, 2);
        if (footer != null) { footer.Dock = DockStyle.Fill; host.Controls.Add(footer, 0, 3); }
        form.Controls.Clear();
        var viewport = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Canvas }; viewport.Controls.Add(host); form.Controls.Add(viewport);
        bool layingOut = false;
        void LayoutViewport()
        {
            if (layingOut) return;
            layingOut = true;
            try
            {
                int width = Math.Max(340, viewport.ClientSize.Width - SystemInformation.VerticalScrollBarWidth);
                int inner = width - 40;
                foreach (Control label in heading.Controls) label.MaximumSize = new Size(Math.Max(180, inner - 10), 0);
                int headingHeight = heading.GetPreferredSize(new Size(inner, 0)).Height + 16;
                int filterHeight = host.GetControlFromPosition(0, 1)?.GetPreferredSize(new Size(inner, 0)).Height ?? 0;
                int footerHeight = (footer?.GetPreferredSize(new Size(inner, 0)).Height ?? 0) + (footer == null ? 0 : 12);
                int contentHeight = body is ResponsiveCards { FillsHeight: false } ? body.GetPreferredSize(new Size(inner, 0)).Height : 180;
                host.RowStyles[0].SizeType = SizeType.Absolute; host.RowStyles[0].Height = headingHeight;
                host.RowStyles[1].SizeType = SizeType.Absolute; host.RowStyles[1].Height = filterHeight;
                host.RowStyles[3].SizeType = SizeType.Absolute; host.RowStyles[3].Height = footerHeight;
                int needed = headingHeight + filterHeight + footerHeight + contentHeight + 64;
                host.Size = new Size(width, Math.Max(viewport.ClientSize.Height, needed));
            }
            finally { layingOut = false; }
        }
        viewport.Layout += (_, _) => LayoutViewport();
        form.Shown += (_, _) => LayoutViewport();
        LayoutViewport();
        form.Text = title + " | SQL Helper"; form.Font = new Font("Segoe UI", 10); form.BackColor = Canvas;
        form.WindowState = FormWindowState.Normal; form.FormBorderStyle = FormBorderStyle.Sizable; form.MaximizeBox = true; form.MinimizeBox = true;
        form.MinimumSize = new Size(380, 480); form.StartPosition = FormStartPosition.CenterParent;
        form.ClientSize = new Size(form is frmConnectSQL or frmSelectCompareDatabase ? 620 : 1050, form is frmConnectSQL or frmSelectCompareDatabase ? 600 : 720);
        form.Load += (_, _) => FitToScreen(form);
        StyleTree(form);
        foreach (var name in new[] { "btnConnect", "btnFeedData", "confirmParameters" })
            if (controls.TryGetValue(name, out var primary) && primary is Button button)
            { button.BackColor = Accent; button.ForeColor = Color.White; button.FlatAppearance.BorderColor = Accent; }
        form.ResumeLayout(true);
    }

    internal static void FitToScreen(Form form)
    {
        var area = Screen.FromControl(form.Owner ?? form).WorkingArea;
        form.MinimumSize = new Size(Math.Min(form.MinimumSize.Width, area.Width), Math.Min(form.MinimumSize.Height, area.Height));
        form.Size = new Size(Math.Min(form.Width, area.Width), Math.Min(form.Height, area.Height));
        form.Location = new Point(area.Left + Math.Max(0, (area.Width - form.Width) / 2), area.Top + Math.Max(0, (area.Height - form.Height) / 2));
    }

    private static IEnumerable<Control> Descendants(Control parent)
    {
        foreach (Control child in parent.Controls) { yield return child; foreach (var nested in Descendants(child)) yield return nested; }
    }

    internal static Control MakeField(string caption, Control input)
    {
        var field = new TableLayoutPanel { ColumnCount = 1, RowCount = 2, Height = 68, Margin = new Padding(0, 0, 12, 12), Padding = Padding.Empty, BackColor = Canvas };
        field.RowStyles.Add(new RowStyle(SizeType.Absolute, 25)); field.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        field.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        field.Controls.Add(new Label { Text = caption, AutoSize = true, ForeColor = Ink, Margin = new Padding(0, 0, 0, 6) }, 0, 0);
        input.Dock = DockStyle.Top; input.Margin = Padding.Empty;
        input.TabIndex = 1; field.Controls.Add(input, 0, 1);
        input.AccessibleName = caption;
        return field;
    }
    private static Control Section(string heading, Control content)
    {
        var panel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, BackColor = Color.White, Padding = new Padding(12), Margin = new Padding(0, 0, 12, 12) };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 34)); panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        panel.Controls.Add(new Label { Text = heading, AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Ink }, 0, 0);
        content.Dock = DockStyle.Fill; panel.Controls.Add(content, 0, 1);
        if (content is not ListView && content is not CheckedListBox) { panel.AutoSize = true; panel.AutoSizeMode = AutoSizeMode.GrowAndShrink; panel.RowStyles[1] = new RowStyle(SizeType.AutoSize); }
        return panel;
    }
    private static Control Wrap(IEnumerable<Control> controls, int preferredWidth, bool fillHeight = false) => new ResponsiveCards(controls, preferredWidth, fillHeight);

    private static Control AdaptiveSplit(Control left, Control right)
    {
        var split = new SplitContainer { Size = new Size(950, 450), Dock = DockStyle.Fill, SplitterDistance = 240, FixedPanel = FixedPanel.Panel1 };
        split.Panel1.Controls.Add(left); split.Panel2.Controls.Add(right); left.Dock = right.Dock = DockStyle.Fill;
        split.SizeChanged += (_, _) =>
        {
            bool narrow = split.Width < 640;
            var orientation = narrow ? Orientation.Horizontal : Orientation.Vertical;
            if (split.Orientation != orientation) split.Orientation = orientation;
            int length = narrow ? split.Height : split.Width;
            if (length > 100) split.SplitterDistance = Math.Min(narrow ? 150 : 240, length / 2);
        };
        return split;
    }

    internal static void StyleTree(Control control)
    {
        foreach (Control child in control.Controls) StyleTree(child);
        if (control is Panel && control.BackColor == SystemColors.ActiveCaption) control.BackColor = Canvas;
        if (control is FlowLayoutPanel) control.BackColor = Canvas;
        if (control is Button button)
        {
            button.AutoSize = true; button.MinimumSize = new Size(110, 36); button.Padding = new Padding(10, 3, 10, 3);
            button.FlatStyle = FlatStyle.Flat; button.FlatAppearance.BorderColor = Color.FromArgb(197, 210, 227); button.BackColor = Color.White; button.ForeColor = Ink;
            button.Cursor = Cursors.Hand; button.UseVisualStyleBackColor = false;
            button.AccessibleName = button.Text.Replace("&", "");
        }
        if (control is DataGridView grid)
        {
            grid.BackgroundColor = Color.White; grid.BorderStyle = BorderStyle.None; grid.RowHeadersVisible = false; grid.EnableHeadersVisualStyles = false;
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(231, 237, 245); grid.ColumnHeadersDefaultCellStyle.ForeColor = Ink;
            grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = grid.ColumnHeadersDefaultCellStyle.BackColor;
            grid.ColumnHeadersHeight = 38; grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(218, 233, 255); grid.DefaultCellStyle.SelectionForeColor = Ink;
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(247, 249, 252); grid.RowTemplate.Height = 30;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells; grid.AllowUserToAddRows = false; grid.ReadOnly = true;
        }
        if (control is RichTextBox editor) { editor.Font = new Font("Consolas", 11); editor.BackColor = Color.White; editor.ForeColor = Ink; editor.WordWrap = false; editor.BorderStyle = BorderStyle.None; }
        if (control is ListView list) { list.BackColor = Color.White; list.ForeColor = Ink; list.BorderStyle = BorderStyle.None; }
        if (control is TextBox text) text.BorderStyle = BorderStyle.FixedSingle;
    }

    private sealed class ServerCard : Panel
    {
        private readonly Control fields;
        private readonly Control trust;
        internal ServerCard(string title, Control fields, Control trust)
        {
            this.fields = fields; this.trust = trust; BackColor = Color.White;
            Controls.Add(new Label { Text = title, AutoSize = true, Location = new Point(12, 12), Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Ink });
            fields.Dock = trust.Dock = DockStyle.None; Controls.Add(fields); Controls.Add(trust);
        }
        public override Size GetPreferredSize(Size proposedSize)
        {
            int width = Math.Max(200, proposedSize.Width > 0 ? proposedSize.Width : Width);
            return new Size(width, 46 + fields.GetPreferredSize(new Size(width - 24, 0)).Height + 44);
        }
        protected override void OnLayout(LayoutEventArgs e)
        {
            base.OnLayout(e);
            if (fields == null) return;
            int height = fields.GetPreferredSize(new Size(Math.Max(100, Width - 24), 0)).Height;
            fields.SetBounds(12, 46, Math.Max(100, Width - 24), height);
            trust.SetBounds(12, height + 46, Math.Max(100, Width - 24), 28);
        }
    }
    /// <summary>Reflows cards into fewer columns as the available width shrinks.</summary>
    private sealed class ResponsiveCards : Panel
    {
        private readonly int preferredWidth;
        private readonly bool fillHeight;
        internal bool FillsHeight => fillHeight;
        private bool arranging;
        internal ResponsiveCards(IEnumerable<Control> items, int preferredWidth, bool fillHeight)
        {
            this.preferredWidth = preferredWidth; this.fillHeight = fillHeight;
            Dock = DockStyle.Top; BackColor = Canvas; Margin = Padding.Empty; AutoScroll = fillHeight;
            foreach (var item in items.ToArray()) { item.Dock = DockStyle.None; item.Anchor = AnchorStyles.Top | AnchorStyles.Left; Controls.Add(item); }
            SizeChanged += (_, _) => Arrange();
            foreach (Control item in Controls) item.SizeChanged += (_, _) => Arrange();
            Arrange();
        }
        public override Size GetPreferredSize(Size proposedSize)
        {
            int available = Math.Max(80, proposedSize.Width > 0 ? proposedSize.Width : Width);
            int columns = Math.Max(1, Math.Min(Controls.Count, available / preferredWidth));
            int width = Math.Max(80, (available - 12 * (columns - 1)) / columns);
            int total = 0;
            for (int i = 0; i < Controls.Count; i += columns)
            {
                int height = 0;
                for (int j = i; j < Math.Min(i + columns, Controls.Count); j++)
                    height = Math.Max(height, Math.Max(Controls[j] is Button ? 38 : 32, Controls[j].GetPreferredSize(new Size(width, 0)).Height));
                total += height + 12;
            }
            return new Size(available, total);
        }
        protected override void OnLayout(LayoutEventArgs e) { base.OnLayout(e); Arrange(); }
        private void Arrange()
        {
            if (arranging || ClientSize.Width <= 0) return;
            arranging = true;
            try
            {
                int available = Math.Max(80, ClientSize.Width - (fillHeight ? SystemInformation.VerticalScrollBarWidth : 0));
                int columns = Math.Max(1, Math.Min(Controls.Count, available / preferredWidth));
                int width = Math.Max(80, (available - 12 * (columns - 1)) / columns);
                int y = 0;
                for (int start = 0; start < Controls.Count; start += columns)
                {
                    int height = 0;
                    for (int j = start; j < Math.Min(start + columns, Controls.Count); j++)
                    {
                        var item = Controls[j]; item.Width = width; item.PerformLayout();
                        int h = fillHeight ? Math.Max(180, (ClientSize.Height - 12) / (int)Math.Ceiling((double)Controls.Count / columns)) : item.GetPreferredSize(new Size(width, 0)).Height;
                        item.Height = Math.Max(item is Button ? 38 : 32, h); item.Location = new Point((j - start) * (width + 12), y); height = Math.Max(height, item.Height);
                    }
                    y += height + 12;
                }
                if (!fillHeight) { Height = y; Parent?.PerformLayout(); }
                else AutoScrollMinSize = new Size(0, y);
            }
            finally { arranging = false; }
        }
    }
}
