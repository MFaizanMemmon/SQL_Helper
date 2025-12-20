using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SQL_Helper
{
    public partial class frmCompareDBConnection : Form
    {
        public frmCompareDBConnection()
        {
            InitializeComponent();
        }
        public bool IsConnectSuccessfull { get; set; } = false;
        private void cmbTargetAuthentication_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbTargetAuthentication.Text == "WINDOW AUTHENTICATION")
            {
                txtTargetUserName.Enabled = false;
                txtTargetPassword.Enabled = false;
            }
            else
            {
                txtTargetUserName.Enabled = true;
                txtTargetPassword.Enabled = true;
            }
        }


        private void txtCompareAuthType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (txtCompareAuthType.Text == "WINDOW AUTHENTICATION")
            {
                txtCompareUserName.Enabled = false;
                txtComparePassword.Enabled = false;
            }
            else
            {
                txtCompareUserName.Enabled = true;
                txtComparePassword.Enabled = true;
            }
        }

        public bool TryCreateConnectionString(string server, string database, string username, string password, string authType, bool trustedCert, out string connectionString)
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder
            {
                DataSource = server,
                InitialCatalog = database,
                TrustServerCertificate = trustedCert
            };

            if (authType == "WINDOW AUTHENTICATION")
            {
                builder.IntegratedSecurity = true;
            }
            else
            {
                builder.UserID = username;
                builder.Password = password;
                builder.IntegratedSecurity = false;
            }

            connectionString = builder.ConnectionString;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    conn.Close();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            string targetConnStr, compareConnStr;

            bool targetOk = TryCreateConnectionString(
               cmbTargetServer.SelectedItem?.ToString(),
                "",
                txtTargetUserName.Text,
                txtTargetPassword.Text,
                cmbTargetAuthentication.Text,
                chkTrustedCertificate.Checked,
                out targetConnStr
            );

            bool compareOk = TryCreateConnectionString(
               cmbCompareServer.SelectedItem?.ToString(),
                "",
                txtCompareUserName.Text,
                txtComparePassword.Text,
                txtCompareAuthType.Text,
                checkBox1.Checked,
                out compareConnStr
            );

            if (!targetOk)
            {
                MessageBox.Show("Failed to connect to Target database.");
                return;
            }

            if (!compareOk)
            {
                MessageBox.Show("Failed to connect to Compare database.");
                return;
            }

            // Assign to static class
            DbConnectionHelper.TargetConnectionString = targetConnStr;
            DbConnectionHelper.CompareToConnectionString = compareConnStr;
            IsConnectSuccessfull = true;
            this.Close();

        }

        private void frmCompareDBConnection_Load(object sender, EventArgs e)
        {


            var connections = Properties.Settings.Default.SavedConnections ?? new StringCollection();

            for (int i = 0; i < connections.Count; i++)
            {
                string conn = connections[i];
                if (string.IsNullOrWhiteSpace(conn)) continue;

                string[] parts = conn.Split('|');
                if (parts.Length < 2) continue;

                // Add each connection WITH its index
                cmbCompareServer.Items.Add(new ServerItem
                {
                    Index = i,
                    Server = parts[0]
                });

                cmbTargetServer.Items.Add(new ServerItem
                {
                    Index = i,
                    Server = parts[0]
                });
            }
        }

        private void cmbTargetServer_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbTargetServer.SelectedItem is not ServerItem item)
                return;

            var connections = Properties.Settings.Default.SavedConnections;
            if (connections == null || item.Index >= connections.Count)
                return;

            string conn = connections[item.Index];
            string[] parts = conn.Split('|');

            string auth = parts.Length > 1 ? parts[1] : "";
            string user = parts.Length > 2 ? parts[2] : "";
            string pass = parts.Length > 3 ? parts[3] : "";

            cmbTargetAuthentication.SelectedItem = auth;
            txtTargetUserName.Text = user;
            txtTargetPassword.Text = pass;

            bool isWindowsAuth = auth == "Windows Authentication";
            txtTargetUserName.ReadOnly = isWindowsAuth;
            txtTargetPassword.ReadOnly = isWindowsAuth;

            if (isWindowsAuth)
            {
                txtTargetUserName.Clear();
                txtTargetPassword.Clear();
            }
        }

        private void cmbCompareServer_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbCompareServer.SelectedItem is not ServerItem item)
                return;

            var connections = Properties.Settings.Default.SavedConnections;
            if (connections == null || item.Index >= connections.Count)
                return;

            string conn = connections[item.Index];
            string[] parts = conn.Split('|');

            string auth = parts.Length > 1 ? parts[1] : "";
            string user = parts.Length > 2 ? parts[2] : "";
            string pass = parts.Length > 3 ? parts[3] : "";

            txtCompareAuthType.SelectedItem = auth;
            txtCompareUserName.Text = user;
            txtComparePassword.Text = pass;

            bool isWindowsAuth = auth == "Windows Authentication";
            txtCompareUserName.ReadOnly = isWindowsAuth;
            txtComparePassword.ReadOnly = isWindowsAuth;

            if (isWindowsAuth)
            {
                txtCompareUserName.Clear();
                txtComparePassword.Clear();
            }
        }
    }
}
