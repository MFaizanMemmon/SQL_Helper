using Microsoft.Data.SqlClient;
namespace SQL_Helper;
public partial class frmCompareDBConnection : Form
{
    public bool IsConnectSuccessfull { get; set; }
    public frmCompareDBConnection() { InitializeComponent(); AppUi.Apply(this); }
    private void frmCompareDBConnection_Load(object sender, EventArgs e)
    {
        try { cmbTargetAuthentication.SelectedIndex = txtCompareAuthType.SelectedIndex = 0; foreach (var p in ConnectionStore.Load()) { cmbTargetServer.Items.Add(p); cmbCompareServer.Items.Add(p); } }
        catch (Exception ex) { SqlTools.Error(this, ex); }
    }
    private void cmbTargetAuthentication_SelectedIndexChanged(object sender, EventArgs e) => txtTargetUserName.Enabled = txtTargetPassword.Enabled = cmbTargetAuthentication.Text != "WINDOW AUTHENTICATION";
    private void txtCompareAuthType_SelectedIndexChanged(object sender, EventArgs e) => txtCompareUserName.Enabled = txtComparePassword.Enabled = txtCompareAuthType.Text != "WINDOW AUTHENTICATION";
    private void cmbTargetServer_SelectedIndexChanged(object sender, EventArgs e) { if (cmbTargetServer.SelectedItem is ConnectionProfile p) { cmbTargetAuthentication.SelectedItem = p.Authentication; txtTargetUserName.Text = p.UserName; txtTargetPassword.Text = p.Password; chkTrustedCertificate.Checked = p.TrustCertificate; } }
    private void cmbCompareServer_SelectedIndexChanged(object sender, EventArgs e) { if (cmbCompareServer.SelectedItem is ConnectionProfile p) { txtCompareAuthType.SelectedItem = p.Authentication; txtCompareUserName.Text = p.UserName; txtComparePassword.Text = p.Password; checkBox1.Checked = p.TrustCertificate; } }
    private async void btnConnect_Click(object sender, EventArgs e)
    {
        btnConnect.Enabled = false;
        try
        {
            var development = new ConnectionProfile(cmbTargetServer.Text, cmbTargetAuthentication.Text, txtTargetUserName.Text, txtTargetPassword.Text, chkTrustedCertificate.Checked);
            var production = new ConnectionProfile(cmbCompareServer.Text, txtCompareAuthType.Text, txtCompareUserName.Text, txtComparePassword.Text, checkBox1.Checked);
            using var first = new SqlConnection(development.ConnectionString); await first.OpenAsync();
            using var second = new SqlConnection(production.ConnectionString); await second.OpenAsync();
            DbConnectionHelper.TargetConnectionString = development.ConnectionString; DbConnectionHelper.CompareToConnectionString = production.ConnectionString;
            IsConnectSuccessfull = true; DialogResult = DialogResult.OK;
        }
        catch (Exception ex) { SqlTools.Error(this, ex); }
        finally { if (!IsDisposed) btnConnect.Enabled = true; }
    }
}
