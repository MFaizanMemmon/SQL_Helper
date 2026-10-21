using Microsoft.Data.SqlClient;

namespace SQL_Helper;

public partial class frmManageConnections : Form
{
    private List<ConnectionProfile> profiles = new();
    private int? editingIndex;
    public frmManageConnections()
    {
        InitializeComponent(); AppUi.Apply(this);
        dataGridViewDetail.SelectionMode = DataGridViewSelectionMode.FullRowSelect; dataGridViewDetail.MultiSelect = false;
        dataGridViewDetail.SelectionChanged += (_, _) => PopulateEditor();
        cmbAuthentication.SelectedIndexChanged += (_, _) => { bool windows = cmbAuthentication.Text == "WINDOW AUTHENTICATION"; txtUserName.Enabled = txtPassword.Enabled = !windows; };
        var reset = new Button { Text = "New connection", AutoSize = true, Dock = DockStyle.Bottom };
        reset.Click += (_, _) => { editingIndex = null; dataGridViewDetail.ClearSelection(); txtServer.Clear(); txtUserName.Clear(); txtPassword.Clear(); txtServer.Focus(); };
        Controls.Add(reset); AppUi.StyleTree(reset);
    }
    private void frmManageConnections_Load(object sender, EventArgs e) => LoadConnectionsGrid();
    private void LoadConnectionsGrid()
    {
        try
        {
            profiles = ConnectionStore.Load();
            dataGridViewDetail.AutoGenerateColumns = false; dataGridViewDetail.Columns.Clear();
            foreach (var name in new[] { "Server", "Authentication", "UserName" }) dataGridViewDetail.Columns.Add(new DataGridViewTextBoxColumn { Name = name, HeaderText = name, DataPropertyName = name });
            dataGridViewDetail.DataSource = profiles; editingIndex = null; dataGridViewDetail.ClearSelection();
        }
        catch (Exception ex) { SqlTools.Error(this, ex); }
    }
    private void PopulateEditor()
    {
        if (dataGridViewDetail.CurrentRow?.DataBoundItem is not ConnectionProfile profile || !dataGridViewDetail.CurrentRow.Selected) return;
        editingIndex = dataGridViewDetail.CurrentRow.Index;
        txtServer.Text = profile.Server; cmbAuthentication.SelectedItem = profile.Authentication; txtUserName.Text = profile.UserName; txtPassword.Text = profile.Password; chkTrustedCertificate.Checked = profile.TrustCertificate;
    }
    private void btnDelete_Click(object sender, EventArgs e)
    {
        if (editingIndex is not int index) { MessageBox.Show(this, "Select a saved connection first."); return; }
        if (MessageBox.Show(this, "Delete the selected saved connection?", "Delete connection", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
        try { profiles.RemoveAt(index); ConnectionStore.Write(profiles); LoadConnectionsGrid(); }
        catch (Exception ex) { SqlTools.Error(this, ex); }
    }
    private async void btnConnect_Click(object sender, EventArgs e)
    {
        btnConnect.Enabled = false;
        try
        {
            var profile = new ConnectionProfile(txtServer.Text, cmbAuthentication.Text, txtUserName.Text, txtPassword.Text, chkTrustedCertificate.Checked).Normalized();
            using var connection = new SqlConnection(profile.ConnectionString); await connection.OpenAsync();
            ConnectionStore.Save(profile, editingIndex); LoadConnectionsGrid(); MessageBox.Show(this, "Connection tested and saved.");
        }
        catch (Exception ex) { SqlTools.Error(this, ex); }
        finally { if (!IsDisposed) btnConnect.Enabled = true; }
    }
}
