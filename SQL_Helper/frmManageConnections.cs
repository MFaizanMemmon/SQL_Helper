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
    public partial class frmManageConnections : Form
    {
        public frmManageConnections()
        {
            InitializeComponent();
        }

        private void frmManageConnections_Load(object sender, EventArgs e)
        {
            LoadConnectionsGrid();
        }

        private void LoadConnectionsGrid()
        {
            var connections = GetSavedConnections();

            dataGridViewDetail.AutoGenerateColumns = false;
            dataGridViewDetail.Columns.Clear();

            dataGridViewDetail.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Server",
                DataPropertyName = "Server"
            });

            dataGridViewDetail.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Authentication",
                DataPropertyName = "Authentication"
            });

            dataGridViewDetail.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "User Name",
                DataPropertyName = "UserName"
            });

            // ✅ PASSWORD COLUMN (VISIBLE)
            dataGridViewDetail.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Password",
                DataPropertyName = "Password",
                Visible = true
            });

            dataGridViewDetail.DataSource = connections;
        }


        private List<DbConnectionInfo> GetSavedConnections()
        {
            var list = new List<DbConnectionInfo>();

            var savedConnections = Properties.Settings.Default.SavedConnections;
            if (savedConnections == null) return list;

            foreach (string conn in savedConnections)
            {
                var parts = conn.Split('|');

                if (parts.Length == 4)
                {
                    list.Add(new DbConnectionInfo
                    {
                        Server = parts[0],
                        Authentication = parts[1],
                        UserName = parts[2],
                        Password = parts[3]
                    });
                }
            }

            return list;
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dataGridViewDetail.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a connection to delete.",
                                "No Selection",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
                return;
            }

            var result = MessageBox.Show(
                "Are you sure you want to delete the selected connection?",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
                return;

            // Get selected item
            var selectedConnection = dataGridViewDetail.SelectedRows[0].DataBoundItem as DbConnectionInfo;
            if (selectedConnection == null) return;

            // Rebuild saved list
            var savedConnections = Properties.Settings.Default.SavedConnections;
            if (savedConnections == null) return;

            string fullConn =
                $"{selectedConnection.Server}|{selectedConnection.Authentication}|{selectedConnection.UserName}|{selectedConnection.Password}";

            if (savedConnections.Contains(fullConn))
            {
                savedConnections.Remove(fullConn);
                Properties.Settings.Default.SavedConnections = savedConnections;
                Properties.Settings.Default.Save();
            }

            // Reload grid
            LoadConnectionsGrid();

            MessageBox.Show("Connection deleted successfully.",
                            "Deleted",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            string server = txtServer.Text.Trim();
            string auth = cmbAuthentication.SelectedItem?.ToString();
            string user = txtUserName.Text.Trim();
            string password = txtPassword.Text;

            if (string.IsNullOrEmpty(server) || string.IsNullOrEmpty(auth))
            {
                MessageBox.Show("Server and Authentication are required.",
                                "Validation",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
                return;
            }

            string connectionString;

            if (auth == "Windows")
            {
                connectionString = $"Server={server};Database=master;Trusted_Connection=True;";
            }
            else
            {
                if (string.IsNullOrEmpty(user))
                {
                    MessageBox.Show("User name is required for SQL authentication.",
                                    "Validation",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Warning);
                    return;
                }

                connectionString =
                    $"Server={server};Database=master;User Id={user};Password={password};";
            }

            // 1️⃣ TEST CONNECTION
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                try
                {
                    con.Open();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Connection failed:\n" + ex.Message,
                                    "Connection Error",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error);
                    return; // ❌ do not save
                }
            }

            // 2️⃣ SAVE ONLY IF CONNECTION SUCCESS
            string fullConn = $"{server}|{auth}|{user}|{password}";
            var savedConnections = Properties.Settings.Default.SavedConnections
                                    ?? new System.Collections.Specialized.StringCollection();

            if (!savedConnections.Contains(fullConn))
            {
                savedConnections.Add(fullConn);
                Properties.Settings.Default.SavedConnections = savedConnections;
                Properties.Settings.Default.Save();
            }

            MessageBox.Show("Connection successful and saved.",
                            "Success",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);

            // 3️⃣ Refresh grid
            LoadConnectionsGrid();
        }
    }

    public class DbConnectionInfo
    {
        public string Server { get; set; }
        public string Authentication { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; } // 👈 visible
    }
}
