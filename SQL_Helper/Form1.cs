using Microsoft.Data.SqlClient;

namespace SQL_Helper;

public partial class frmConnectSQL : Form
{
    public frmConnectSQL() { InitializeComponent(); AppUi.Apply(this); }
    private void frmConnectSQL_Load(object? sender, EventArgs e)
    {
        try { cmbAuthentication.SelectedIndex = 0; cmbServer.Items.Clear(); foreach (var profile in ConnectionStore.Load()) cmbServer.Items.Add(profile); }
        catch (Exception ex) { SqlTools.Error(this, ex); }
    }
    private async void btnConnect_Click(object sender, EventArgs e)
    {
        btnConnect.Enabled = false;
        try
        {
            var profile = new ConnectionProfile(cmbServer.Text, cmbAuthentication.Text, txtUserName.Text, txtPassword.Text, chkTrustedCertificate.Checked).Normalized();
            using var connection = new SqlConnection(profile.ConnectionString); await connection.OpenAsync();
            ConnectionStore.Save(profile); DbConnectionHelper.ConnectionString = profile.ConnectionString; DbConnectionHelper.SelectedDatabase = null; DialogResult = DialogResult.OK;
        }
        catch (Exception ex) { SqlTools.Error(this, ex); }
        finally { if (!IsDisposed) btnConnect.Enabled = true; }
    }
    private void cmbAuthentication_SelectedIndexChanged(object sender, EventArgs e)
    {
        bool windows = cmbAuthentication.Text == "WINDOW AUTHENTICATION";
        txtUserName.Enabled = txtPassword.Enabled = !windows;
    }
    private void cmbServer_SelectedIndexChanged_1(object sender, EventArgs e)
    {
        if (cmbServer.SelectedItem is not ConnectionProfile profile) return;
        cmbAuthentication.SelectedItem = profile.Authentication; txtUserName.Text = profile.UserName; txtPassword.Text = profile.Password; chkTrustedCertificate.Checked = profile.TrustCertificate;
    }
    private void lblLinkConnections_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) { using var form = new frmManageConnections(); form.ShowDialog(this); frmConnectSQL_Load(sender, e); }
}
