using Microsoft.Data.SqlClient;


namespace SQL_Helper
{
    public partial class frmConnectSQL : Form
    {
        public frmConnectSQL()
        {
            InitializeComponent();
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (Validation())
            {
                string server = txtServer.Text.Trim();
                string trustCert = chkTrustedCertificate.Checked ? "True" : "False";
                string connectionString;

                if (cmbAuthentication.SelectedItem?.ToString()?.ToUpper() == "WINDOW AUTHENTICATION")
                {
                    connectionString = $"Data Source={server};Integrated Security=True;TrustServerCertificate={trustCert};";
                }
                else // SQL Server Authentication
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

                        this.Hide(); // 👈 Hide or use this.Close(); depending on your need
                        MainDashboard db = new MainDashboard();
                        db.ShowDialog(); // Use ShowDialog() if this is the main flow
                        this.Close();    // Ensures full close after dashboard is done
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Connection Failed:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }


        private void cmbAuthentication_SelectedIndexChanged(object sender, EventArgs e)
        {
           
            if (string.Equals(cmbAuthentication.SelectedItem?.ToString(), "SQL SERVER AUTHENTICATION", StringComparison.OrdinalIgnoreCase))
            {
                txtUserName.Text = txtPassword.Text = string.Empty;
                txtUserName.ReadOnly = txtPassword.ReadOnly = false;
            }
            else
            {
                txtUserName.Text = txtPassword.Text = string.Empty;
                txtUserName.ReadOnly = txtPassword.ReadOnly = true;
            }
        }

        private bool Validation()
        {
            if (string.IsNullOrEmpty(txtServer.Text))
            {
                MessageBox.Show("Please Enter Server Name ", "Required", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtServer.Focus();
                return false;
            }
            else if (string.IsNullOrEmpty(cmbAuthentication.SelectedItem?.ToString()))
            {
                MessageBox.Show("Please Select Authentication Type ", "Required", MessageBoxButtons.OK, MessageBoxIcon.Error);
                cmbAuthentication.Focus();
                return false;
            }
            
            else if (string.Equals(cmbAuthentication.SelectedItem?.ToString(), "SQL Server Authentication", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrEmpty(txtUserName.Text))
                {
                    MessageBox.Show("Please Enter User Name ", "Required", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtUserName.Focus();
                    return false;
                }
                else if (string.IsNullOrEmpty(txtPassword.Text))
                {
                    MessageBox.Show("Please Enter Password ", "Required", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtPassword.Focus();
                    return false;
                }


            }

            return true;
        }
    }
}
