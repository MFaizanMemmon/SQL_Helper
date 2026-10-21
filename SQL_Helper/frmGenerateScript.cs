namespace SQL_Helper;
public partial class frmGenerateScript : Form
{
    private SchemaSnapshot? source, target;
    private IReadOnlyList<SchemaDifference> differences = Array.Empty<SchemaDifference>();
    private readonly CheckBox destructive = new() { Text = "Include removal of production-only objects", AutoSize = true };
    public frmGenerateScript()
    {
        InitializeComponent(); AppUi.Apply(this); richTextBox1.ReadOnly = true;
        var actions = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill, WrapContents = true };
        var save = new Button { Text = "Save SQL…", AutoSize = true }; var copy = new Button { Text = "Copy SQL", AutoSize = true };
        save.Click += Save_Click; copy.Click += (_, _) => { try { if (richTextBox1.TextLength > 0) Clipboard.SetText(richTextBox1.Text); } catch (Exception ex) { SqlTools.Error(this, ex); } };
        destructive.CheckedChanged += (_, _) => Generate(); actions.Controls.Add(save); actions.Controls.Add(copy); actions.Controls.Add(destructive);
        actions.SizeChanged += (_, _) => destructive.MaximumSize = new Size(Math.Max(180, actions.ClientSize.Width - 12), 0);
        var root = (TableLayoutPanel)richTextBox1.Parent!; root.RowStyles[1] = new RowStyle(SizeType.Absolute, 90); root.Controls.Add(actions, 0, 1); AppUi.StyleTree(actions);
    }
    internal frmGenerateScript(SchemaSnapshot source, SchemaSnapshot target, IReadOnlyList<SchemaDifference> differences) : this() { this.source = source; this.target = target; this.differences = differences; }
    private void frmGenerateScript_Load(object sender, EventArgs e) => Generate();
    private void Generate()
    {
        if (source == null || target == null) { richTextBox1.Text = "-- Compare two databases first, then generate a migration."; return; }
        try { richTextBox1.Text = MigrationScript.Build(source, target, differences, destructive.Checked); }
        catch (Exception ex) { SqlTools.Error(this, ex); }
    }
    private async void Save_Click(object? sender, EventArgs e)
    {
        using var picker = new SaveFileDialog { Filter = "SQL script (*.sql)|*.sql", FileName = "database-migration.sql", AddExtension = true, DefaultExt = "sql" };
        if (picker.ShowDialog(this) != DialogResult.OK) return;
        try { await File.WriteAllTextAsync(picker.FileName, richTextBox1.Text, new System.Text.UTF8Encoding(true)); }
        catch (Exception ex) { SqlTools.Error(this, ex); }
    }
}
