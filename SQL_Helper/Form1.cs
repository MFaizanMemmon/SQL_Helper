using System;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;

namespace SQL_Helper
{
    public partial class frmConnectSQL : Form
    {
        public frmConnectSQL()
        {
            InitializeComponent();
        }

        private void frmConnectSQL_Load(object sender, EventArgs e)
        {

            cmbAuthentication.SelectedIndex = 0;

            // Load saved server names
            var connections = Properties.Settings.Default.SavedConnections ?? new StringCollection();
            foreach (string conn in connections)
            {
                string[] parts = conn.Split('|');
                if (parts.Length > 0 && !cmbServer.Items.Contains(parts[0]))
                {
                    cmbServer.Items.Add(parts[0]); // Just add Server Name
                }
            }
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (!Validation()) return;

            string server = cmbServer.Text.Trim();
            string trustCert = chkTrustedCertificate.Checked ? "True" : "False";
            string connectionString;

            if (cmbAuthentication.SelectedItem?.ToString()?.ToUpper() == "WINDOW AUTHENTICATION")
            {
                connectionString = $"Data Source={server};Integrated Security=True;TrustServerCertificate={trustCert};";
            }
            else
            {
                string username = txtUserName.Text.Trim();
                string password = txtPassword.Text.Trim();
                connectionString = $"Data Source={server};User ID={username};Password={password};TrustServerCertificate={trustCert};";
            }

            DbConnectionHelper.ConnectionString = connectionString;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    MessageBox.Show("Connected Successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Save connection if not exists
                    string fullConn = $"{server}|{cmbAuthentication.SelectedItem}|{txtUserName.Text}|{txtPassword.Text}";
                    var savedConnections = Properties.Settings.Default.SavedConnections ?? new StringCollection();
                    if (!savedConnections.Contains(fullConn))
                    {
                        savedConnections.Add(fullConn);
                        Properties.Settings.Default.SavedConnections = savedConnections;
                        Properties.Settings.Default.Save();
                    }

                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Connection Failed:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private bool Validation()
        {
            if (string.IsNullOrEmpty(cmbServer.Text))
            {
                MessageBox.Show("Please Enter or Select Server Name", "Required", MessageBoxButtons.OK, MessageBoxIcon.Error);
                cmbServer.Focus();
                return false;
            }
            else if (string.IsNullOrEmpty(cmbAuthentication.SelectedItem?.ToString()))
            {
                MessageBox.Show("Please Select Authentication Type", "Required", MessageBoxButtons.OK, MessageBoxIcon.Error);
                cmbAuthentication.Focus();
                return false;
            }
            else if (cmbAuthentication.SelectedItem.ToString() == "SQL Server Authentication")
            {
                if (string.IsNullOrEmpty(txtUserName.Text))
                {
                    MessageBox.Show("Please Enter User Name", "Required", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtUserName.Focus();
                    return false;
                }
                else if (string.IsNullOrEmpty(txtPassword.Text))
                {
                    MessageBox.Show("Please Enter Password", "Required", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtPassword.Focus();
                    return false;
                }
            }

            return true;
        }

        private void cmbAuthentication_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool isWindows = cmbAuthentication.SelectedItem?.ToString() == "WINDOW AUTHENTICATION";
            txtUserName.ReadOnly = txtPassword.ReadOnly = isWindows;

            if (isWindows)
            {
                txtUserName.Clear();
                txtPassword.Clear();
            }
        }

        private void cmbServer_SelectedIndexChanged(object sender, EventArgs e)
        {
            string conn = cmbServer.SelectedItem?.ToString();
            if (!string.IsNullOrEmpty(conn))
            {
                try
                {
                    var builder = new SqlConnectionStringBuilder(conn);
                    cmbServer.Text = builder.DataSource;

                    if (builder.IntegratedSecurity)
                    {
                        cmbAuthentication.SelectedIndex = 0;
                        txtUserName.Text = "";
                        txtPassword.Text = "";
                    }
                    else
                    {
                        cmbAuthentication.SelectedIndex = 1;
                        txtUserName.Text = builder.UserID;
                        txtPassword.Text = builder.Password;
                    }
                }
                catch
                {
                    MessageBox.Show("Invalid connection string format.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void cmbServer_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            string selectedServer = cmbServer.SelectedItem?.ToString();
            var connections = Properties.Settings.Default.SavedConnections ?? new StringCollection();

            foreach (string conn in connections)
            {
                string[] parts = conn.Split('|');
                if (parts.Length == 4 && parts[0] == selectedServer)
                {
                    cmbAuthentication.SelectedItem = parts[1];
                    txtUserName.Text = parts[2];
                    txtPassword.Text = parts[3];

                    txtUserName.ReadOnly = cmbAuthentication.SelectedItem.ToString() == "Windows Authentication";
                    txtPassword.ReadOnly = cmbAuthentication.SelectedItem.ToString() == "Windows Authentication";
                    break;
                }
            }
        }
    }


}
