using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
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
                txtTargetServer.Text,
                "",
                txtTargetUserName.Text,
                txtTargetPassword.Text,
                cmbTargetAuthentication.Text,
                chkTrustedCertificate.Checked,
                out targetConnStr
            );

            bool compareOk = TryCreateConnectionString(
                txtCompareServerName.Text,
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
    }
}
